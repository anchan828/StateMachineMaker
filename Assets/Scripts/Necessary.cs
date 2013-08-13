using System;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    [Serializable]
    public enum Necessary
    {
        False = 0,
        True = 1,
        Greater = 2,
        GreaterOrEqual = 3,
        Less = 4,
        LessOrEqual = 5,
    }
#if !UNITY_3_5
}
#endif