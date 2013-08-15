using System;
#if !UNITY_3_5
using StateMachineMaker;
#endif
using UnityEngine;

[Serializable]
public class ExampleState : State
{
    public string hogehooge;
    public Texture2D texture;
}