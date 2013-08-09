using System;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class Transition : System.Object
    {
        public string transitionName;
        public long fromStateUniqID, toStateNameUniqID;


        public bool selected;


        public string parameterKey;
        public int selectedParameter = 0;

        public Necessary necessary;
        public float necessaryValue;
        public NecessaryBool necessaryBool;

    }

    [Serializable]
    public enum Necessary
    {
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual,
    }

    [Serializable]
    public enum NecessaryBool
    {
        True,
        False,
    }

}