using System.IO;
using UnityEditor;
using UnityEngine;

public class CustomUnityVS
{
    [MenuItem("UnityVS/Open in UnityVS Mac")]
    static void Open()
    {
        string[] strings = Path.GetDirectoryName(Application.dataPath.Replace("/Assets/", "")).Split(Path.DirectorySeparatorChar);
        string path = (string.Format("UnityVS.{0}.sln", strings[strings.Length - 1]));
        if (!File.Exists(path))
        {
            EditorApplication.ExecuteMenuItem("UnityVS/Generate Project Files");
        }
        EditorUtility.OpenWithDefaultApp(path);
    }

}
