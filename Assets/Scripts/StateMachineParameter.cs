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
    }
#if !UNITY_3_5
}
#endif