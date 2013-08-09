using System.IO;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class ExampleStateMachineController : StateMachineController<ExampleStateMachine, ExampleState, ExampleTransition>
{
    [MenuItem("Assets/Create/ExampleStateMachineController")]
    static void Create()
    {
        CreateAssets<ExampleStateMachineController>();
    }

}
