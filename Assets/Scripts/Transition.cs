using System;
using System.Text;

namespace StateMachineMaker
{
    [Serializable]
    public class Transition
    {
        /// <summary>
        ///     StateのユニークID
        /// </summary>
        public string fromStateUniqueID;

        /// <summary>
        ///     Transiiton名
        /// </summary>
        public string name;

        /// <summary>
        ///     遷移する必要条件
        /// </summary>
        public Necessary necessary;

        /// <summary>
        ///     遷移するときの必要条件値
        /// </summary>
        public object necessaryValue;

        /// <summary>
        ///     parameterのKey名
        /// </summary>
        public string parameterKey;

        /// <summary>
        ///     遷移する必要条件
        ///     設定されているパラメータタイプ
        /// </summary>
        public ParameterType parameterType;

        public bool selected;

        /// <summary>
        ///     StateのユニークID
        /// </summary>
        public string toStateNameUniqueID;

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