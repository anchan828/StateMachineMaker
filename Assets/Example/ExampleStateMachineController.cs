using StateMachineMaker;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ExampleStateMachineController : StateMachineController<ExampleStateMachine, ExampleState, ExampleTransition>
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/ExampleStateMachineController")]
    static void Create()
    {
        CreateAssets<ExampleStateMachineController>();
    }
#endif
}
