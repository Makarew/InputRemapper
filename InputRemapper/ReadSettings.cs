using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;
using Rewired;
using UnityEngine;

namespace InputRemapper
{
    internal class ReadSettings
    {
        internal struct InputMaps
        {
            public int actionID;
            public bool negative;
            public KeyCode keyCode;
        }

        internal static void UpdateInputsFromSettings()
        {
            try
            {
                // Get The RInput Object
                Type t = typeof(RInput);
                RInput rin = GameObject.FindObjectOfType<RInput>();

                // Get The Player Field
                FieldInfo field = t.GetField("P", BindingFlags.NonPublic | BindingFlags.Instance);
                Player p = (Player)field.GetValue(rin);

                // Get The Map
                KeyboardMap map = (KeyboardMap)p.controllers.maps.GetMap(0);
                MouseMap mouseMap = (MouseMap)p.controllers.maps.GetMap(p.controllers.Mouse, 0, 0);

                // Remove All Mappings From The Map
                map.ClearElementMaps();
                mouseMap.ClearElementMaps();

                // Disable Mouse In Plugin
                Melon<Plugin>.Instance.usingMouse = false;

                // Reset Camera Sensitivity
                Melon<Plugin>.Instance.sensitivity.x = 90;
                Melon<Plugin>.Instance.sensitivity.y = 90;

                // Read The Settings File
                StreamReader r = new StreamReader(Path.Combine(MelonHandler.ModsDirectory, "P06ML/Mods/Plugins/Input Remapper/Settings.ini"));

                string line;
                using (r)
                {
                    do
                    {
                        //Read a line
                        line = r.ReadLine();
                        if (line != null)
                        {
                            //Divide line into basic structure
                            string[] lineData = line.Split(';');

                            // Get The ActionID From The Settings File For The Current Line
                            int actionID = GetActionID(lineData[0]);

                            Rewired.Pole pole = Rewired.Pole.Positive;

                            // Set Certain Actions To A Negative Pole And Switch To The Correct ActionID
                            switch (actionID)
                            {
                                case -2:
                                    actionID = 1;
                                    pole = Rewired.Pole.Negative;
                                    break;
                                case -3:
                                    actionID = 0;
                                    pole = Rewired.Pole.Negative;
                                    break;
                                case -4:
                                    actionID = 3;
                                    pole = Rewired.Pole.Negative;
                                    break;
                                case -5:
                                    actionID = 2;
                                    pole = Rewired.Pole.Negative;
                                    break;
                                case -6:
                                    actionID = 5;
                                    pole = Rewired.Pole.Negative;
                                    break;
                                case -7:
                                    actionID = 4;
                                    pole = Rewired.Pole.Negative;
                                    break;
                            }

                            // Check If The Line Is For Mouse Or Keyboard Controls
                            if (lineData[1].StartsWith("Mouse") || lineData[1].StartsWith("Wheel"))
                            {
                                // Get Mouse Element Map For The Current Line
                                MouseElementMap mMap = GetMouseElementMap(lineData[1]);

                                // Add The Input Mapping
                                Melon<Plugin>.Instance.UpdateInputs(actionID, mMap.axisContribution, mMap.elementID, mMap.elementType, mMap.axisRange, mMap.invert, mouseMap);

                                // Let Plugin Know That The Mouse Is Being Used
                                Melon<Plugin>.Instance.usingMouse = true;
                            } else
                            {
                                KeyCode keyCode = KeyCode.None;

                                float f = 0;
                                // Check If The Line Key Is A Number Value - Numbers Should Only Be On Sensitivity
                                if (float.TryParse(lineData[1], out f))
                                {
                                    // Set Sensitivity X/Y To The Line Key
                                    if (actionID == -10)
                                    {
                                        Melon<Plugin>.Instance.sensitivity.x = f;
                                    }
                                    if (actionID == -11)
                                    {
                                        Melon<Plugin>.Instance.sensitivity.y = f;
                                    }
                                }
                                else
                                {
                                    // Get The KeyCode From The Settings File For The Current Line
                                    keyCode = GetKeyCode(lineData[1]);

                                    // Add The Input Mapping
                                    Melon<Plugin>.Instance.UpdateInputs(actionID, pole, keyCode, map);
                                }
                            }
                        }
                    }
                    while (line != null);
                    //Stop reading the file
                    r.Close();
                }
            }
            catch { MelonLogger.Error("Couldn't Read Input Settings"); }
        }

