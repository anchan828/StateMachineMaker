using StateMachineMaker;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (ExampleStateMachineController))]
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