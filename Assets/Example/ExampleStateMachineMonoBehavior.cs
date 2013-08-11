using Kyusyukeigo.StateMachine;
using UnityEngine;
using System.Collections;

public class ExampleStateMachineMonoBehavior
    : StateMachineMonoBehaviour<ExampleStateMachineController,
    ExampleStateMachine,
    ExampleState,
    ExampleTransition>
{

    void OnGUI()
    {
        int i = stateMachineController.currentStateMachine.GetInt("intval");
        GUILayout.Label(i.ToString());
        float horizontalSlider = GUILayout.HorizontalSlider(i, 0, 11, GUILayout.Width(Screen.width));
        if (GUI.changed)
        {
            stateMachineController.currentStateMachine.SetInt("intval", (int)horizontalSlider);
        }
    }

    protected override bool OnWillTransition(State @from, State to)
    {
        return true;
    }

    protected override void MovedState(State currentState)
    {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        primitive.AddComponent<Rigidbody>();
    }
}