        internal static InputMaps[] GetFromSettings()
        {
            List<InputMaps> maps = new List<InputMaps>();

            try
            {
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

                // Read The Settings File
                StreamReader r = new StreamReader(Path.Combine(MelonHandler.ModsDirectory, "P06ML/Mods/Plugins/Input Remapper/Settings.ini"));

                string line;
                using (r)
                {
                    do
                    {
                        //Read a line
                        line = r.ReadLine();
                        if (line != null)
                        {
                            //Divide line into basic structure
                            string[] lineData = line.Split(';');

                            // Get The ActionID From The Settings File For The Current Line
                            int actionID = GetActionID(lineData[0]);

                            bool isNeg = false;

                            // Set Certain Actions To A Negative Pole And Switch To The Correct ActionID
                            switch (actionID)
                            {
                                case -2:
                                    actionID = 1;
                                    isNeg = true;
                                    break;
                                case -3:
                                    actionID = 0;
                                    isNeg = true;
                                    break;
                                case -4:
                                    actionID = 3;
                                    isNeg = true;
                                    break;
                                case -5:
                                    actionID = 2;
                                    isNeg = true;
                                    break;
                                case -6:
                                    actionID = 5;
                                    isNeg = true;
                                    break;
                                case -7:
                                    actionID = 4;
                                    isNeg = true;
                                    break;
                            }

                            KeyCode keyCode = KeyCode.None;

                            float f = 0;
                            // Check If The Line Key Is A Number Value - Numbers Should Only Be On Sensitivity
                            if (float.TryParse(lineData[1], out f))
                            {
                                // Do Nothing - Section Needs Rewrite To Include Reporting Sensitivity
                            }
                            else
                            {
                                // Get The KeyCode From The Settings File For The Current Line
                                keyCode = GetKeyCode(lineData[1]);
                            }

                            // Store Data About The Input Map
                            InputMaps imap = new InputMaps();
                            imap.actionID = actionID;
                            imap.negative = isNeg;
                            imap.keyCode = keyCode;

                            maps.Add(imap);
                        }
                    }
                    while (line != null);
                    //Stop reading the file
                    r.Close();

                    UpdateInputsFromSettings();
                }
            }
            catch { MelonLogger.Error("Couldn't Read Input Settings"); }

            // Return The Stored Input Map Data
            return maps.ToArray();
        }

        // Get An Int For The ActionID Based On A String
        private static int GetActionID(string actionString)
        {
            int result = -1;

            switch(actionString)
            {
                case "LSUp":
                    result = 1;
                    break;
                case "LSDown":
                    result = -2;
                    break;
                case "LSRight":
                    result = 0;
                    break;
                case "LSLeft":
                    result = -3;
                    break;
                case "RSUp":
                    result = 3;
                    break;
                case "RSDown":
                    result = -4;
                    break;
                case "RSRight":
                    result = 2;
                    break;
                case "RSLeft":
                    result = -5;
                    break;
                case "PadUp":
                    result = 5;
                    break;
                case "PadDown":
                    result = -6;
                    break;
                case "PadRight":
                    result = 4;
                    break;
                case "PadLeft":
                    result = -7;
                    break;
                case "Start":
                    result = 14;
                    break;
                case "Back":
                    result = 15;
                    break;
                case "A":
                    result = 6;
                    break;
                case "B":
                    result = 8;
                    break;
                case "X":
                    result = 7;
                    break;
                case "Y":
                    result = 9;
                    break;
                case "LB":
                    result = 10;
                    break;
                case "RB":
                    result = 11;
                    break;
                case "LT":
                    result = 12;
                    break;
                case "RT":
                    result = 13;
                    break;

                case "SensitivityX":
                    result = -10;
                    break;
                case "SensitivityY":
                    result = -11;
                    break;
            }

            return result;
        }

