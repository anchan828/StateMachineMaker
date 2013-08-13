
using System;
using UnityEngine;

namespace StateMachineMaker
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
        public object value;
    }

}