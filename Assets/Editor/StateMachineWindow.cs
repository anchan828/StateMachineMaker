using System;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

public class StateMachineWindow<M, S, T> : EditorWindow
    where M : StateMachine<S, T>
    where S : State
    where T : Transition
{
    protected Graph stateMachineGraph;
    protected GraphGUI stateMachineGraphGUI;
    protected static EditorWindow window;
    private const int ToolbarHeight = 17;

    private M stateMachine = null;
    private S startMakeTransition = null;
    private S forcusedState = null;

    private bool initialized
    {
        get
        {
            return stateMachineGraph != null && stateMachineGraphGUI != null;
        }
    }

    /// <summary>
    /// Button以外でクリックされたときにtrueを返す
    /// </summary>
    private bool isClicked
    {
        get
        {
            return (Event.current.button == 0) && (Event.current.type == EventType.MouseDown);
        }
    }

    /// <summary>
    /// 背景部分の初期化
    /// </summary>
    void OnInitializeGraph()
    {
        if (stateMachineGraph == null)
        {
            stateMachineGraph = CreateInstance<Graph>();
            stateMachineGraph.hideFlags = HideFlags.HideAndDontSave;
        }
        if (stateMachineGraphGUI == null)
        {
            stateMachineGraphGUI = (GetEditor(stateMachineGraph));
        }
    }

    protected virtual void OnEnable()
    {
        stateMachine = Activator.CreateInstance<M>();
        window = GetCurrentWindow();
        OnInitializeGraph();
    }

    protected virtual void OnDisable()
    {
        window = null;
    }

    /// <summary>
    /// OnGUIは触らないほうがいいと思う
    /// OnGUIの中をいじるのであればOnGraphGUIやOnStateGUIを使用する
    /// </summary>
    void OnGUI()
    {
        if (!initialized)
            OnInitializeGraph();

        stateMachineGraphGUI.BeginGraphGUI(window, new Rect(0, ToolbarHeight, window.position.width, window.position.height));
        OnGraphGUI();
        stateMachineGraphGUI.EndGraphGUI();
    }

    /// <summary>
    /// StateやTransitionを描画する
    /// </summary>
    protected virtual void OnGraphGUI()
    {
        BeginWindows();
        for (int index = 0; index < stateMachine.stateCount; index++)
        {
            S state = stateMachine.GetState(index);
            state.position = GUI.Window(index, state.position, (id) =>
            {
                S _state = stateMachine.GetState(id);

                if (isClicked)
                {
                    forcusedState = _state;
                }

                OnStateGUI(_state);
                DisPlayStatePopupMenu(_state);
                GUI.DragWindow(new Rect(0, 0, state.position.width, 20));
            }, state.stateName);
        }
        EndWindows();

        if (startMakeTransition != null)
        {
            DrawMakeTransition();

            if (Event.current.type == EventType.MouseDown)
            {
                if (forcusedState != null && startMakeTransition != forcusedState)
                {
                    stateMachine.AddTransition(startMakeTransition, forcusedState);
                }
                startMakeTransition = null;
            }
        }

        DrawTransitions();

        DisPlayStateMachinePopupMenu();
    }

    void DrawTransitions()
    {
        for (int i = 0; i < stateMachine.transitionCount; i++)
        {
            T blendShapeTransition = stateMachine.GetTransition(i);

            if (blendShapeTransition == null)
            {
                stateMachine.RemoveTransition(i);
                break;
            }

            DrawTransition(stateMachine.GetState(blendShapeTransition.fromStateName), stateMachine.GetState(blendShapeTransition.toStateName));
        }
    }
    protected virtual void OnStateGUI(S state)
    {
        // StateのGUI (Window)

    }

    GraphGUI GetEditor(Graph graph)
    {
        var graphGUI = CreateInstance("GraphGUI") as GraphGUI;
        if (graphGUI != null)
        {
            graphGUI.graph = graph;
            graphGUI.hideFlags = HideFlags.HideAndDontSave;

        }
        return graphGUI;
    }

    protected EditorWindow GetCurrentWindow()
    {
        return GetWindow<StateMachineWindow<M, S, T>>();
    }

    private void DrawTransition(S start, S end)
    {
        DrawNodeCurve(start.position, end.position);
    }

    private void DrawMakeTransition()
    {
        if (startMakeTransition.stateName == string.Empty)
        {
            startMakeTransition = null;
            return;
        }
        Vector2 pos = Event.current.mousePosition;
        DrawNodeCurve(startMakeTransition.position, new Rect(pos.x, pos.y, 0, 0));
    }

    void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height * 0.5f, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height * 0.5f, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Handles.DrawAAPolyLine(3, endPos + new Vector3(-1, 1, 0) * 10, endPos, endPos + new Vector3(-1, -1, 0) * 10);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 5);
    }

    void DisPlayStateMachinePopupMenu()
    {
        if (Event.current.type == EventType.ContextClick)
        {
            var options = new GUIContent[]
            {
                new GUIContent("Add State"), 
            };

            EditorUtility.DisplayCustomMenu(new Rect(0, 0, 100, 100), options, -1,
               StateContextMenu, Event.current.mousePosition);
            Event.current.Use();
        }
    }

    void DisPlayStatePopupMenu(S state)
    {

        if (Event.current.type == EventType.ContextClick)
        {
            var options = new GUIContent[]
            {
                new GUIContent("Make Transition"),
                new GUIContent(""),
                new GUIContent("Duplicate State"),
                new GUIContent("Delete State"),  
            };

            EditorUtility.DisplayCustomMenu(new Rect(0, 0, 100, 100), options, -1,
               StateContextMenu, state);
            Event.current.Use();
        }
    }

    /// <summary>
    /// すべてのContextMenuの動作はここで定義する
    /// FEATURE 後々は分けたほうがいいかも...
    /// </summary>
    /// <param name="userData"></param>
    /// <param name="options"></param>
    /// <param name="selected"></param>
    private void StateContextMenu(object userData, string[] options, int selected)
    {
        S state = null;
        switch (options[selected])
        {
            case "Make Transition":
                state = userData as S;
                if (state == null) return;
                startMakeTransition = state;
                break;
            case "Add State":
                stateMachine.AddState("Empty");
                break;
            case "Duplicate State":
                state = userData as S;
                if (state == null) return;
                stateMachine.AddState((S)state.Clone());
                break;
            case "Delete State":
                state = userData as S;
                if (state == null) return;
                stateMachine.RemoveState(state);
                break;
            default: break;
        }

    }

    void Update()
    {
        Repaint();
    }
}