        // Get The KeyCode Based On A String
        private static KeyCode GetKeyCode(string keyCode)
        {
            KeyCode result = KeyCode.None;

            switch (keyCode)
            {
                case "A":
                    result = KeyCode.A; break;
                case "B":
                    result = KeyCode.B; break;
                case "C":
                    result = KeyCode.C; break;
                case "D":
                    result = KeyCode.D; break;
                case "E":
                    result = KeyCode.E; break;
                case "F":
                    result = KeyCode.F; break;
                case "G":
                    result = KeyCode.G; break;
                case "H":
                    result = KeyCode.H; break;
                case "I":
                    result = KeyCode.I; break;
                case "J":
                    result = KeyCode.J; break;
                case "K":
                    result = KeyCode.K; break;
                case "L":
                    result = KeyCode.L; break;
                case "M":
                    result = KeyCode.M; break;
                case "N":
                    result = KeyCode.N; break;
                case "O":
                    result = KeyCode.O; break;
                case "P":
                    result = KeyCode.P; break;
                case "Q":
                    result = KeyCode.Q; break;
                case "R":
                    result = KeyCode.R; break;
                case "S":
                    result = KeyCode.S; break;
                case "T":
                    result = KeyCode.T; break;
                case "U":
                    result = KeyCode.U; break;
                case "V":
                    result = KeyCode.V; break;
                case "W":
                    result = KeyCode.W; break;
                case "X":
                    result = KeyCode.X; break;
                case "Y":
                    result = KeyCode.Y; break;
                case "Z":
                    result = KeyCode.Z; break;

                case "Space":
                    result = KeyCode.Space; break;

                case "BackQuote":
                    result = KeyCode.BackQuote; break;
                case "Tab":
                    result = KeyCode.Tab; break;
                case "LeftShift":
                    result = KeyCode.LeftShift; break;
                case "LeftControl":
                    result = KeyCode.LeftControl; break;
                case "LeftAlt":
                    result = KeyCode.LeftAlt; break;

                case "Backspace":
                    result = KeyCode.Backspace; break;
                case "Backslash":
                    result = KeyCode.Backslash; break;
                case "Return":
                    result = KeyCode.Return; break;
                case "RightShift":
                    result = KeyCode.RightShift; break;
                case "RightAlt":
                    result = KeyCode.RightAlt; break;

                case "LeftBracket":
                    result = KeyCode.LeftBracket; break;
                case "RightBracket":
                    result = KeyCode.RightBracket; break;
                case "Semicolon":
                    result = KeyCode.Semicolon; break;
                case "Quote":
                    result = KeyCode.Quote; break;
                case "Comma":
                    result = KeyCode.Comma; break;
                case "Period":
                    result = KeyCode.Period; break;
                case "Slash":
                    result = KeyCode.Slash; break;
                case "Minus":
                    result = KeyCode.Minus; break;
                case "Equals":
                    result = KeyCode.Equals; break;

                case "Alpha0":
                    result = KeyCode.Alpha0; break;
                case "Alpha1":
                    result = KeyCode.Alpha1; break;
                case "Alpha2":
                    result = KeyCode.Alpha2; break;
                case "Alpha3":
                    result = KeyCode.Alpha3; break;
                case "Alpha4":
                    result = KeyCode.Alpha4; break;
                case "Alpha5":
                    result = KeyCode.Alpha5; break;
                case "Alpha6":
                    result = KeyCode.Alpha6; break;
                case "Alpha7":
                    result = KeyCode.Alpha7; break;
                case "Alpha8":
                    result = KeyCode.Alpha8; break;
                case "Alpha9":
                    result = KeyCode.Alpha9; break;

                case "UpArrow":
                    result = KeyCode.UpArrow; break;
                case "DownArrow":
                    result = KeyCode.DownArrow; break;
                case "RightArrow":
                    result = KeyCode.RightArrow; break;
                case "LeftArrow":
                    result = KeyCode.LeftArrow; break;

                case "Insert":
                    result = KeyCode.Insert; break;
                case "Home":
                    result = KeyCode.Home; break;
                case "PageUp":
                    result = KeyCode.PageUp; break;
                case "Delete":
                    result = KeyCode.Delete; break;
                case "End":
                    result = KeyCode.End; break;
                case "PageDown":
                    result = KeyCode.PageDown; break;

                case "Keypad0":
                    result = KeyCode.Keypad0; break;
                case "Keypad1":
                    result = KeyCode.Keypad1; break;
                case "Keypad2":
                    result = KeyCode.Keypad2; break;
                case "Keypad3":
                    result = KeyCode.Keypad3; break;
                case "Keypad4":
                    result = KeyCode.Keypad4; break;
                case "Keypad5":
                    result = KeyCode.Keypad5; break;
                case "Keypad6":
                    result = KeyCode.Keypad6; break;
                case "Keypad7":
                    result = KeyCode.Keypad7; break;
                case "Keypad8":
                    result = KeyCode.Keypad8; break;
                case "Keypad9":
                    result = KeyCode.Keypad9; break;

                case "KeypadDivide":
                    result = KeyCode.KeypadDivide; break;
                case "KeypadMultiply":
                    result = KeyCode.KeypadMultiply; break;
                case "KeypadMinus":
                    result = KeyCode.KeypadMinus; break;
                case "KeypadPlus":
                    result = KeyCode.KeypadPlus; break;
                case "KeypadEnter":
                    result = KeyCode.KeypadEnter; break;
                case "KeypadPeriod":
                    result = KeyCode.KeypadPeriod; break;

                case "Escape":
                    result = KeyCode.Escape; break;

                case "F1":
                    result = KeyCode.F1; break;
                case "F2":
                    result = KeyCode.F2; break;
                case "F3":
                    result = KeyCode.F3; break;
                case "F4":
                    result = KeyCode.F4; break;

                case "F5":
                    result = KeyCode.F5; break;
                case "F6":
                    result = KeyCode.F6; break;
                case "F7":
                    result = KeyCode.F7; break;
                case "F8":
                    result = KeyCode.F8; break;

                case "F9":
                    result = KeyCode.F9; break;
                case "F10":
                    result = KeyCode.F10; break;
                case "F11":
                    result = KeyCode.F11; break;
                case "F12":
                    result = KeyCode.F12; break;
            }

            return result;
        }

