using System;
using UnityEngine;

namespace StateMachineMaker
{
    public class StateNode : ScriptableObject
    {
        public Type controllerType;
        public string stateID;
        public object stateMachine;
    }
}