using System;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class Transition
    {
        public string transitionName;
        public string fromStateName, toStateName;
    }
}