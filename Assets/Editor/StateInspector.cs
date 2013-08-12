using System;
using System.Linq;
using StateMachineMaker;
using UnityEditor;
using UnityEngine;
public class StateInspector<M, S, TS> : Editor
    where M : StateMachine<S, TS>
    where TS : Transition
    where S : State
{

    public virtual void OnEnable()
    {
        StateNode stateNode = (StateNode)target;
        if (stateNode.stateMachine == null) return;
        M stateMachine = (M)stateNode.stateMachine;
        S state = stateMachine.UniqueIDToState(stateNode.stateID);
        SetUpInspector(stateMachine, state);
    }

    private Action DrawOnStateHeader, DrawState, DrawTransitions;
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

        string[] displayNames = stateMachine.parameters.Select<StateMachineParameter, string>(paramater => paramater.name).ToArray();

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
            if (EditorGUI.EndChangeCheck())
            {
                transition.parameterKey = displayNames[value];
                transition.parameterType = stateMachine.GetParameterType(transition.parameterKey);
            }
            switch (transition.parameterType)
            {
                case ParameterType.Bool:
                    transition.necessary =
                        (Necessary)EditorGUILayout.Popup((int)transition.necessary, new string[] { "True", "False" });
                    break;
                case ParameterType.Int:
                case ParameterType.Float:

                    if ((int)transition.necessary < 2)
                    {
                        transition.necessary = Necessary.Greater;
                    }
                    transition.necessary = (Necessary)EditorGUILayout.Popup((int)transition.necessary - 2, Enum.GetNames(typeof(Necessary)).Skip(2).ToArray()) + 2;
                    break;
                case ParameterType.String:
                case ParameterType.Vector2:
                case ParameterType.Vector3:
                default:
                    break;
            }

            OnNecessaryValueGUI(transition);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void OnNecessaryValueGUI(Transition transition)
    {
        EditorGUI.BeginChangeCheck();
        switch (transition.parameterType)
        {
            case ParameterType.String:
                string valueString = EditorGUILayout.TextField(transition.necessaryValueString);
                if (EditorGUI.EndChangeCheck())
                {
                    transition.necessaryValueString = valueString;
                }
                break;
            case ParameterType.Int:

                int valueInt = EditorGUILayout.IntField(transition.necessaryValueInt);

                if (EditorGUI.EndChangeCheck())
                {
                    transition.necessaryValueInt = valueInt;

                }
                break;
            case ParameterType.Float:
                float valueFloat = EditorGUILayout.FloatField(transition.necessaryValueFloat);
                if (EditorGUI.EndChangeCheck())
                {
                    transition.necessaryValueFloat = valueFloat;
                }
                break;
            case ParameterType.Vector2:
                EditorGUILayout.BeginHorizontal();
                float x = EditorGUILayout.FloatField(transition.necessaryValueVector2.x);
                float y = EditorGUILayout.FloatField(transition.necessaryValueVector2.y);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    transition.necessaryValueVector2 = new Vector2(x, y);
                }
                break;
            case ParameterType.Vector3:
                EditorGUILayout.BeginHorizontal();
                x = EditorGUILayout.FloatField(transition.necessaryValueVector3.x);
                y = EditorGUILayout.FloatField(transition.necessaryValueVector3.y);
                float z = EditorGUILayout.FloatField(transition.necessaryValueVector3.z);
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    transition.necessaryValueVector3 = new Vector3(x, y, z);
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
    }
}

