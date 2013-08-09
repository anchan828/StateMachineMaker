using Kyusyukeigo.StateMachine;
using UnityEngine;
using UnityEditor;
public class ExampleWindow : StateMachineWindow<ExampleStateMachine, ExampleState, ExampleTransition>
{

    [MenuItem("Window/ExampleWindow")]
    static void Open()
    {
        GetWindow<ExampleWindow>();
    }


    protected override void OnStateGUI(ExampleState state)
    {
        GUILayout.Label("ヾ(๑╹◡╹)ﾉ”");
    }
}