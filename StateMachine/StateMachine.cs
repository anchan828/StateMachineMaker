using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class StateMachine<S, T>
        where S : State
        where T : Transition
    {
        [SerializeField]
        private List<S> states = new List<S>();
        [SerializeField]
        private List<T> transitions = new List<T>();

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
            List<S> _states = new List<S>();

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
            state.position = SetPosition(state.position);
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
        /// StateNameからStateがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasState(string stateName)
        {
            return states.Count(state => state.stateName == stateName) != 0;
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
        /// fromからtoへのTransitionを作成＆追加します
        /// </summary>
        public T AddTransition(S from, S to)
        {
            T transition = Activator.CreateInstance<T>();
            transition.fromStateName = @from.stateName;
            transition.toStateName = to.stateName;
            transition.transitionName = GetUniqName(@from, to);
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
        public bool HasTransition(string fromStateName, string toStateName)
        {
            if (!HasState(fromStateName) || !HasState(toStateName))
            {
                return false;
            }
            return HasTransition(GetState(fromStateName), GetState(toStateName));
        }

        /// <summary>
        /// fromからtoへのTransitionがStateMachine内に存在するか確認します
        /// </summary>
        public bool HasTransition(S from, S to)
        {
            return
                transitions.Count(
                    transition =>
                        (transition.fromStateName == from.stateName
                            && transition.toStateName == to.stateName)) != 0;
        }

        /// <summary>
        /// indexからTransitionを取得します
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetTransition(int index)
        {
            T transition = transitions[index];
            return HasTransition(transition.fromStateName, transition.toStateName)
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
            int count = transitions.Count(transition => transition.transitionName.StartsWith(transitionName));
            return count == 0 ? transitionName : transitionName + " " + count;
        }

        /// <summary>
        /// ユニークなRect値を設定します
        /// </summary>
        private Rect SetPosition(Rect position)
        {
            Rect pos = new Rect(position);
            if (states.Count(state => (state.position.x == position.x) && (state.position.y == position.y)) != 0)
            {
                pos.x += pos.width * 0.5f;
                pos.y += pos.height * 0.5f;
                return SetPosition(pos);
            }
            return pos;
        }
    }
}