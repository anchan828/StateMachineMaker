using StateMachineMaker;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (StateNode))]
public class ExampleStateInspector : StateInspector<ExampleStateMachine, ExampleState, ExampleTransition>
{
    public override void OnStateGUI(ExampleStateMachine stateMachine, ExampleState state)
    {
        GUILayout.Label(@"　　　　　　　　　　　　○。　　○ 
　　　　ミﾊｯｸｼｭ　　　○　　　　ｏ　　　○ 
　　　 ミ　｀д´∵°　。　ｏ　○ 
　　.c(,_ｕｕﾉ 　○　○　　　○ 
");
    }
}