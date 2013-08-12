using Kyusyukeigo.StateMachine;
using UnityEngine;
using System.Collections;

public class ExampleStateMachineMonoBehavior
    : StateMachineMonoBehaviour<ExampleStateMachineController,
    ExampleStateMachine,
    ExampleState,
    ExampleTransition>
{


    public GameObject quad;
    private int time;
    IEnumerator Start()
    {
        quad.renderer.sharedMaterial.mainTexture = stateMachineController.currentStateMachine.currentState.texture;
      
        while (true)
        {
            yield return new WaitForSeconds(1);
            stateMachineController.currentStateMachine.SetInt("intval", ++time);
        }
       
    }

   


    protected override bool OnWillTransition(ExampleState exampleState, ExampleState state)
    {
        return true;
    }

    protected override void MovedState(ExampleState exampleState)
    {
        Debug.Log(exampleState);
        if (exampleState.isDefault)
        {
            stateMachineController.currentStateMachine.SetInt("intval", 0);
            time = 0;
        }

        quad.renderer.sharedMaterial.mainTexture = exampleState.texture;

    }
}
