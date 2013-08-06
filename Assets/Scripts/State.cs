using System;
using UnityEngine;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class State : ICloneable
    {
        /// <summary>
        /// State名
        /// </summary>
        public string stateName;

#if UNITY_EDITOR

        /// <summary>
        /// EditorWindow上でStateを表示するときの大きさ
        /// </summary>
        public static readonly float Width = 170, Height = 60;
        public Rect position = new Rect(0, 0, Width, Height);

        public override string ToString()
        {
            return string.Format("[State] {0} {1}", stateName, position);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

#endif
    }
}