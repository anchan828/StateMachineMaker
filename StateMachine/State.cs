using System;
using UnityEngine;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class State
    {
        /// <summary>
        /// State名
        /// </summary>
        public string stateName;

#if UNITY_EDITOR

        /// <summary>
        /// EditorWindow上でStateを表示するときの大きさ
        /// </summary>
        public static readonly float Width = 170, Height = 30;
        public Rect position = new Rect(0, 0, Width, Height);

        public override string ToString()
        {
            return string.Format("[State] {0} {1}", stateName, position);
        }

#endif
    }
}