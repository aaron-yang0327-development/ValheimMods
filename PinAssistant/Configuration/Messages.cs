﻿using System;
using System.Reflection;

namespace WxAxW.PinAssistant.Configuration
{
    public enum TextType
    {
        NULL,

        // Mod Config
        [Text("General")] CONFIG_CATEGORY_GENERAL,

        [Text("Trackable Types")] CONFIG_CATEGORY_TYPES,
        [Text("Hotkeys")] CONFIG_CATEGORY_HOTKEYS,
        [Text("Technical")] CONFIG_CATEGORY_TECHNICAL,
        [Text("Enabled Mod")] CONFIG_NAME_TOGGLE_MOD,
        [Text("Auto Pinning")] CONFIG_NAME_TOGGLE_AUTOPINNING,
        [Text("Look Tick Rate")] CONFIG_NAME_VAL_TICKRATE,
        [Text("Redundancy Distance")] CONFIG_NAME_VAL_DISTANCEREDUNDANCY,
        [Text("Look Distance")] CONFIG_NAME_VAL_DISTANCELOOK,

        [Text("Destructible")] CONFIG_NAME_TYPE_DESTRUCTIBLE,
        [Text("Pickable")] CONFIG_NAME_TYPE_PICKABLE,
        [Text("MineRock")] CONFIG_NAME_TYPE_MINEROCK,
        [Text("Location")] CONFIG_NAME_TYPE_LOCATION,
        [Text("SpawnArea")] CONFIG_NAME_TYPE_SPAWNAREA,
        [Text("Vegvisir")] CONFIG_NAME_TYPE_VEGVISIR,
        [Text("ResourceRoot")] CONFIG_NAME_TYPE_RESOURCEROOT,
        [Text("TreeBase")] CONFIG_NAME_TYPE_TREEBASE,

        [Text("Track Looked Object")] CONFIG_NAME_KEY_TRACKOBJECT,
        [Text("Pin Object")] CONFIG_NAME_KEY_PINOBJECT,
        [Text("Reload Tracked Objects")] CONFIG_NAME_KEY_RELOADTRACKED,

        [Text("Debug Mode")] CONFIG_NAME_DEBUGMODE,
        [Text("Objects Tracked")] CONFIG_NAME_OBJEECTSTRACKED,

        // Config messages
        [Text("Enable or disable plugin\nTo auto pin you must enable '{0}' and look at the object")] CONFIG_MESSAGE_TOGGLE_MOD,

        [Text("Enable or disable auto pinning when looking at an object (only auto pins objects from '{0}'")] CONFIG_MESSAGE_TOGGLE_AUTOPINNING,
        [Text("The tick rate for when to check the object you're looking at to attempt to pin it\nThe value is 'n seconds per tick'")] CONFIG_MESSAGE_VAL_TICKRATE,
        [Text("The minimum distance to prevent pinning multiple objects close together")] CONFIG_MESSAGE_VAL_DISTANCEREDUNDANCY,
        [Text("The maximum distance you can detect an object")] CONFIG_MESSAGE_VAL_DISTANCELOOK,

        [Text("A lot of things, objects that have other types may have this as well (ie. Berry Bush has both 'Pickable' and 'Destructible')")] CONFIG_MESSAGE_TYPE_DESTRUCTIBLE,
        [Text("Pickable Objects: Basically things you can press a key to pick (flint, stone, branch, berry bush, mushroom, etc)")] CONFIG_MESSAGE_TYPE_PICKABLE,
        [Text("Minerals: Tin, Copper(Intact), Obsidian (Note: Giant ores' types are 'Destructible' at first because it is purely solid object with no breakable piece. When damanged, it turns into a different rock that looks exactly the same but has breakable parts and the object now has the 'MineRock' type and not 'Destructible')")] CONFIG_MESSAGE_TYPE_MINEROCK,
        [Text("POIs: crypts, sunken crypts, structures that transport you to a different area")] CONFIG_MESSAGE_TYPE_LOCATION,
        [Text("Spawners: Bone pile, Greydwarf nest, etc.")] CONFIG_MESSAGE_TYPE_SPAWNAREA,
        [Text("The runestone that shows you boss locations")] CONFIG_MESSAGE_TYPE_VEGVISIR,
        [Text("Yggdrasil root (the giant ancient root one) at Mistlands")] CONFIG_MESSAGE_TYPE_RESOURCEROOT,
        [Text("Trees that leaves stumps")] CONFIG_MESSAGE_TYPE_TREEBASE,

