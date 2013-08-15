using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    public class Styles
    {
        private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        public static GUIStyle BackgroudStyle
        {
            get
            {
                if (sprites.Count == 0)
                {
                    Load();
                }
                string key = "background";
                if (EditorGUIUtility.isProSkin)
                    key += "_p";
                return sprites[key].style;
            }
        }

        private static void Load()
        {
            sprites = new Dictionary<string, Sprite>();
            string[] loadAllAssetsAtPath = Directory.GetFiles("Assets/StateMachineMaker/Editor/Images/");
            foreach (string key in loadAllAssetsAtPath)
            {
                string spriteName = Path.GetFileNameWithoutExtension(key);
                sprites.Add(spriteName, new Sprite(key));
            }
        }

        public static GUIStyle GetStateStyle(StateColor stateColor, bool on)
        {
            if (sprites.Count == 0)
            {
                Load();
            }
            string key = stateColor.ToString().ToLower();

            if (on)
                key += " on";
            if (EditorGUIUtility.isProSkin)
                key += "_p";
            return sprites[key].style;
        }

        private class Sprite
        {
            public GUIStyle style;

            public Sprite(string path)
            {
                style = new GUIStyle();
                style.border = new RectOffset(11, 11, 11, 15);
                style.padding = new RectOffset(0, 0, 29, 0);
                style.overflow = new RectOffset(7, 7, 6, 9);
                style.alignment = TextAnchor.UpperCenter;
                style.normal.background = AssetDatabase.LoadAssetAtPath(path, typeof (Texture2D)) as Texture2D;
                style.contentOffset = new Vector2(0, -22);
            }
        }
    }
#if !UNITY_3_5
}
#endif