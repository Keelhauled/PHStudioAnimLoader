using UnityEngine;

namespace StudioAnimLoader
{
    internal class PBMessageBox : MonoBehaviour
    {
        private float topOffset = 100f;
        private Rect rect;
        private string message;
        private float time;
        private bool initialized = false;

        public void Init(float width, float height, float screenWidth, float screenHeight, string message, float time)
        {
            rect = new Rect((screenWidth - width) / 2, topOffset, width, height);
            this.message = message;
            this.time = time;
            initialized = true;
        }

        private void OnGUI()
        {
            if(initialized)
            {
                GUIStyle guiStyle = new GUIStyle(GUI.skin.textArea);
                guiStyle.alignment = TextAnchor.MiddleCenter;
                GUI.Label(rect, message, guiStyle);
            }
        }

        private void Start()
        {
            Destroy(gameObject, time);
        }
    }
}