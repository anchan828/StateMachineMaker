using System.Collections;
using System.Reflection;
#if !UNITY_3_5
using StateMachineMaker;
#endif
using UnityEngine;

public class ExampleStateMachineMonoBehavior
    : StateMachineMonoBehaviour<ExampleStateMachineController,
        ExampleStateMachine,
        ExampleState,
        ExampleTransition>
{
    public GameObject quad;
    private int time;

    private IEnumerator Start()
    {
        quad.renderer.sharedMaterial.mainTexture = stateMachineController.currentStateMachine.currentState.texture;

        while (true)
        {
            yield return new WaitForSeconds(1);
            stateMachineController.currentStateMachine.SetInt("New Int", (int)Mathf.Repeat(++time, 10));
        }
    }


    protected override bool OnWillTransition(ExampleState exampleState, ExampleState state)
    {
        return true;
    }

    protected override void MovedState(ExampleState exampleState)
    {
        if (exampleState.isDefault)
        {
            stateMachineController.currentStateMachine.SetInt("New Int", 0);
            time = 0;
        }

        quad.renderer.sharedMaterial.mainTexture = exampleState.texture;
    }
}