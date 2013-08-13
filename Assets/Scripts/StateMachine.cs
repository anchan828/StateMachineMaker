using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System;

namespace StateMachineMaker
{
    [Serializable]
    public class StateMachine<S, T>
        where S : State
        where T : Transition
    {
        public string name;
        public string uniqueID = Guid.NewGuid().ToString();
        [SerializeField, HideInInspector]
        private List<S> states = new List<S>();
        [SerializeField, HideInInspector]
        private List<T> transitions = new List<T>();
        [SerializeField, HideInInspector]
        public List<StateMachineParameter> parameters = new List<StateMachineParameter>();
        /// <summary>
        /// StateMachine内にあるStateの数
        /// </summary>
        public int stateCount
        {
            get
            {
                return states.Count;
            }
        }

        [NonSerialized]
        private int selectedState = -1;

        private S _currentState = null;
        /// <summary>
        /// 現在選択中のStateMachineを取得します
        /// </summary>
        public S currentState
        {
            get
            {
                if (states.Count == 0) return null;

                S state = null;
                if (selectedState == -1)
                {
                    state = states[0];
                    selectedState = 0;
                    for (int index = 0; index < states.Count; index++)
                    {
                        S _states = states[index];
                        if (_states.isDefault)
                        {
                            selectedState = index;
                            state = _states;
                            break;
                        }
                    }
                    if (selectedState == 0)
                    {
                        SetDefault(state);
                    }
                }
                else
                {
                    state = GetState(selectedState);
                }
                if (state == null)
                {
                    Debug.LogError("Stateがありません");
                }
                return state;
            }
        }

        public void SetCurrentState(S state)
        {
            selectedState = states.IndexOf(state);
        }

        /// <summary>
        /// StateMachine内にあるTransitionの数
        /// </summary>
        public int transitionCount
        {
            get
            {
                return transitions.Count;
            }
        }

        /// <summary>
        /// StateMachine内のStateをすべて取得します
        /// </summary>
        public List<S> GetAllStates()
        {
            return states;
        }

        /// <summary>
        /// StateMachine内のTransitionをすべて取得します
        /// </summary>
        public List<T> GetAllTransitions()
        {
            return transitions;
        }



        /// <summary>
        /// Stateに付加しているTransitionをすべて取得します
        /// </summary>
        public List<T> GetTransitionOfState(S state)
        {
            List<T> list = new List<T>();
            list.AddRange(GetTransitionOfFromState(state));
            list.AddRange(GetTransitionOfToState(state));
            return list;
        }

        /// <summary>
        /// Stateへ移動してくるTransitionをすべて取得します
        /// </summary>
        public List<T> GetTransitionOfFromState(S state)
        {
            List<T> list = new List<T>();
            foreach (T transition in transitions)
            {
                if (transition.toStateNameUniqueID == state.uniqueID)
                {
                    list.Add(transition);
                }
            }
            return list;
        }
        /// <summary>
        /// 別のStateへ移動するTrnsitionをすべて取得します
        /// </summary>
        public List<T> GetTransitionOfToState(S state)
        {
            List<T> list = new List<T>();
            foreach (T transition in transitions)
            {
                if (transition.fromStateUniqueID == state.uniqueID)
                {
                    list.Add(transition);
                }
            }
            return list;
        }
        /// <summary>
        /// StateNameでStateを作成＆追加します
        /// </summary>
        /// <param name="stateName">すでに同じ名前があるときユニークな名前に変更される(例 Hoge -> Hoge 1)</param>
        public S AddState(string stateName)
        {
            S state = Activator.CreateInstance<S>();
            state.stateName = GetUniqName(stateName);
            return AddState(state);
        }

        /// <summary>
        /// StateNameでStateを作成＆追加します
        /// </summary>
        /// <param name="stateName">すでに同じ名前があるときユニークな名前に変更される(例 Hoge -> Hoge 1)</param>
        public S[] AddState(params string[] stateNames)
        {
            var _states = new List<S>();

            foreach (var stateName in stateNames)
            {
                S state = Activator.CreateInstance<S>();
                state.stateName = GetUniqName(stateName);
                _states.Add(AddState(state));
            }
            return _states.ToArray();
        }

        /// <summary>
        /// Stateを追加します
        /// すでに同じ名前があるときユニークな名前に変更される(例 Hoge -> Hoge 1)
        /// </summary>
        public S AddState(S state)
        {
            state.stateName = GetUniqName(state);
            states.Add(state);
            return state;
        }

        /// <summary>
        /// Stateを追加します
        /// すでに同じ名前があるときユニークな名前に変更される(例 Hoge -> Hoge 1)
        /// </summary>
        public S[] AddState(params S[] states)
        {
            foreach (var state in states)
            {
                AddState(state);
            }
            return states;
        }

        /// <summary>
        /// indexからStateを取得します
        /// </summary>
        /// <param name="index"></param>
        public S GetState(int index)
        {
            return states.Count == 0 ? null : states[index];
        }

        /// <summary>
        /// StateNameからStateを取得します
        /// </summary>
        public S GetState(string stateName)
        {
            return states.First(state => state.stateName == stateName);
        }
        /// <summary>
        /// ユニークIDからStateへ変換します
        /// </summary>
        /// <param name="uniqueID"></param>
        /// <returns></returns>
        public S UniqueIDToState(string uniqueID)
        {
            return states.First(state => state.uniqueID == uniqueID);
        }

        /// <summary>
        /// StateNameからStateがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasState(string stateName)
        {
            return states.Count(state => state.stateName == stateName) != 0;
        }

        /// <summary>
        /// uniqueIDからStateがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasStateFromUniqueID(string uniqueID)
        {
            return states.Count(state => state.uniqueID == uniqueID) != 0;
        }

        /// <summary>
        /// StateがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasState(S state)
        {
            return HasState(state.stateName);
        }

        /// <summary>
        /// StateMachine内に存在するStateを削除します
        /// </summary>
        public void RemoveState(S state)
        {
            states.Remove(state);
        }

        /// <summary>
        /// StateMachine内に存在するTransitionを削除します
        /// </summary>
        public void RemoveTransition(T transition)
        {
            transitions.Remove(transition);
        }

        /// <summary>
        /// indexからStateMachine内に存在するTransitionを削除します
        /// </summary>
        public void RemoveTransition(int index)
        {
            transitions.RemoveAt(index);
        }

        /// <summary>
        /// デフォルトを設定する
        /// FIXME isDefaultがreadonlyではないのでどうにかする
        /// </summary>
        /// <param name="state"></param>
        public void SetDefault(S state)
        {
            foreach (S s in states)
            {
                s.isDefault = state.stateName == s.stateName;
            }
        }

        /// <summary>
        /// fromからtoへのTransitionを作成＆追加します
        /// </summary>
        public T AddTransition(S from, S to)
        {
            T transition = Activator.CreateInstance<T>();
            transition.fromStateUniqueID = @from.uniqueID;
            transition.toStateNameUniqueID = to.uniqueID;
            transition.name = GetUniqName(@from, to);
            transitions.Add(transition);
            return transition;
        }


        /// <summary>
        /// fromからtoへのTransitionを作成＆追加します
        /// </summary>
        public T AddTransition(string fromStateName, string toStateName)
        {
            S from = HasState(fromStateName) ? GetState(fromStateName) : AddState(fromStateName);
            S to = HasState(toStateName) ? GetState(toStateName) : AddState(toStateName);
            return AddTransition(from, to);
        }


        /// <summary>
        /// fromからtoへのTransitionがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasTransitionFromUniqueID(string fromStateUniqID, string toStateNameUniqID)
        {
            if (!HasStateFromUniqueID(fromStateUniqID) || !HasStateFromUniqueID(toStateNameUniqID))
            {
                return false;
            }
            return HasTransition(UniqueIDToState(fromStateUniqID), UniqueIDToState(toStateNameUniqID));
        }

        /// <summary>
        /// fromからtoへのTransitionがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasTransition(S from, S to)
        {
            return
                transitions.Count(
                    transition =>
                        (transition.fromStateUniqueID == from.uniqueID
                            && transition.toStateNameUniqueID == to.uniqueID)) != 0;
        }

        /// <summary>
        /// indexからTransitionを取得します
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetTransition(int index)
        {
            T transition = transitions[index];
            return HasTransitionFromUniqueID(transition.fromStateUniqueID, transition.toStateNameUniqueID)
                ? transition
                : null;
        }

        /// <summary>
        /// ユニークなState名を取得します
        /// </summary>
        private string GetUniqName(S state)
        {
            return GetUniqName(state.stateName);
        }

        /// <summary>
        /// ユニークなState名を取得します
        /// </summary>
        private string GetUniqName(string stateName)
        {
            int count = states.Count(s => s.stateName.StartsWith(stateName));
            return count == 0 ? stateName : stateName + " " + count;
        }

        /// <summary>
        /// ユニークなState名を取得します
        /// </summary>
        private string GetUniqName(S from, S to)
        {
            string transitionName = from.stateName + "-" + to.stateName;
            int count = transitions.Count(transition => transition.name.StartsWith(transitionName));
            return count == 0 ? transitionName : transitionName + " " + count;
        }

        /// <summary>
        /// ユニークなRect値を設定します
        /// </summary>
        private Rect GetPosition(Rect position)
        {
            Rect pos = new Rect(position);
            if (states.Count(state => (state.position.x == position.x) && (state.position.y == position.y)) != 0)
            {
                pos.x += pos.width * 0.5f;
                pos.y += pos.height * 0.5f;
                return GetPosition(pos);
            }
            return pos;
        }

        public ParameterType GetParameterType(string key)
        {
            return GetParameter(key).parameterType;
        }

        public void SetString(string key, string value)
        {
            var parameter = GetParameter(key);
            parameter.value = value;
            parameter.parameterType = ParameterType.String;
        }

        public void SetBool(string key, bool value)
        {
            var parameter = GetParameter(key);
            parameter.value = value;
            parameter.parameterType = ParameterType.Bool;
        }
        public void SetInt(string key, int value)
        {
            var parameter = GetParameter(key);
            parameter.value = value;
            parameter.parameterType = ParameterType.Int;
        }
        public void SetFloat(string key, float value)
        {
            var parameter = GetParameter(key);
            parameter.value = value;
            parameter.parameterType = ParameterType.Float;
        }
        public void SetVector2(string key, Vector2 value)
        {
            var parameter = GetParameter(key);
            parameter.value = value;
            parameter.parameterType = ParameterType.Vector2;
        }
        public void SetVector3(string key, Vector3 value)
        {
            var parameter = GetParameter(key);
            parameter.value = value;
            parameter.parameterType = ParameterType.Vector3;
        }

        public string GetString(string key)
        {
            return (string)GetParameter(key).value ?? "";
        }
        public bool GetBool(string key)
        {
            object val = GetParameter(key).value ?? false;
            return (bool)val;
        }
        public int GetInt(string key)
        {
            object val = GetParameter(key).value ?? 0;
            return (int)val;
        }
        public float GetFloat(string key)
        {
            object val = GetParameter(key).value ?? 0f;
            return (float)val;
        }
        public Vector2 GetVector2(string key)
        {
            object val = GetParameter(key).value ?? Vector2.zero;
            return (Vector2)val;
        }
        public Vector3 GetVector3(string key)
        {
            object val = GetParameter(key).value ?? Vector3.zero;
            return (Vector3)val;
        }

        private StateMachineParameter GetParameter(string key)
        {
            StateMachineParameter parameter = null;
            try
            {
                parameter = parameters.First(paramater => paramater.name == key);
            }
            catch (InvalidOperationException)
            {
                parameter = CreteStateMachineParameter(key);
            }
            return parameter;
        }

        private StateMachineParameter CreteStateMachineParameter(string key)
        {
            var parameter = new StateMachineParameter(key);
            parameters.Add(parameter);
            return parameter;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (S state in GetAllStates())
            {
                sb.AppendLine(state.ToString());
            }

            foreach (T transition in GetAllTransitions())
            {
                sb.AppendLine(transition.ToString());
            }

            foreach (StateMachineParameter parameter in parameters)
            {
                sb.AppendLine(parameter.ToString());
            }

            return sb.ToString();
        }
    }
}