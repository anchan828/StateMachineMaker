using System;
using UnityEngine;

namespace Kyusyukeigo.StateMachine
{
    [Serializable]
    public class Transition
    {
        /// <summary>
        /// Transiiton名
        /// </summary>
        public string name;
        /// <summary>
        /// StateのユニークID
        /// </summary>
        public int fromStateUniqueID, toStateNameUniqueID;
        public bool selected;
        /// <summary>
        /// parameterのKey名
        /// </summary>
        public string parameterKey;

        /// <summary>
        /// 遷移する必要条件
        /// 設定されているパラメータタイプ
        /// </summary>
        public ParameterType parameterType;

        /// <summary>
        /// 遷移する必要条件
        /// </summary>
        public Necessary necessary;

        /// <summary>
        /// 遷移するときの必要条件値
        /// </summary>
        public string necessaryValueString;
        public int necessaryValueInt;
        public float necessaryValueFloat;
        public Vector2 necessaryValueVector2;
        public Vector3 necessaryValueVector3;
    }

   

    [Serializable]
    public enum Necessary
    {
        False = 0,
        True = 1,
        Greater = 2,
        GreaterOrEqual = 3,
        Less = 4,
        LessOrEqual = 5,
    }
}