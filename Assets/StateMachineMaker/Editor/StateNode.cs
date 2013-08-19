using System;
using UnityEngine;

#if !UNITY_3_5
namespace StateMachineMaker.Editor
{
#endif
    public class StateNode : ScriptableObject
    {
        public Type controllerType;
        public string stateID;
        public object stateMachine;
    }
#if !UNITY_3_5
}
#endif