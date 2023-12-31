﻿using Mono.Security.Cryptography;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WxAxW.PinAssistant.Utils
{
    [Serializable]
    public class LooseDictionary<TValue> where TValue : IComparable<TValue>
    {
        public class TraverseDetails
        {
            public string searchKey;
            public string originalKey;
            public TValue value;
            public TrieNode nodeResult;
            public string actualKey;
            public bool conflicting;
            public string blackListedWord;
            public bool deleteMode;
            public bool exactMatchOnly;
            public StringBuilder keyBuilder = new StringBuilder();

            public TValue conflictingNodeValue = default;
            public bool endOfDeleteTraversal = false;

            public TraverseDetails(string searchKey, TValue value = default, string actualKey = "", string blackListedWord = "", bool deleteMode = false, bool exactMatchOnly = false)
            {
                this.searchKey = searchKey;
                this.originalKey = searchKey;
                this.value = value;
                this.actualKey = actualKey;
                this.blackListedWord = blackListedWord;
                this.deleteMode = deleteMode;
                this.exactMatchOnly = exactMatchOnly;
            }
        }

        [JsonProperty("Version")]
        private readonly string m_version = "2.0";

        private Dictionary<string, TrieNode> m_altDictionary = new Dictionary<string, TrieNode>(); // dictionary to easily find/count/delete words instead of traversing trie dictionary

        private TrieNode root = new TrieNode();

        [JsonProperty("Alternate Dictionary")]
        public Dictionary<string, TrieNode> AltDictionary
        {
            get => m_altDictionary;
            set
            {
                m_altDictionary = value;
                InitializeTrieFromDict();
            }
        }

        public LooseDictionary()
        { }

        public void InitializeTrieFromDict()
        {
            root = new TrieNode();
            foreach (var item in m_altDictionary)
            {
                root.AddNode(item.Key, item.Value, true);
            }
        }

        public bool Add(string key, TValue value, out bool conflicting, string blackListWord = "", bool exactMatchOnly = false)
        {
            key = key.ToLower();
            conflicting = false;
            if (m_altDictionary.ContainsKey(key)) return false; // skip if key exists

            // start adding
            TrieNode newNode = new TrieNode(value, exactMatchOnly, blackListWord);
            TraverseDetails traverseDetails = new TraverseDetails(key, value: value, blackListedWord: blackListWord, exactMatchOnly: exactMatchOnly);
            // check if new key might conflict
            if (!traverseDetails.exactMatchOnly) conflicting = Traverse(traverseDetails);

            root.AddNode(key, newNode);
            m_altDictionary.Add(key, newNode);
            return true;
        }

        public bool Modify(string key, TValue newValue, bool newNodeExact, string newBlackListWord)
        {
            key = key.ToLower();

            if (!m_altDictionary.TryGetValue(key, out TrieNode nodeResult)) return false;

            nodeResult.SetValues(newValue, newNodeExact, newBlackListWord);
            return true;
        }

        public bool ChangeKey(string key, string newKey, out bool conflicting, out TValue conflictingValue)
        {
            key = key.ToLower();
            newKey = newKey.ToLower();
            conflicting = false;
            conflictingValue = default;
            if (key.Equals(newKey)) return true; // return success if same (skip code)
            if (m_altDictionary.ContainsKey(newKey) || !m_altDictionary.TryGetValue(key, out TrieNode nodeToSwap)) return false; // return fail if error

            TrieNode newNode = nodeToSwap.Clone();  // retrieve node copy its values
            m_altDictionary.Remove(key);            // delete old node from dictionary
            m_altDictionary.Add(newKey, newNode);   // add new node to new key
            RemoveNode(key);             // delete old node from trienode (or reset values depends if it has children)

            // check for conflicts
            TraverseDetails td = new TraverseDetails(key, exactMatchOnly: newNode.NodeExactMatchOnly);
            Traverse(td);
            conflicting = td.conflicting;
            conflictingValue = td.value;

            root.AddNode(newKey, newNode);          // add to new node

            return true;
        }

        public bool Remove(string key)
        {
            key = key.ToLower();
            if (!m_altDictionary.Remove(key)) return false;
            RemoveNode(key);
            return true;
        }

        public void SortTrackedObjects()
        {
            // Sort the dictionary by the "Name" property of MyClass
            var sortedList = m_altDictionary.OrderBy(kvp => kvp.Value).ToList();
            
            m_altDictionary = sortedList.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public void Clear()
        {
            if (m_altDictionary.Count == 0) return;
            m_altDictionary.Clear();
            root.Clear();
        }

        public bool TryGetValueLoose(string key, out TValue value, bool exactMatch = false)
        {
            key = key.ToLower();
            value = default;

            if (m_altDictionary.TryGetValue(key, out TrieNode nodeResult))
            {
                value = nodeResult.Value;
                return true;
            }
            if (exactMatch) return false;

            TraverseDetails traverseDetails = new TraverseDetails(key);
            bool found = Traverse(traverseDetails);
            value = traverseDetails.value;
            return found;
        }

        // overload for traverse delete
        public bool RemoveNode(string key)
        {
            TraverseDetails traverseDetails = new TraverseDetails(key, deleteMode: true, exactMatchOnly: true);
            bool found = Traverse(traverseDetails);
            return found;
        }

        public bool TryGetNodeLoose(string key, out TrieNode nodeResult)
        {
            TraverseDetails traverseDetails = new TraverseDetails(key);
            bool found = Traverse(traverseDetails);
            nodeResult = traverseDetails.nodeResult;
            return found;
        }

        public bool TryGetValueLooseLite(string key, out TValue result)
        {
            result = default;
            key = key.ToLower();

            if (m_altDictionary.TryGetValue(key, out TrieNode nodeResult))
            {
                result = nodeResult.Value;
                return true;
            }

            string substr;
            for (int i = 0; i < key.Length; i++)
            {
                substr = key.Substring(i);
                if (!root.TryGetValueRecursiveLite(substr, key, out result)) continue;
                return true;
            }

            return false;
        }

        public bool Traverse(TraverseDetails traverseDetails)
        {
            if (traverseDetails.exactMatchOnly) return root.TraverseRecursive(traverseDetails);
            string originalKey = traverseDetails.searchKey;
            for (int i = 0; i < traverseDetails.searchKey.Length; i++)
            {
                traverseDetails.searchKey = originalKey.Substring(i);
                if (!root.TraverseRecursive(traverseDetails)) continue;
                return true;
            }

            return false;
        }

        public class TrieNode : IComparable<TrieNode>
        {
            private readonly Dictionary<char, TrieNode> m_children = new Dictionary<char, TrieNode>();

            private TValue m_value = default;             // root is always null
            private bool m_nodeExactMatchOnly;  // use to determine if this node can only be accessed through exact searching
            private string m_blackListWord = "";     // blacklisted words, if this node was found, use this variable to compare with key, if key contains blacklist, ignore this node

            public TValue Value { get => m_value; set => m_value = value; }
            public bool NodeExactMatchOnly { get => m_nodeExactMatchOnly; set => m_nodeExactMatchOnly = value; }
            public string BlackListWord { get => m_blackListWord; set => m_blackListWord = value; }

            //private string key;     // used to determine the entire key string
            public TrieNode()
            { }

            public TrieNode(TValue value, bool nodeExactMatchOnly, string blackListWord)
            {
                SetValues(value, nodeExactMatchOnly, blackListWord);
            }

            public TrieNode Clone()
            {
                return new TrieNode(m_value, m_nodeExactMatchOnly, m_blackListWord);
            }

            public bool AddNode(string key, TrieNode nodeToAdd, bool forceAdd = false)
            {
                char keyChar = key[0];
                // traverse to children
                if (key.Length > 1)
                {
                    string newKeyString = key.Substring(1);

                    if (!m_children.TryGetValue(keyChar, out TrieNode childToAddTo))
                    {
                        // keyChar not found
                        childToAddTo = new TrieNode();
                        m_children.Add(keyChar, childToAddTo);
                    }
                    return childToAddTo.AddNode(newKeyString, nodeToAdd, forceAdd);
                }
                else // found destination
                {
                    bool childFound = m_children.TryGetValue(keyChar, out TrieNode childNodeToReplace);
                    if (childFound)
                    {
                        if (!forceAdd) return false;
                        childNodeToReplace.SetValues(nodeToAdd);
                    }
                    else
                    {
                        m_children.Add(keyChar, nodeToAdd);
                    }
                    return true;
                }
            }

            // delete this one node only, the others will be deleted from the recursive method
            public void Remove(char childNodeKey, TraverseDetails td)
            {
                TrieNode childNodeToDelete = m_children[childNodeKey];
                childNodeToDelete.ResetValues();
                // if this node has children do not cut the node off the main dictionary, only delete the instance
                if (childNodeToDelete.m_children.Count != 0)
                {
                    td.endOfDeleteTraversal = true;
                    return;
                }
                // delete node from main dictionary keep deleting until the parent's children is more than 1,
                /* ex. delete key foob
                 *     f - don't remove f cause 'u' child exists
                 *    u o - remove child o from f parent
                 *       o - remove child o from o parent
                 *        b - remove child b from o parent
                 * ex. 2
                 *     f - don't remove f cause node has result
                 *      o - remove child o from f parent
                 *       o - remove child o from o parent
                 *        b - remove child b from o parent
                 */
                m_children.Remove(childNodeKey); // remove child from parent
            }

            public void Clear()
            {
                m_children.Clear();
                m_value = default;
                m_nodeExactMatchOnly = false;
                m_blackListWord = "";
            }

            // finds the key either loosely or exact match, ex: entryKey = oobar, searchKey = barf*oobar*foo
            public bool TraverseRecursive(TraverseDetails td, int currentIndex = 0)
            {
                // keep checking until end of key, if true means there's more chars to check
                if (currentIndex >= td.searchKey.Length) return false;

                char currentChar = td.searchKey[currentIndex];
                td.keyBuilder.Append(currentChar); // add char to keybuilder (to build the actual key that's found)
                // if currentNode's children does not have the specified char key, exit this method to backtrack and check the next char
                // ex. entry in current node: rock , r doesn't exist, check o next time.
                // rock, ock, ck, k
                if (!m_children.TryGetValue(currentChar, out TrieNode nodeToCheck))
                {
                    td.keyBuilder.Clear();
                    return false;
                }

                // if node exists, check if node is valid ie. node has result is node can only be found through exact match, or key search does not contain blacklisted words by node
                if (IsNodeValid(nodeToCheck, td.originalKey, currentIndex, td.exactMatchOnly))
                {
                    // node is valid, return results
                    td.value = nodeToCheck.m_value;
                    td.nodeResult = nodeToCheck;
                    td.actualKey = td.keyBuilder.ToString();
                    if (td.deleteMode)
                    {
                        Remove(currentChar, td);
                        return true;
                    }
                    td.conflicting = CheckChildrenValid(td.searchKey, td.exactMatchOnly);
                    return true;
                }
                // invalid node result therefore keep looping until end of key or end of node (either the node can only be found through exact searching only or there's a blacklisted word that the node doens't want)
                bool foundValid = nodeToCheck.TraverseRecursive(td, currentIndex + 1);

                if (!foundValid) return false;
                // found result
                // if in delete mode, run remove child method, if reached a node with children, end of deletion
                if (td.deleteMode && !td.endOfDeleteTraversal) Remove(currentChar, td);
                return true;
            }

            private bool IsNodeValid(TrieNode nodeToCheck, string key, int currentIndex, bool exactMatchOnly = false)
            {
                if (nodeToCheck.m_value == null || // check if node is filled
                    ((nodeToCheck.m_nodeExactMatchOnly || exactMatchOnly) && currentIndex != key.Length - 1) || // found a close match, but can only be accessed through exact match
                    (!string.IsNullOrEmpty(nodeToCheck.m_blackListWord) && key.IndexOf(nodeToCheck.m_blackListWord, StringComparison.OrdinalIgnoreCase) != -1) // if the found node has a blacklisted word that is not found in the main key
                   )
                {
                    return false;
                }
                return true;
            }

            private void FindAllNodeValues(List<TrieNode> allValues)
            {
                // loop through node's childrend
                foreach (TrieNode node in m_children.Values)
                {
                    allValues.Add(node); // add current node
                    node.FindAllNodeValues(allValues); // get all children nodes of curr node
                }
            }

            private bool CheckChildrenValid(string key, bool exactMatch)
            {
                if (exactMatch) return false;
                List<TrieNode> descendants = new List<TrieNode>();
                FindAllNodeValues(descendants);
                for (int i = 0; i < descendants.Count; i++)
                {
                    TrieNode currChildNode = descendants[i];
                    if (currChildNode.m_value == null ||  // check if node is filled
                        currChildNode.m_nodeExactMatchOnly) continue; // found a close match, but can only be accessed through exact match
                    bool keyBlacklisted = !string.IsNullOrEmpty(currChildNode.m_blackListWord) && key.IndexOf(currChildNode.m_blackListWord, StringComparison.OrdinalIgnoreCase) != -1;
                    if (keyBlacklisted) continue;

                    return true;
                }
                return false;
            }

            // searches similar to contains/indexof but for trie, probably more efficient since if calling indexof from a list of keys it will call it for each entry
            public bool TryGetValueRecursiveLite(string key, string originalKey, out TValue result, int currentIndex = 0)
            {
                result = default;
                // keep checking until end of key, if true means there's more chars to check
                if (currentIndex >= key.Length) return false;

                char currentChar = key[currentIndex];
                // if currentNode's children does not have the specified char key, exit this method to backtrack and check the next char
                // ex. entry in current node: rock , r doesn't exist, check o next time.
                // rock, ock, ck, k
                if (!m_children.TryGetValue(currentChar, out TrieNode nodeToCheck))
                {
                    return false;
                }

                // if node exists, check if node is valid ie. node has result is node can only be found through exact match, or key search does not contain blacklisted words by node

                if (IsNodeValid(nodeToCheck, originalKey, currentIndex))
                {
                    // node is valid, return results
                    result = nodeToCheck.m_value;
                    return true;
                }

                // invalid node result therefore keep looping until end of key or end of node (either the node can only be found through exact searching only or there's a blacklisted word that the node doens't want)
                return nodeToCheck.TryGetValueRecursiveLite(key, originalKey, out result, currentIndex + 1);
            }

            public void SetValues(TrieNode nodeToRetrieve)
            {
                SetValues(nodeToRetrieve.Value, nodeToRetrieve.NodeExactMatchOnly, nodeToRetrieve.BlackListWord);
            }

            public void SetValues(TraverseDetails td)
            {
                SetValues(td.value, td.exactMatchOnly, td.blackListedWord);
            }

            public void SetValues(TValue value, bool exactMatchOnly, string blackListedWord)
            {
                m_value = value;
                m_nodeExactMatchOnly = exactMatchOnly;
                m_blackListWord = blackListedWord;
            }

            private void ResetValues()
            {
                m_value = default;
                m_nodeExactMatchOnly = false;
                m_blackListWord = string.Empty;
            }

            // searches loosely ex. key = c_o_p_p_e_r | result = copper
            // finds the key either loosely or exact match, ex: entryKey = oobar, searchKey = barf*oobar*foo
            public bool TraverseLooseRecursive(StringBuilder keyBuilder, TraverseDetails td, int currentIndex = 0, int nodeLength = 1)
            {
                // keep checking until end of key, if true means there's more chars to check
                if (currentIndex >= td.searchKey.Length) return false;

                // to check if the child entered gave no result, therefore get out of the child and remove a prefix to keep looping
                // ex. entry = runestone, and copper | search = rock4_copper, entered r child, but returned false when checking for o, but with this bool, it will exit out of r
                bool enteredChild = true;

                char currentChar = td.searchKey[currentIndex];
                keyBuilder.Append(currentChar); // add char to keybuilder (to build the actual key that's found)
                // if currentNode's children does not have the specified char key, exit this method to backtrack and check the next char
                // ex. entry in current node: rock , r doesn't exist, check o next time.
                // rock, ock, ck, k
                if (!m_children.TryGetValue(currentChar, out TrieNode nodeToCheck))
                {
                    if (td.exactMatchOnly) return false;
                    nodeToCheck = this;
                    keyBuilder.Clear(); // clear keybuilder because current path failed
                    enteredChild = false; // set to false to not run this method again in this current TrieNode class
                    nodeLength--;
                }

                // if node exists, check if node is valid ie. node has result is node can only be found through exact match, or key search does not contain blacklisted words by node
                if (IsNodeValidLoose(nodeToCheck, td.searchKey, nodeLength, td.exactMatchOnly))
                {
                    // node is valid, return results
                    td.value = nodeToCheck.m_value;
                    td.nodeResult = nodeToCheck;
                    td.actualKey = keyBuilder.ToString();
                    td.conflicting = CheckChildrenValid(td.searchKey, td.exactMatchOnly);
                    if (td.deleteMode) Remove(currentChar, td);
                    return true;
                }
                // invalid node result therefore keep looping until end of key or end of node (either the node can only be found through exact searching only or there's a blacklisted word that the node doens't want)
                bool foundValid = nodeToCheck.TraverseLooseRecursive(keyBuilder, td, currentIndex + 1, nodeLength + 1);

                if (enteredChild && !foundValid) // if the traverse child did not result a valid node, traverse parent node but check the next character
                {
                    nodeToCheck = this;
                    foundValid = nodeToCheck.TraverseLooseRecursive(keyBuilder, td, currentIndex + 1, nodeLength);
                    keyBuilder.Clear(); // clear keybuilder because current path has changed
                }

                if (!foundValid) return false;
                // found result
                // if in delete mode, run remove child method, if reached a node with children, end of deletion
                if (td.deleteMode && !td.endOfDeleteTraversal) Remove(currentChar, td);
                return true;
            }

            // searches loosely ex. key = c_o_p_p_e_r | result = copper
            // used solely for searching dictionary with loose key search
            public bool TryGetValueLooseRecursiveLite(string key, out TValue result, int currentIndex = 0, int nodeLength = 1)
            {
                result = default;
                // keep checking until end of key, if true means there's more chars to check
                if (currentIndex >= key.Length) return false;

                bool enteredChild = true;
                char currentChar = key[currentIndex];
                // if currentNode's children does not have the specified char key, exit this method to backtrack and check the next char
                // ex. entry in current node: rock , r doesn't exist, check o next time.
                // rock, ock, ck, k
                if (!m_children.TryGetValue(currentChar, out TrieNode nodeToCheck))
                {
                    nodeToCheck = this;
                    enteredChild = false;
                    nodeLength--;
                }

                // if node exists, check if node is valid ie. node has result is node can only be found through exact match, or key search does not contain blacklisted words by node

                if (IsNodeValidLoose(nodeToCheck, key, nodeLength))
                {
                    // node is valid, return results
                    result = nodeToCheck.m_value;
                    return true;
                }

                // invalid node result therefore keep looping until end of key or end of node (either the node can only be found through exact searching only or there's a blacklisted word that the node doens't want)
                bool foundValid = nodeToCheck.TryGetValueLooseRecursiveLite(key, out result, currentIndex + 1, nodeLength + 1);

                if (enteredChild && !foundValid) // if the traverse child did not result a valid node, traverse parent node but check the next character
                {
                    nodeToCheck = this;
                    foundValid = nodeToCheck.TryGetValueLooseRecursiveLite(key, out result, currentIndex + 1, nodeLength);
                }

                return foundValid;
            }
            private bool IsNodeValidLoose(TrieNode nodeToCheck, string key, int nodeLength, bool exactMatchOnly = false)
            {
                if (nodeToCheck.m_value == null || // check if node is filled
                    ((nodeToCheck.m_nodeExactMatchOnly || exactMatchOnly) && nodeLength != key.Length) || // found a close match, but can only be accessed through exact match
                    (!string.IsNullOrEmpty(nodeToCheck.m_blackListWord) && key.IndexOf(nodeToCheck.m_blackListWord, StringComparison.OrdinalIgnoreCase) != -1) // if the found node has a blacklisted word that is not found in the main key
                   )
                {
                    return false;
                }
                return true;
            }

            public int CompareTo(TrieNode other)
            {
                return m_value.CompareTo(other.m_value);
            }
        }
    }
}