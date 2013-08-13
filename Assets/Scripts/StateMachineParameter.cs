using System;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
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
#if !UNITY_3_5
}
#endif