﻿using MelonLoader;
using Rewired;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;
using HarmonyLib;

namespace InputRemapper
{
    public class Plugin : MelonMod
    {
        // For Testing - Keep True
        private bool setInputs = true;

        // UI Handler
        private InputRemapperGUI irGUI;

        internal ReadSettings.InputMaps[] maps;

        public Vector2 sensitivity = new Vector2(90, 90);
        internal Vector2 trailerRot = new Vector2();

        private bool GetSettings;

        internal bool usingMouse;
        private bool madeMouse;

        public static new HarmonyLib.Harmony Harmony { get; private set; }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            // Create The UI Handler On The Title Screen If It Doesn't Exist
            if (sceneName == "TitleScreen")
            {
                if (irGUI == null)
                {
                    irGUI = GameObject.Instantiate(new GameObject()).AddComponent<InputRemapperGUI>();
                    GameObject.DontDestroyOnLoad(irGUI);

                    // Give The UI Handler A Font And Color
                    irGUI.style.font = GameObject.Find("Canvas/title_screen/SaveSelect/WindowBG/HeaderText").GetComponent<Text>().font;
                    irGUI.style.normal.textColor = Color.white;
                }

                // Get The RInput Object
                Type t = typeof(RInput);
                RInput rin = GameObject.FindObjectOfType<RInput>();

                // Get The Player Field
                FieldInfo field = t.GetField("P", BindingFlags.NonPublic | BindingFlags.Instance);
                Player p = (Player)field.GetValue(rin);

                // Add Mouse Controller And Map
                if (!madeMouse)
                {
                    p.controllers.AddController(ControllerType.Mouse, 0, false);
                    p.controllers.maps.AddMap(p.controllers.Mouse, new MouseMap(), true);
                }

                // Load Input Settings Next Frame
                GetSettings = true;
            }

            // For Testing - Shouldn't Run By Default
            if (!setInputs) 
            {
                Type t = typeof(RInput);
                RInput rin = GameObject.FindObjectOfType<RInput>();

                FieldInfo field = t.GetField("P", BindingFlags.NonPublic | BindingFlags.Instance);
                Player p = (Player)field.GetValue(rin);

                KeyboardMap map = (KeyboardMap)p.controllers.maps.GetMap(0);

                map.ClearElementMaps();

                UpdateInputs(0, Rewired.Pole.Positive, KeyCode.None, map); 
            }
        }

        // Create The UI Handler On The Mod Menu If It Doesn't Exist
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();

            if (irGUI == null && GameObject.Find("Select Stage Menu UI"))
            {
                irGUI = GameObject.Instantiate(new GameObject()).AddComponent<InputRemapperGUI>();
                GameObject.DontDestroyOnLoad(irGUI);

                // Give The UI Handler A Font And Color
                irGUI.style.font = GameObject.Find("Select Stage Menu UI/Menu Holder/Global-Canvas/Plugins Menu Selector/Stage Preview/Options/Title Text").GetComponent<Text>().font;
                irGUI.style.normal.textColor = Color.white;
            }

            // Patch The Camera To Use Sensitivity
            Harmony = new HarmonyLib.Harmony("InputRemapper");

            CameraPatches.Initialize();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // Don't Run If The UI Handler Doesn't Exist
            if (irGUI == null) return;

            // Wait A Frame After Loading Title Screen To Update Input Maps
            // Prevents Remapper From Trying To Use The Mouse Map Before It Finishes Initializing
            if (GetSettings)
            {
                // Read The Settings File And Apply The Input Mapping
                maps = ReadSettings.GetFromSettings();

                // Turn On The UI Handler With A Four Second Timer
                irGUI.guiTimer = 4;
                irGUI.useGUI = true;

                // Set To False To Prevent Map Updates Every Frame
                GetSettings = false;
            }

            // Wait For The Player To Press Right Control
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                // Read The Settings File And Apply The Input Mapping
                maps = ReadSettings.GetFromSettings();

