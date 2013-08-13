#if !UNITY_3_5
using StateMachineMaker;
#endif
using UnityEditor;
using UnityEngine;

public class ExampleWindow : StateMachineWindow<ExampleStateMachine, ExampleState, ExampleTransition>
{
    [MenuItem("Window/ExampleWindow")]
    private static void Open()
    {
        GetWindow<ExampleWindow>();
    }

    public override void OnStateGUI(ExampleState state)
    {
        var guiStyle = new GUIStyle(EditorStyles.label);


#if UNITY_3_5
        EditorGUIUtility.LookLikeControls(60);
#endif
        GUILayout.Label("ヾ(๑╹◡╹)ﾉ”", guiStyle);
        GUILayout.Label(state.position.ToString(), guiStyle);

        state.color = (StateColor)EditorGUILayout.EnumPopup("Color", state.color);
    }

    public override Vector2 GetStateSize(ExampleState state)
    {
        return new Vector2(200, 100);
    }
}