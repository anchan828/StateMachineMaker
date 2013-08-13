using System;
using StateMachineMaker;
#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class ExampleStateMachineController :
    StateMachineController<ExampleStateMachine, ExampleState, ExampleTransition>
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/ExampleStateMachineController")]
    private static void Create()
    {
        CreateAssets<ExampleStateMachineController>();
    }
#endif
}