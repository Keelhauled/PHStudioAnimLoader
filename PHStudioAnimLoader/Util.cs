using System;
using System.Linq;
using UnityEngine;

namespace StudioAnimLoader
{
    internal static class Util
    {
        private static string[] separators = { "+" };
        private static string[,] replacePairs = { { "control", "ctrl" } };
        private static string[] leftRightKeys = { "ctrl", "shift", "alt" };

        private class UnityJsonWrapper
        {
            public int[] Value;
        }

        internal static int[] ParseJsonArray(string json)
        {
            int[] array;
            try
            {
                array = JsonUtility.FromJson<UnityJsonWrapper>("{\"Value\":" + json + "}").Value;
            }
            catch (Exception e)
            {
                //Console.WriteLine("PlayBoop Error: " + e);
                array = null;
            }

            if (array != null && array.Length == 0) array = null;

            return array;
        }

        //isKeyDown: true = once per press / false = continuously while pressing 
        internal static bool InputChecker(string[] keyNames, bool isKeyDown)
        {
            if (keyNames == null) return false;
            bool initBool = true;
            try
            {
                for (int i = 0; i < keyNames.Length; i++)
                {
                    string keyName = keyNames[i];
                    if (leftRightKeys.Contains<string>(keyName))
                    {
                        if (isKeyDown && i + 1 == keyNames.Length)
                        {
                            initBool &= Input.GetKeyDown("left " + keyName) | Input.GetKeyDown("right " + keyName);
                        }
                        else
                        {
                            initBool &= Input.GetKey("left " + keyName) | Input.GetKey("right " + keyName);
                        }
                    }
                    else
                    {
                        if (isKeyDown && i + 1 == keyNames.Length)
                        {
                            initBool &= Input.GetKeyDown(keyName);
                        }
                        else
                        {
                            initBool &= Input.GetKey(keyName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("PlayBoop Error: " + e);
                return false;
            }

            return initBool;
        }
    }

    //UI msg box
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
            if (initialized)
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