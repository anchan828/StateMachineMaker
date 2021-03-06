﻿using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if !UNITY_3_5
namespace StateMachineMaker.Editor
{
#endif
    public class StateInspector<M, S, TS> : UnityEditor.Editor
        where M : StateMachine<S, TS>
        where TS : Transition
        where S : State
    {
        private bool isClicked
        {
            get { return (Event.current.button == 0) && (Event.current.type == EventType.MouseDown); }
        }
        private Action DrawOnStateHeader, DrawState, DrawTransitions;
        private bool saveFlag = false;
        public virtual void OnEnable()
        {
            var stateNode = (StateNode)target;
            if (stateNode.stateMachine == null) return;
            var stateMachine = (M)stateNode.stateMachine;
            S state = stateMachine.UniqueIDToState(stateNode.stateID);
            SetUpInspector(stateMachine, state);
        }

        public void SetUpInspector(M stateMachine, S state)
        {
            DrawOnStateHeader = () => OnStateHeaderGUI(state);

            DrawState = () => OnStateGUI(stateMachine, state);

            DrawTransitions = () =>
            {
                EditorGUILayout.LabelField("Transition");
                foreach (TS t in stateMachine.GetTransitionOfToState(state))
                {
                    OnTransitionGUI(stateMachine, t);
                }
            };
        }


        public virtual void OnStateHeaderGUI(S state)
        {
            GUI.Box(new Rect(0, 0, Screen.width, 50), "", "In BigTitle");
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label((Texture)EditorGUIUtility.Load("Icons/Generated/State Icon.asset"), GUILayout.Width(40));

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();
            string stateName = EditorGUILayout.TextField(state.stateName);
            if (EditorGUI.EndChangeCheck())
            {
                state.stateName = stateName;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public virtual void OnStateGUI(M stateMachine, S state)
        {
        }

        public virtual void OnTransitionGUI(M stateMachine, TS transition)
        {
            S fromState = stateMachine.UniqueIDToState(transition.fromStateUniqueID);
            S toState = stateMachine.UniqueIDToState(transition.toStateNameUniqueID);

            string[] displayNames =
                stateMachine.parameters.Select(parameter => parameter.name).ToArray();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(fromState.stateName + " -> " + toState.stateName);
            if (GUILayout.Button("x", EditorStyles.miniButton))
            {
                stateMachine.RemoveTransition(transition);
                return;
            }
            EditorGUILayout.EndHorizontal();
            if (displayNames.Length != 0)
            {
                EditorGUILayout.BeginHorizontal();
                int selected = 0;
                for (int index = 0; index < displayNames.Length; index++)
                {
                    if (transition.parameterKey == displayNames[index])
                    {
                        selected = index;
                        break;
                    }
                }
                //初期化
                if (selected == 0)
                {
                    transition.parameterKey = displayNames[0];
                    transition.parameterType = stateMachine.GetParameterType(transition.parameterKey);
                }
                EditorGUI.BeginChangeCheck();
                int value = EditorGUILayout.Popup(selected, displayNames);

                switch (transition.parameterType)
                {
                    case ParameterType.Bool:
                        transition.necessary =
                            (Necessary)
                                EditorGUILayout.Popup((int)transition.necessary, new[] { "True", "False" });
                        break;
                    case ParameterType.Int:
                    case ParameterType.Float:

                        if ((int)transition.necessary < 2)
                        {
                            transition.necessary = Necessary.Greater;
                        }
                        transition.necessary =
                            (Necessary)
                                EditorGUILayout.Popup((int)transition.necessary - 2,
                                    Enum.GetNames(typeof(Necessary)).Skip(2).ToArray()) + 2;
                        break;
                    default:
                        break;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    transition.parameterKey = displayNames[value];
                    transition.parameterType = stateMachine.GetParameterType(transition.parameterKey);
                    saveFlag = true;
                }
                OnNecessaryValueGUI(transition);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            if (isClicked)
            {
                transition.selected = lastRect.Contains(Event.current.mousePosition);
                WindowRepaint();
            }
            if (transition.selected)
            {
                Handles.color = Color.red;
                var pos = new[]
                {
                    new Vector3(lastRect.x, lastRect.y),
                    new Vector3(lastRect.x+lastRect.width,lastRect.y), 
                    new Vector3(lastRect.x+lastRect.width,lastRect.y+lastRect.height), 
                    new Vector3(lastRect.x,lastRect.y+lastRect.height), 
                    new Vector3(lastRect.x, lastRect.y)
                };
                Handles.DrawPolyLine(pos);
                Repaint();
            }
            
        }

        private void WindowRepaint()
        {
            var windows = Resources.FindObjectsOfTypeAll(typeof(StateMachineWindow<M, S, TS>));
            foreach (StateMachineWindow<M, S, TS> window in windows)
            {
                window.Repaint();
            }
        }
        private void Save()
        {
            var windows = Resources.FindObjectsOfTypeAll(typeof(StateMachineWindow<M, S, TS>));
            foreach (StateMachineWindow<M, S, TS> window in windows)
            {
                window.Save();
            }
        }

        private void OnNecessaryValueGUI(Transition transition)
        {
            EditorGUI.BeginChangeCheck();

            switch (transition.parameterType)
            {
                case ParameterType.String:
                    string valueString = EditorGUILayout.TextField((string)transition.necessaryValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        transition.necessaryValue = valueString;
                        saveFlag = true;
                    }
                    break;
                case ParameterType.Int:
                    object val = transition.necessaryValue ?? 0;
                    GUI.SetNextControlName("textField");
                    int valueInt = EditorGUILayout.IntField((int)val);

                    if (EditorGUI.EndChangeCheck())
                    {
                        transition.necessaryValue = valueInt;
                        saveFlag = true;
                    }
                    break;
                case ParameterType.Float:
                    val = transition.necessaryValue ?? 0f;
                    float valueFloat = EditorGUILayout.FloatField((float)val);
                    if (EditorGUI.EndChangeCheck())
                    {
                        transition.necessaryValue = valueFloat;
                        saveFlag = true;
                    }
                    break;
                case ParameterType.Vector2:
                    EditorGUILayout.BeginHorizontal();
                    val = transition.necessaryValue ?? Vector2.zero;
                    var val2 = (Vector2)val;
                    float x = EditorGUILayout.FloatField(val2.x);
                    float y = EditorGUILayout.FloatField(val2.y);
                    EditorGUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        transition.necessaryValue = new Vector2(x, y);
                        saveFlag = true;
                    }
                    break;
                case ParameterType.Vector3:
                    EditorGUILayout.BeginHorizontal();
                    val = transition.necessaryValue ?? Vector3.zero;
                    var val3 = (Vector3)val;
                    x = EditorGUILayout.FloatField(val3.x);
                    y = EditorGUILayout.FloatField(val3.y);
                    float z = EditorGUILayout.FloatField(val3.z);
                    EditorGUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        transition.necessaryValue = new Vector3(x, y, z);
                        saveFlag = true;
                    }
                    break;
                case ParameterType.Bool:
                default:
                    EditorGUI.EndChangeCheck();
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            if (DrawOnStateHeader != null)
            {
                DrawOnStateHeader();
                DrawState();
                DrawTransitions();
            }
            if (saveFlag && string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
            {
                Save();
                saveFlag = false;
            }
        }
    }
#if !UNITY_3_5
}
#endif