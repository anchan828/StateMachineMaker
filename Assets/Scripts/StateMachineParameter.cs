
using UnityEngine;

public class StateMachineParameter
{
    public StateMachineParameter(string name)
    {
        this.name = name;
    }

    public string name;

    public StateMachineParameterType parameterType;
    public string stringValue;
    public bool boolValue;
    public int intValue;
    public float floatValue;
    public Vector2 vector2Value;
    public Vector3 vector3Value;
}

public enum StateMachineParameterType
{
    String, Bool, Int, Float, Vector2, Vector3
}