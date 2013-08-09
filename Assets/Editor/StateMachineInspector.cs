using System;
using System.Linq;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StateMachine<State, Transition>))]
public class StateMachineInspector : Editor
{
    protected object stateMachine;

    public virtual void OnStateHeaderGUI<M, S, T>(S state) where S : State
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

    public virtual void OnStateGUI<M, S, T>(M stateMachine, S state)
        where M : StateMachine<S, T>
        where S : State
        where T : Transition
    {


    }

    public virtual void OnTransitionGUI<M, S, T>(M stateMachine, T transition)
        where M : StateMachine<S, T>
        where S : State
        where T : Transition
    {
        S fromState = stateMachine.UniqueIDToState(transition.fromStateUniqueID);
        S toState = stateMachine.UniqueIDToState(transition.toStateNameUniqueID);
        string[] displayNames = stateMachine.parameters.Select<StateMachineParameter, string>(paramater => paramater.name).ToArray();

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label(fromState.stateName + " -> " + toState.stateName);
        if (displayNames.Length != 0)
        {
            EditorGUILayout.BeginHorizontal();
            transition.selectedParameter = EditorGUILayout.Popup(transition.selectedParameter, displayNames);
            transition.parameterKey = displayNames[transition.selectedParameter];
            transition.necessary = (Necessary)EditorGUILayout.EnumPopup(transition.necessary);
            transition.necessaryValue = EditorGUILayout.FloatField(transition.necessaryValue);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    public void SetStateMachine<M, S, T>(M stateMachine)
        where M : StateMachine<S, T>
        where S : State
        where T : Transition
    {
        this.stateMachine = stateMachine;
    }
}
