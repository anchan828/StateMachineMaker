using System.IO;
using Kyusyukeigo.StateMachine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

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