        [Text("Key to open the GUI\nTo register the object you're looking at to the list of objects to automaticaly pin")] CONFIG_MESSAGE_KEY_TRACKOBJECT,
        [Text("Key to manually pin the object you're looking at\nUsed for when you disable '{0}'\n(Object must be tracked using '{1}' to pin)")] CONFIG_MESSAGE_KEY_PINOBJECT,
        [Text("Key to force reload the tracked objects, if you manually modified the config entry for {0}")] CONFIG_MESSAGE_KEY_RELOADTRACKED,
        [Text("Enables debug mode, useful for understanding what's happening whenever you use this mod")] CONFIG_MESSAGE_DEBUGMODE,
        [Text("List of objects tracked currently being tracked to pin when looked or pressed the '{0}' key\nONLY EDIT THIS IF YOU KNOW WHAT YOU'RE DOING, IF NOT JUST USE THE GUI INSTEAD")] CONFIG_MESSAGE_OBJECTSTRACKED,

        // AutoPinning Messages

        // Info
        [Text("{0}:\tLayerMask: {1} | {2}\tType: {3}")] OBJECT_INFO,

        // Pins
        [Text("Minimap not found, this should not happen I think")] MINIMAP_NOT_FOUND,

        [Text("No pins found, I assume you're loading your world")] WORLD_LOADING,
        [Text("{0}: attempting to add: '{1}' at '{2}'!")] PIN_ADDING,
        [Text("Pin Added")] PIN_ADDED,
        [Text("This is a ping, not adding to database")] PING_ADDING,
        [Text("Pin already exists")] PIN_ADDING_EXISTS,
        [Text("Nearby pin with same name already exists")] PIN_ADDING_EXISTS_NEARBY,

        [Text("Removed '{0}' at '{1}'!")] PIN_REMOVED,

        [Text("Cleared mod's pin storage")] PINS_CLEARED,

        [Text("Pins Populated")] PINS_POPULATED,

        // Object Tracking
        [Text("Empty tracked objects, initializing instead")] TRACKED_OBJECTS_INITIALIZED,

        [Text("Loaded tracked objects")] TRACKED_OBJECTS_LOADED,
        [Text("Saved tracked objects")] TRACKED_OBJECTS_SAVED,
        [Text("Invalid Data or Empty!")] TRACKED_OBJECTS_EMPTY,
        [Text("Invalid Data!")] TRACKED_OBJECTS_INVALID,
        [Text("The object trying to be pinned is not included in the objects to track")] OBJECT_NOT_TRACKED,
        [Text("Dropdown tracked objects populated")] OBJECTS_DROPDOWN_LOADED,

        // AutoPinUI Messages

        // UI Names
        [Text("Track")] BUTTON_TRACK,

        [Text("Modify")] BUTTON_MODIFY,
        [Text("Cancel")] BUTTON_CANCEL,
        [Text("Untrack")] BUTTON_UNTRACK,
        [Text("Track Object")] HEADER_TRACK,
        [Text("Modify Tracked Object")] HEADER_MODIFY,

        // UI Messages
        [Text("Successfully tracked '{0}'!")] TRACK_SUCCESS,

        [Text("Object tracked, but '{0}' conflicts with other ID/s")] TRACK_WARNING_CONFLICT,
        [Text("Unable to track '{0}', '{0}' already exists!")] TRACK_FAIL,
        [Text("Object ID cannot be empty")] TRACK_INVALID,

        [Text("Successfully modified '{0}'!")] MODIFY_SUCCESS,
        [Text("Object modified, but '{0}' conflicts with {1}!")] MODIFY_WARNING_CONFLICT,

        [Text("Successfully untracked '{0}'!")] UNTRACK_SUCCESS,
        [Text("'{0}' is not being tracked? This is not supposed to happen!")] UNTRACK_FAIL,

        // Debug stuff
        [Text("Plugin Enabled")] PLUGIN_ENABLED,

        [Text("Plugin Destroyed")] PLUGIN_DISABLED,
        [Text("Mod Enabled")] MOD_ENABLED,
        [Text("Mod Disabled")] MOD_DISABLED,
        [Text("Scene changed to: {0}")] SCENE_CHANGE,

        [Text("FAIL! This is not supposed to happen! contact developer to fix this")] NOT_POSSIBLE,

        // not in use
        [Text("Unable to track '{0}', '{0}' is conflicting with '{1}'")] TRACK_FAIL_CONFLICT,

        [Text("Unable to modify '{0}'. New ID '{1}' is conflicting with '{0}'!")] MODIFY_FAIL_CONFLICT,
    }

    public class TextAttribute : Attribute
    {
        public string Value { get; }

        public TextAttribute(string value)
        {
            Value = value;
        }

        public static string Get(TextType value, params object[] parameters)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var stringValueAttribute = fieldInfo.GetCustomAttribute<TextAttribute>();
            if (stringValueAttribute == null) return value.ToString();
            // Alternatively, you can use string interpolation
            // return string.Format(format, parameters);

            string format = stringValueAttribute.Value;
            return string.Format(format, parameters);
        }
    }
}