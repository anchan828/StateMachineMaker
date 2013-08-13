using System;

namespace StateMachineMaker
{
    [Serializable]
    public class StateMachineParameter
    {
        public string name;
        public ParameterType parameterType;
        public object value;

        public StateMachineParameter(string name)
        {
            this.name = name;
        }
    }
}