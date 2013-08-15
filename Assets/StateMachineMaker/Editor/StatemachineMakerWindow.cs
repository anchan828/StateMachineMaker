using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using UnityEditor;
public class StatemachineMakerWindow : ExampleWindow
{
    private string stateName;
    Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();

    private string errorString = "";

    [MenuItem("Window/StateMachineMaler")]
    static void Open()
    {
        GetWindow<StatemachineMakerWindow>();
    }
    string[] iconNames = new string[] { "FolderEmpty", "Folder", "cs Script", "js Script" };
    private ScriptType scriptType;
    private enum ScriptType
    {
        csScript = 0,
        jsScript = 1
    }
    private enum FolderType
    {
        FolderEmpty = 0,
        Folder = 1,
    }
    void OnEnable()
    {
        foreach (var iconName in iconNames)
        {
            if (!icons.ContainsKey(iconName))
            {
                icons.Add(iconName, EditorGUIUtility.Load(string.Format("Icons/Generated/{0} Icon.asset", iconName)) as Texture2D);
            }
        }
    }

    void OnGUI()
    {
        GUILayout.Label("StateMachine Maker", EditorStyles.largeLabel);

        

        var isEmpty = OnTextFieldGUI();
        if (!string.IsNullOrEmpty(errorString))
        {
            EditorGUILayout.HelpBox(errorString, MessageType.Error, false);
        }
        OnScriptTypeGUI();
        OnPreviewGUI(isEmpty);
        if (string.IsNullOrEmpty(errorString))
            OnCreateButtonGUI();
    }

    private void OnCreateButtonGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(Screen.width * 0.8f);
        if (GUILayout.Button("Create", GUILayout.Width(Screen.width * 0.18f)))
        {
            //TODO CreateScript
        }
    }

    private void OnScriptTypeGUI()
    {
        scriptType = (ScriptType)EditorGUILayout.EnumPopup(scriptType);
    }

    private void OnPreviewGUI(bool isEmpty)
    {
        EditorGUILayout.LabelField(new GUIContent("Assets", icons[(isEmpty ? FolderType.FolderEmpty : FolderType.Folder).ToString()]));

        if (!isEmpty)
        {
            OnFolderGUI(stateName, 1);
            {
                OnFolderGUI("Editor", 2);
                {
                    OnFileGUI(stateName + "StateInspector", 3);
                    OnFileGUI(stateName + "StateMachineControllerInspector", 3);
                    OnFileGUI(stateName + "Window", 3);
                }
                OnFileGUI(stateName + "State", 2);
                OnFileGUI(stateName + "StateMachine", 2);
                OnFileGUI(stateName + "StateController", 2);
                OnFileGUI(stateName + "MonoBehavior", 2);
                OnFileGUI(stateName + "Transition", 2);
            }
        }
    }

    private bool OnTextFieldGUI()
    {
        EditorGUI.BeginChangeCheck();
        string value = EditorGUILayout.TextField("StateMachine Name", stateName);
        if (EditorGUI.EndChangeCheck())
        {
            if (!string.IsNullOrEmpty(value))
                errorString = Directory.Exists("Assets/" + value) ? value + "はすでに存在します。名前を変更してください" : string.Empty;
            stateName = value;
        }
        return string.IsNullOrEmpty(stateName);
    }

    void OnFolderGUI(string stateName, int indent)
    {
        EditorGUI.indentLevel = indent;
        EditorGUILayout.LabelField(new GUIContent(stateName, icons[FolderType.Folder.ToString()]));
    }
    void OnFileGUI(string stateName, int indent)
    {
        EditorGUI.indentLevel = indent;
        EditorGUILayout.LabelField(new GUIContent(stateName, icons.Values.ToArray()[((int)scriptType) + 2]));
    }
}
