using System;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class Transition : System.Object
    {
        public string transitionName;
        public string fromStateName, toStateName;
    }
}