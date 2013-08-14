using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    public abstract class StateMachineMonoBehaviour<T, M, S, TS> : MonoBehaviour
        where T : StateMachineController<M, S, TS>
        where M : StateMachine<S, TS>
        where S : State
        where TS : Transition
    {
        public T stateMachineController;

        protected virtual bool OnWillTransition(S from, S to)
        {
            return true;
        }

        protected virtual void MovedState(S currentState)
        {
        }

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
                        qualified = s == (string)transition.necessaryValue;
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
                                qualified = (int)transition.necessaryValue < i;
                                break;
                            case Necessary.GreaterOrEqual:
                                qualified = (int)transition.necessaryValue <= i;
                                break;
                            case Necessary.Less:
                                qualified = i < (int)transition.necessaryValue;
                                break;
                            case Necessary.LessOrEqual:
                                qualified = i <= (int)transition.necessaryValue;
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
                                qualified = (float)transition.necessaryValue < f;
                                break;
                            case Necessary.GreaterOrEqual:
                                //　FIXME ここ怪しい  Mathf.Approximately()必要？
                                qualified = (float)transition.necessaryValue <= f;
                                break;
                            case Necessary.Less:
                                qualified = f < (float)transition.necessaryValue;
                                break;
                            case Necessary.LessOrEqual:
                                //　FIXME ここ怪しい  Mathf.Approximately()必要？
                                qualified = f <= (float)transition.necessaryValue;
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
                        break;
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
#if UNITY_EDITOR
                var windows = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
                foreach (EditorWindow window in windows)
                {
                    var methodInfo = window.GetType().GetMethod("DrawTransitionMove", BindingFlags.Public | BindingFlags.Instance);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(window, new object[] { from, to });
                    }
                }
#endif

                MovedState(to);
            }
        }
    }
#if !UNITY_3_5
}
#endif