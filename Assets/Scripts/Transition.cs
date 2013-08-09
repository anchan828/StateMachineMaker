using System;
namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class Transition
    {
        public string name;
        public int fromStateUniqueID, toStateNameUniqueID;
        public bool selected;
        public string parameterKey;
        public int selectedParameter;
        public float necessaryValue;
        public Necessary necessary;
        public NecessaryBool necessaryBool;

    }


    [Serializable]
    public enum Necessary
    {
        Greater = 0,
        GreaterOrEqual = 1,
        Less = 2,
        LessOrEqual = 3,
    }

    [Serializable]
    public enum NecessaryBool
    {
        True = 0,
        False = 1,
    }
}