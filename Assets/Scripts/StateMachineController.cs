using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class StateMachineController<M, S, TS> : ScriptableObject
    where M : StateMachine<S, TS>
    where S : State
    where TS : Transition
{

    public StateMachineController()
    {
        // TODO どうにかしてIconを変更したい...
        //        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(GetInstanceID()));
        //        EditorApplication.projectWindowItemOnGUI += (_guid, rect) =>
        //        {
        //            if (guid == _guid)
        //            {
        //                rect.width = 16;
        //                rect.height = 16;
        //                GUI.DrawTexture(rect, (Texture)EditorGUIUtility.Load("Icons/Generated/State Icon.asset"));
        //            }
        //        };
        //
        //        Object defaultAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(GetInstanceID()), typeof(UnityEngine.Object));
        //        Debug.Log(defaultAsset);
        //
        //        MethodInfo methodInfo = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
        //        methodInfo.Invoke(null, new object[] { defaultAsset, EditorGUIUtility.whiteTexture });
    }


    public int stateMahineCount
    {
        get
        {
            string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            return objects.Count(obj => obj is M);
        }
    }

    public M GetStateMeshine(int index)
    {
        string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        M[] ms = objects.Where(obj => obj is M).Cast<M>().ToArray();
        return index <= stateMahineCount ? ms[index] : null;
    }

    public void AddStateMachine(string stateMachineName)
    {
        M scriptableObject = (M)ScriptableObject.CreateInstance(typeof(M));
        scriptableObject.name = stateMachineName;
        string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        AssetDatabase.AddObjectToAsset(scriptableObject, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    protected static void CreateAssets<T>() where T : StateMachineController<M, S, TS>
    {
        T stateMachineController = ScriptableObject.CreateInstance<T>();
        string directoryPath = "Assets";

        if (Selection.activeObject)
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (Directory.Exists(assetPath))
            {
                directoryPath = assetPath;
            }
            else
            {
                directoryPath = Path.GetDirectoryName(assetPath);
            }
        }
        stateMachineController.name = "New" + stateMachineController.GetType().Name;
        string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(directoryPath + "/" + stateMachineController.name + ".asset");
        AssetDatabase.CreateAsset(stateMachineController, uniqueAssetPath);
        M stateMachine = ScriptableObject.CreateInstance<M>();
        stateMachine.name = "NewStateMachine";
        stateMachine.AddState("New State");
        AssetDatabase.AddObjectToAsset(stateMachine, uniqueAssetPath);
        stateMachineController.name = "Settings";
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void Sync(M stateMachine)
    {
        //        string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        //        Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        //
        //        foreach (Object o in objects)
        //        {
        //            if (o is M)
        //            {
        //                M stateM = (M)o;
        //                EditorUtility.SetDirty(stateM);
        //                EditorUtility.SetDirty(this);
        //                AssetDatabase.SaveAssets();
        //                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        //            }
        //        }
    }


}
