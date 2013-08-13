using System;
using UnityEngine;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    [Serializable]
    public class State
    {
        /// <summary>
        ///     EditorWindow上でStateを表示するときの大きさ
        /// </summary>
        public static readonly float Width = 170, Height = 60;

        public StateColor color = StateColor.Grey;

        /// <summary>
        ///     デフォルトの場合はtrueを返す
        ///     必ずStateMachineにデフォルトは１つ
        /// </summary>
        public bool isDefault;

        public Rect position = new Rect(0, 0, Width, Height);

        /// <summary>
        ///     State名
        /// </summary>
        public string stateName;

        /// <summary>
        ///     ユニークID
        /// </summary>
        public string uniqueID = Guid.NewGuid().ToString();

        public override string ToString()
        {
            return string.Format("[State] {0} {1} {2}", stateName, uniqueID, position);
        }

        public object Clone()
        {
            var state = (State) MemberwiseClone();
            state.uniqueID = Guid.NewGuid().ToString();
            return state;
        }
    }
#if !UNITY_3_5
}
#endif