
using System;
using UnityEngine;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class StateMachineParameter
    {
        public StateMachineParameter(string name)
        {
            this.name = name;
        }

        public string name;

        public ParameterType parameterType;
        public string stringValue;
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
    }

    [Serializable]
    public enum ParameterType
    {
        String, Bool, Int, Float, Vector2, Vector3,
    }
}