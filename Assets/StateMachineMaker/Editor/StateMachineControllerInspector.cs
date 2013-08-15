using UnityEditor;
using UnityEngine;
using Object = System.Object;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    [CustomEditor(typeof (StateMachineController<StateMachine<State, Transition>, State, Transition>))]
    public class StateMachineControllerInspector : Editor
    {
        /// <summary>
        ///     Button以外でクリックされたときにtrueを返す
        /// </summary>
        private bool isClicked
        {
            get { return (Event.current.button == 0) && (Event.current.type == EventType.MouseDown); }
        }

        protected int stateMahineCount
        {
            get { return (int) target.GetType().GetProperty("stateMahineCount").GetValue(target, new object[0]); }
        }

        protected Object currentStateMachine
        {
            get { return target.GetType().GetProperty("currentStateMachine").GetValue(target, new object[0]); }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Add StateMachine"))
            {
                AddStateMachine("NewStateMachine");
            }

            for (int i = 0; i < stateMahineCount; i++)
            {
                Object stateMachine = GetStateMeshine(i);
                var style = new GUIStyle("box");

                if (currentStateMachine == stateMachine)
                {
                    style.normal.background = EditorGUIUtility.whiteTexture;
                }

                EditorGUILayout.BeginHorizontal(style);
                GUILayout.Label(i.ToString(), GUILayout.Width(32));
                EditorGUI.BeginChangeCheck();
                string value = EditorGUILayout.TextField(GetStateMachineName(stateMachine));
                if (EditorGUI.EndChangeCheck())
                {
                    SetStateMachineName(stateMachine, value);
                    EditorApplication.RepaintProjectWindow();
                }

                if (i == 0)
                    EditorGUI.BeginDisabledGroup(true);

                if (GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(32)))
                {
                    MoveStateMachine(stateMachine, i - 1);
                    break;
                }
                if (i == 0)
                    EditorGUI.EndDisabledGroup();
                if (i == stateMahineCount - 1)
                    EditorGUI.BeginDisabledGroup(true);
                if (GUILayout.Button("↓", EditorStyles.miniButtonMid, GUILayout.Width(32)))
                {
                    MoveStateMachine(stateMachine, i + 1);
                    break;
                }
                if (i == stateMahineCount - 1)
                    EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(32)))
                {
                    RemoveStateMachine(stateMachine);
                    break;
                }

                EditorGUILayout.EndHorizontal();
                if (isClicked)
                {
                    Rect rect = GUILayoutUtility.GetLastRect();
                    if (rect.y < Event.current.mousePosition.y && Event.current.mousePosition.y < rect.y + rect.height)
                    {
                        SetSelectedStateMachine(i);
                    }
                }
            }
        }

        protected string GetStateMachineName(Object stateMachine)
        {
            return (string) stateMachine.GetType().GetField("name").GetValue(stateMachine);
        }

        protected void SetStateMachineName(Object stateMachine, string name)
        {
            stateMachine.GetType().GetField("name").SetValue(stateMachine, name);
        }

        protected void AddStateMachine(string stateMachineName)
        {
            target.GetType().GetMethod("AddStateMachine").Invoke(target, new object[] {stateMachineName});
        }

        protected void RemoveStateMachine(Object removeStateMachine)
        {
            target.GetType().GetMethod("RemoveStateMachine").Invoke(target, new[] {removeStateMachine});
        }

        protected void MoveStateMachine(Object stateMachine, int index)
        {
            target.GetType().GetMethod("MoveStateMachine").Invoke(target, new[] {stateMachine, index});
        }

        protected Object GetStateMeshine(int index)
        {
            return target.GetType().GetMethod("GetStateMeshine").Invoke(target, new object[] {index});
        }

        protected void SetSelectedStateMachine(int index)
        {
            target.GetType().GetField("selectedStateMachine").SetValue(target, index);
        }
    }
#if !UNITY_3_5
}
#endif