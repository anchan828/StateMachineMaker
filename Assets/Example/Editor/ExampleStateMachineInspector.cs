using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(ExampleStateMachine))]
public class ExampleStateMachineInspector : StateMachineInspector
{
    public override void OnStateGUI<M, S, T>(M stateMachine, S state)
    {
        GUILayout.Label(@"　　　　　　　　　　　　○。　　○ 
　　　　ミﾊｯｸｼｭ　　　○　　　　ｏ　　　○ 
　　　 ミ　｀д´∵°　。　ｏ　○ 
　　.c(,_ｕｕﾉ 　○　○　　　○ 
");
    }
}