                // Turn On The UI Handler With A Four Second Timer
                irGUI.guiTimer = 4;
                irGUI.useGUI = true;
            }

            // If Any Mouse Controls Are Used, Lock The Cursor To The Center Of The Screen While Playing
            if (usingMouse && Singleton<GameManager>.Instance.GameState == GameManager.State.Playing && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            // And Free The Cursor When Not Playing
            else if (usingMouse && Singleton<GameManager>.Instance.GameState != GameManager.State.Playing && Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        // Add An Input Map
        internal void UpdateInputs(int actionID, Rewired.Pole pole, KeyCode keycode, KeyboardMap map)
        {
            if (map != null)
            {
                ActionElementMap oneP = new ActionElementMap();

                map.CreateElementMap(actionID, pole, keycode, ModifierKey.None, ModifierKey.None, ModifierKey.None, out oneP);

                setInputs = true;
            }
        }

        // Add A Mouse Input Map
        internal void UpdateInputs(int actionID, Rewired.Pole axisContribution, int elementID, Rewired.ControllerElementType elementType, AxisRange axisRange, bool invert, MouseMap map)
        {
            if (map != null)
            {
                ActionElementMap oneP = new ActionElementMap();

                map.CreateElementMap(actionID, axisContribution, elementID, elementType, axisRange, invert, out oneP);
            }
        }

        // Revert To The Default Control Scheme When The Plugin Is Disabled
        public override void OnDeinitializeMelon()
        {
            base.OnDeinitializeMelon();

            // Get The RInput Object
            Type t = typeof(RInput);
            RInput rin = GameObject.FindObjectOfType<RInput>();

            // Get The Player Field
            FieldInfo field = t.GetField("P", BindingFlags.NonPublic | BindingFlags.Instance);
            Player p = (Player)field.GetValue(rin);

            // Get The Map
            KeyboardMap map = (KeyboardMap)p.controllers.maps.GetMap(0);

            // Remove All Mappings From The Map
            map.ClearElementMaps();

            // Add The Original Mappings To The Map

            // Left Stick Y
            UpdateInputs(1, Rewired.Pole.Positive, KeyCode.UpArrow, map);
            UpdateInputs(1, Rewired.Pole.Negative, KeyCode.DownArrow, map);

            // Left Stick X
            UpdateInputs(0, Rewired.Pole.Positive, KeyCode.RightArrow, map);
            UpdateInputs(0, Rewired.Pole.Negative, KeyCode.LeftArrow, map);

            // Start
            UpdateInputs(14, Rewired.Pole.Positive, KeyCode.Return, map);

            // Back
            UpdateInputs(15, Rewired.Pole.Positive, KeyCode.Backspace, map);

            // A
            UpdateInputs(6, Rewired.Pole.Positive, KeyCode.Space, map);

            // B
            UpdateInputs(8, Rewired.Pole.Positive, KeyCode.X, map);

            // X
            UpdateInputs(7, Rewired.Pole.Positive, KeyCode.Z, map);

            // Y
            UpdateInputs(9, Rewired.Pole.Positive, KeyCode.C, map);

            // Left Bumper
            UpdateInputs(10, Rewired.Pole.Positive, KeyCode.Q, map);

            // Right Bumper
            UpdateInputs(11, Rewired.Pole.Positive, KeyCode.E, map);

            // Left Trigger
            UpdateInputs(12, Rewired.Pole.Positive, KeyCode.LeftShift, map);

            // Right Trigger
            UpdateInputs(13, Rewired.Pole.Positive, KeyCode.RightShift, map);

            // D-Pad Y
            UpdateInputs(5, Rewired.Pole.Positive, KeyCode.G, map);
            UpdateInputs(5, Rewired.Pole.Negative, KeyCode.B, map);

            // D-Pad X
            UpdateInputs(4, Rewired.Pole.Positive, KeyCode.N, map);
            UpdateInputs(4, Rewired.Pole.Negative, KeyCode.V, map);

            // Right Stick Y
            UpdateInputs(3, Rewired.Pole.Positive, KeyCode.W, map);
            UpdateInputs(3, Rewired.Pole.Negative, KeyCode.S, map);

            // Right Stick X
            UpdateInputs(2, Rewired.Pole.Positive, KeyCode.D, map);
            UpdateInputs(2, Rewired.Pole.Negative, KeyCode.A, map);
        }
    }
}
