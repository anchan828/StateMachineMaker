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
        GUIStyle guiStyle = new GUIStyle(EditorStyles.label);
        guiStyle.richText = true;
        GUILayout.Label("<color=black>ヾ(๑╹◡╹)ﾉ”</color>", guiStyle);
        Vector2 mousePos = Event.current.mousePosition;
        GUILayout.Label("<color=black>" + state.position.ToString() + "</color>", guiStyle);

        state.color = (StateColor)EditorGUILayout.EnumPopup("Color", state.color);
    }

    public override Vector2 GetStateSize(ExampleState state)
    {
        return new Vector2(200, 100);
    }
}