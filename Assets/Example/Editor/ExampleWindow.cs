using Kyusyukeigo.StateMachine;
using UnityEngine;
using UnityEditor;
public class ExampleWindow : StateMachineWindow<ExampleStateMachine, ExampleState, ExampleTransition>
{

    [MenuItem("Window/ExampleWindow")]
    static void Open()
    {
        GetWindow<ExampleWindow>();
    }
}

public class ExampleTransition : Transition
{
}

public class ExampleState : State
{
}

public class ExampleStateMachine : StateMachine<ExampleState, ExampleTransition>
{
}
