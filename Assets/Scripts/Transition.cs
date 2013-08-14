using System;
using System.Text;
using UnityEngine;

#if !UNITY_3_5
namespace StateMachineMaker
{
#endif

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
        public object necessaryValue
        {
            get
            {
                object obj = null;
                switch (parameterType)
                {
                    case ParameterType.String:
                        obj = stringValue;
                        break;
                    case ParameterType.Bool:
                        obj = boolValue;
                        break;
                    case ParameterType.Int:
                        obj = intValue;
                        break;
                    case ParameterType.Float:
                        obj = floatValue;
                        break;
                    case ParameterType.Vector2:
                        obj = vector2Value;
                        break;
                    case ParameterType.Vector3:
                        obj = vector3Value;
                        break;
                }
                return obj;
            }
            set
            {
                switch (parameterType)
                {
                    case ParameterType.String:
                        stringValue = (string)value;
                        break;
                    case ParameterType.Bool:
                        boolValue = (bool)value;
                        break;
                    case ParameterType.Int:
                        intValue = (int)value;
                        break;
                    case ParameterType.Float:
                        floatValue = (float)value;
                        break;
                    case ParameterType.Vector2:
                        vector2Value = (Vector2)value;
                        break;
                    case ParameterType.Vector3:
                        vector3Value = (Vector3)value;
                        break;
                }
                ToString();
            }
        }

        [SerializeField,HideInInspector]
        private string stringValue;
        [SerializeField, HideInInspector]
        private bool boolValue;
        [SerializeField, HideInInspector]
        private int intValue;
        [SerializeField, HideInInspector]
        private float floatValue;
        [SerializeField, HideInInspector]
        private Vector2 vector2Value;
        [SerializeField, HideInInspector]
        private Vector3 vector3Value;

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
            sb.AppendLine(necessaryValue.ToString());
            return sb.ToString();
        }
    }
#if !UNITY_3_5
}
#endif