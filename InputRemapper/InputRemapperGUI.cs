//Display A Message In The Top Left Corner When The Input Mapping Is Changed

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
                GUI.Label(new Rect(10, 20, 500, 70), "Updated Input Map", style);
            }
        }
    }
}
