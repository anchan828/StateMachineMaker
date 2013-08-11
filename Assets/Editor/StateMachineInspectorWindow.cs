using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class StateMachineInspectorWindow : EditorWindow
{
    private StateMachineInspector editor;
    private Action DrawOnStateHeader, DrawState, DrawTransitions;
    public void SetStateMachine<M, S, T>(EditorWindow window, M stateMachine, S state)
        where M : StateMachine<S, T>
        where T : Transition
        where S : State
    {
        Type type = Types.GetType("UnityEditor.CustomEditorAttributes", "UnityEditor.dll");
        Type editorType = type.GetMethod("FindCustomEditorType", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { stateMachine, false }) as Type;
        if (editorType != null)
        {
            editor = (StateMachineInspector)(CreateInstance(editorType));

            editor.SetStateMachine<M, S, T>(window, stateMachine);
            DrawOnStateHeader = () => editor.OnStateHeaderGUI<M, S, T>(state);

            DrawState = () => editor.OnStateGUI<M, S, T>(stateMachine, state);

            DrawTransitions = () =>
            {
                EditorGUILayout.LabelField("Transition");
                foreach (T transition in stateMachine.GetTransitionOfToState(state))
                {
                    editor.OnTransitionGUI<M, S, T>(stateMachine, transition);
                }
            };
        }
    }

    private void OnGUI()
    {
        if (editor && DrawOnStateHeader != null)
        {
            DrawOnStateHeader();
            DrawState();
            DrawTransitions();
        }
    }
}