using System;
using UnityEngine;

namespace StateMachineMaker
{
    [Serializable]
    public class State
    {
        /// <summary>
        /// State名
        /// </summary>
        public string stateName;

        /// <summary>
        /// デフォルトの場合はtrueを返す
        /// 必ずStateMachineにデフォルトは１つ
        /// </summary>
        public bool isDefault;

        /// <summary>
        /// ユニークID
        /// </summary>
        public string uniqueID = Guid.NewGuid().ToString();

        /// <summary>
        /// EditorWindow上でStateを表示するときの大きさ
        /// </summary>
        public static readonly float Width = 170, Height = 60;
        public Rect position = new Rect(0, 0, Width, Height);

        public StateColor color = StateColor.Grey;

        public override string ToString()
        {
            return string.Format("[State] {0} {1} {2}", stateName, uniqueID, position);
        }

        public object Clone()
        {
            State state = (State)MemberwiseClone();
            state.uniqueID = Guid.NewGuid().ToString();
            return state;
        }
    }
}