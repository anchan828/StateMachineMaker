using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kyusyukeigo.StateMachine
{
    public abstract class StateMachineMonoBehaviour<T, M, S, TS> : UnityEngine.MonoBehaviour
        where T : StateMachineController<M, S, TS>
        where M : StateMachine<S, TS>
        where S : State
        where TS : Transition
    {
        protected virtual bool OnWillTransition(State from, State to)
        {
            return true;
        }

        protected virtual void MovedState(State currentState)
        {
        }

        public T stateMachineController;

        public virtual void Update()
        {
            if (!stateMachineController)
            {
                Debug.LogError("StateMachineが設定されていません", stateMachineController);
                Debug.Break();
            }

            M currentStateMachine = stateMachineController.currentStateMachine;

            // TODO ここでStateMachineをUpdate
            S currentState = stateMachineController.currentStateMachine.currentState;

            List<TS> transitionOfToState = currentStateMachine.GetTransitionOfToState(currentState);

            foreach (TS transition in transitionOfToState)
            {
                bool qualified = false;
                switch (transition.parameterType)
                {
                    case ParameterType.String:
                        string s = currentStateMachine.GetString(transition.parameterKey);
                        qualified = s == transition.necessaryValueString;
                        break;
                    case ParameterType.Bool:
                        bool b = currentStateMachine.GetBool(transition.parameterKey);
                        qualified = Convert.ToInt32(b) == (int)transition.necessary;
                        break;
                    case ParameterType.Int:
                        int i = currentStateMachine.GetInt(transition.parameterKey);
                        switch (transition.necessary)
                        {
                            case Necessary.Greater:
                                qualified = transition.necessaryValueInt < i;
                                break;
                            case Necessary.GreaterOrEqual:
                                qualified = transition.necessaryValueInt <= i;
                                break;
                            case Necessary.Less:
                                qualified = i < transition.necessaryValueInt;
                                break;
                            case Necessary.LessOrEqual:
                                qualified = i <= transition.necessaryValueInt;
                                break;
                            case Necessary.False:
                            case Necessary.True:
                            default:
                                break;
                        }
                        break;
                    case ParameterType.Float:
                          float f = currentStateMachine.GetFloat(transition.parameterKey);
                        switch (transition.necessary)
                        {
                            case Necessary.Greater:
                                qualified = transition.necessaryValueInt < f;
                                break;
                            case Necessary.GreaterOrEqual:
                                //　FIXME ここ怪しい  Mathf.Approximately()必要？
                                qualified = transition.necessaryValueInt <= f;
                                break;
                            case Necessary.Less:
                                qualified = f < transition.necessaryValueInt;
                                break;
                            case Necessary.LessOrEqual:
                                //　FIXME ここ怪しい  Mathf.Approximately()必要？
                                qualified = f <= transition.necessaryValueInt;
                                break;
                            case Necessary.False:
                            case Necessary.True:
                            default:
                                break;
                        }
                        break;
                    case ParameterType.Vector2:
                        break;
                    case ParameterType.Vector3:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
                if (qualified)
                {
                    Qualified(currentStateMachine, transition);
                }
            }
        }

        private void Qualified(M currentStateMachine, TS transition)
        {
            S from = currentStateMachine.UniqueIDToState(transition.fromStateUniqueID);
            S to = currentStateMachine.UniqueIDToState(transition.toStateNameUniqueID);

            if (OnWillTransition(@from, to))
            {
                currentStateMachine.SetCurrentState(to);
                MovedState(to);
            }
        }
    }
}
