using System;
using System.Text;
using UnityEngine;

namespace StateMachineMaker
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
        public string fromStateUniqueID, toStateNameUniqueID;

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
        public object necessaryValue;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Transition");
            sb.AppendLine(name);
            sb.AppendLine(parameterType.ToString());
            sb.Append(parameterKey).Append(":").AppendLine(necessaryValue.ToString());
            return sb.ToString();
        }
    }
}