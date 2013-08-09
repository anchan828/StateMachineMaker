using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(StateMachineController<StateMachine<State, Transition>, State, Transition>))]
public class StateMachineControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Add StateMachine"))
        {
            target.GetType().GetMethod("AddStateMachine").Invoke(target, new object[] {"Hoge"});
        }
    }
}
