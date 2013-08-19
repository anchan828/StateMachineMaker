#if !UNITY_3_5
using StateMachineMaker.Editor;
using StateMachineMaker;
#endif
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (ExampleController))]
public class ExampleStateMachineControllerInspector : StateMachineControllerInspector
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Show StateMachineWindow"))
        {
            EditorWindow.GetWindow<ExampleWindow>();
        }
        base.OnInspectorGUI();
    }
}