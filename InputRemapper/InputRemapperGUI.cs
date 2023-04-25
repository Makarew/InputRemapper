//Display A Message In The Top Left Corner When The Input Mapping Is Changed

using MelonLoader;
using UnityEngine;

namespace InputRemapper
{
    internal class InputRemapperGUI : MonoBehaviour
    {
        internal float guiTimer;
        internal bool useGUI;

        internal GUIStyle style = new GUIStyle()
        {
            fontSize = 20,
        };

        private void Update()
        {
            // Basic Timer - Turns Off The GUI Once The Timer Hits 0
            if (guiTimer > 0)
            {
                guiTimer -= Time.deltaTime;
            }
            else if (useGUI)
            {
                useGUI = false;
            }
        }

        void OnGUI()
        {
            if (useGUI)
            {
                // Notification That Plays When Input Maps Update
                GUI.Label(new Rect(10, 20, 500, 70), "Updated Input Map", style);

                float y = 60;

                // Check All Input Maps Stored In Plugin And Display On Screen
                for (int i = 0; i < Melon<Plugin>.Instance.maps.Length; i++)
                {
                    string res = "";
                    if (Melon<Plugin>.Instance.maps[i].negative)
                    {
                        res = "N";
                    }
                    else
                    {
                        res = "P";
                    }

                    res += " " + Melon<Plugin>.Instance.maps[i].actionID + " " + Melon<Plugin>.Instance.maps[i].keyCode;

                    GUI.Label(new Rect(10, y, 500, 70), res, style);

                    y += 30;
                }
            }
        }
    }
}
