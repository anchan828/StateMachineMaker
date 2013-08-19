using System;
#if !UNITY_3_5
using StateMachineMaker;
#endif
#if UNITY_EDITOR
using UnityEditor;

#endif

[Serializable]
public class ExampleController :
    StateMachineController<ExampleStateMachine, ExampleState, ExampleTransition>
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/ExampleController")]
    private static void Create()
    {
        CreateAssets<ExampleController>();
    }
#endif
}