using System.Collections.Generic;
using StateMachineMaker;
using UnityEngine;
using UnityEditor;
public class ExampleWindow : StateMachineWindow<ExampleStateMachine, ExampleState, ExampleTransition>
{

    [MenuItem("Window/ExampleWindow")]
    static void Open()
    {
        GetWindow<ExampleWindow>();
    }

    public override void OnStateGUI(ExampleState state)
    {
        GUILayout.Label("ヾ(๑╹◡╹)ﾉ”");
        GUILayout.Button("okok");

        state.texture = (Texture2D)EditorGUILayout.ObjectField(state.texture, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));
    }

    public override float GetStateHeight(ExampleState state)
    {
        return 150;
    }
}