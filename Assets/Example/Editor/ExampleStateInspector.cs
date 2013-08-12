using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(StateNode))]
public class ExampleStateInspector : StateInspector<ExampleStateMachine, ExampleState, ExampleTransition>
{
    public override void OnStateGUI(ExampleStateMachine stateMachine, ExampleState state)
    {
    }
}