        // Stores Mouse Input Map Data
        internal struct MouseElementMap
        {
            public Rewired.Pole axisContribution;
            public int elementID;
            public ControllerElementType elementType;
            public AxisRange axisRange;
            public bool invert;
        }

        // Get The Mouse Input Map Data
        private static MouseElementMap GetMouseElementMap(string mouseCode) 
        { 
            MouseElementMap mMap = new MouseElementMap();

            switch (mouseCode)
            {
                case "MouseX":
                    mMap.axisContribution = Rewired.Pole.Positive;
                    mMap.elementID = 0;
                    mMap.elementType = ControllerElementType.Axis;
                    mMap.axisRange = AxisRange.Full;
                    mMap.invert = true;
                    break;
                case "MouseY":
                    mMap.axisContribution = Rewired.Pole.Positive;
                    mMap.elementID = 1;
                    mMap.elementType = ControllerElementType.Axis;
                    mMap.axisRange = AxisRange.Full;
                    mMap.invert = true;
                    break;
                case "MouseLeft":
                    mMap.axisContribution = Rewired.Pole.Positive;
                    mMap.elementID = 3;
                    mMap.elementType = ControllerElementType.Button;
                    mMap.axisRange = AxisRange.Positive;
                    mMap.invert = false;
                    break;
                case "MouseRight":
                    mMap.axisContribution = Rewired.Pole.Positive;
                    mMap.elementID = 4;
                    mMap.elementType = ControllerElementType.Button;
                    mMap.axisRange = AxisRange.Positive;
                    mMap.invert = false;
                    break;
                case "WheelUp":
                    mMap.axisContribution = Rewired.Pole.Positive;
                    mMap.elementID = 2;
                    mMap.elementType = ControllerElementType.Axis;
                    mMap.axisRange = AxisRange.Positive;
                    mMap.invert = false;
                    break;
                case "WheelDown":
                    mMap.axisContribution = Rewired.Pole.Negative;
                    mMap.elementID = 2;
                    mMap.elementType = ControllerElementType.Axis;
                    mMap.axisRange = AxisRange.Negative;
                    mMap.invert = false;
                    break;
            }

            return mMap;
        }
    }
}
