using System;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    [Serializable]
    public enum ParameterType
    {
        String,
        Bool,
        Int,
        Float,
        Vector2,
        Vector3
    }
#if !UNITY_3_5
}
#endif