using System;
using UnityEngine;

public class StateNode : ScriptableObject
{
    public string stateID;
    public object stateMachine;
    public Type controllerType;
}
