using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetbundle
{
    [MenuItem("Assets/CreateAssetBundle", true)]
    static bool _Create()
    {
        return Directory.Exists(AssetDatabase.GetAssetPath(Selection.activeObject));
    }
    [MenuItem("Assets/CreateAssetBundle")]
    static void Create()
    {
        var objs = new List<Object>();
        var dependencies = GetObjects(AssetDatabase.GetAssetPath(Selection.activeObject), objs);

        BuildPipeline.BuildAssetBundle(null, dependencies, "Assets/StateMachineMaker/Editor/Resources.unity3d", BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }

    static Object[] GetObjects(string path, List<Object> objs)
    {
        var dependencies = Directory.GetFiles(path);

        var directories = Directory.GetDirectories(path);
        foreach (string directory in directories)
        {
            GetObjects(directory, objs);
        }
        foreach (string dependency in dependencies)
        {
            var obj = AssetDatabase.LoadAssetAtPath(dependency, typeof(Object));
            objs.Add(obj);
        }

        return objs.ToArray();
    }

}
