using System;
#if !UNITY_3_5
using StateMachineMaker;
#endif

[Serializable]
public class ExampleStateMachine : StateMachine<ExampleState, ExampleTransition>
{
}