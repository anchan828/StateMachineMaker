using System;
using System.Linq;
using System.Reflection;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// FIXME 継承が気持ち悪いのでどうにかする...
/// </summary>
public class StateMachineWindow<M, S, T> : EditorWindow
    where M : StateMachine<S, T>
    where S : State
    where T : Transition
{
    protected Graph stateMachineGraph;
    protected GraphGUI stateMachineGraphGUI;
    protected static EditorWindow window;
    private const int ToolbarHeight = 17;
    private StateMachineInspectorWindow stateMachineInspectorWindow;
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

    private StateMachineController<M, S, T> controller;
    void OnSelectionChange()
    {

        string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        Object[] objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

        foreach (Object o in objects)
        {
            if (o is StateMachineController<M, S, T>)
            {
                controller = (StateMachineController<M, S, T>)o;
                break;
            }
        }

        if (controller)
        {
            stateMachine = controller.GetStateMeshine(0);
        }
    }

    private int selectedStatemachine = 0;
    void OnToolbarGUI()
    {

        System.Collections.Generic.List<string> stateMachines = new System.Collections.Generic.List<string>();
        for (int i = 0; i < controller.stateMahineCount; i++)
        {
            stateMachines.Add(controller.GetStateMeshine(i).name);
        }

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        int selected = EditorGUILayout.Popup(selectedStatemachine, stateMachines.ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            selectedStatemachine = selected;
            stateMachine = controller.GetStateMeshine(selected);
        }

        if (GUILayout.Button("Add StateMachine"))
        {
            controller.AddStateMachine("NewStateMachine");
        }

        EditorGUILayout.EndHorizontal();
    }
    Rect parametorWindow = new Rect(0, 0, 0, 0);
    private bool showNewParameterPopup = false;
    GenericMenu genericMenu = new GenericMenu();
    void OnStateMachineParameter()
    {
        if (parametorWindow == new Rect(0, 0, 0, 0))
        {
            parametorWindow = new Rect(0, Screen.height - 200, 300, 200);
        }
        parametorWindow = GUI.Window(int.MaxValue, parametorWindow, (windowID) =>
        {
            foreach (StateMachineParameter stateMachineParameter in stateMachine.parameters)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(parametorWindow.width * 0.9f));
                stateMachineParameter.name = GUILayout.TextField(stateMachineParameter.name);
                switch (stateMachineParameter.parameterType)
                {
                    case StateMachineParameterType.String:
                        stateMachineParameter.stringValue = GUILayout.TextField(stateMachineParameter.stringValue);
                        break;
                    case StateMachineParameterType.Bool:
                        stateMachineParameter.boolValue = GUILayout.Toggle(stateMachineParameter.boolValue, GUIContent.none);
                        break;
                    case StateMachineParameterType.Int:
                        stateMachineParameter.intValue = EditorGUILayout.IntField(stateMachineParameter.intValue);
                        break;
                    case StateMachineParameterType.Float:
                        stateMachineParameter.floatValue = EditorGUILayout.FloatField(stateMachineParameter.floatValue);
                        break;
                    case StateMachineParameterType.Vector2:
                        float x = stateMachineParameter.vector2Value.x, y = stateMachineParameter.vector2Value.y;
                        x = EditorGUILayout.FloatField(x);
                        y = EditorGUILayout.FloatField(y);
                        stateMachineParameter.vector2Value = new Vector2(x, y);
                        break;
                    case StateMachineParameterType.Vector3:
                        x = stateMachineParameter.vector3Value.x;
                        y = stateMachineParameter.vector3Value.y;
                        float z = stateMachineParameter.vector3Value.z;
                        x = EditorGUILayout.FloatField(x);
                        y = EditorGUILayout.FloatField(y);
                        z = EditorGUILayout.FloatField(z);
                        stateMachineParameter.vector3Value = new Vector3(x, y, z);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                EditorGUILayout.EndHorizontal();

            }
            showNewParameterPopup = GUI.Toggle(new Rect(parametorWindow.width - 20, 0, 20, 16), showNewParameterPopup, GUIContent.none);
            if (showNewParameterPopup)
            {
                if (genericMenu.GetItemCount() == 0)
                    DisPlayParameterPopupMenu();
                Debug.Log(genericMenu.GetItemCount());
                genericMenu.ShowAsContext();
                showNewParameterPopup = false;
            }
            GUI.DragWindow(new Rect(0, 0, parametorWindow.width, 20));
        }, "Parameter");
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
        if (!controller)
        {
            EditorGUIUtility.ExitGUI();
            return;
        }
        OnToolbarGUI();

        stateMachineGraphGUI.BeginGraphGUI(window, new Rect(0, ToolbarHeight, window.position.width, window.position.height));

        if (stateMachine != null)
        {
            OnGraphGUI();
        }

        stateMachineGraphGUI.EndGraphGUI();

    }
    /// <summary>
    /// StateやTransitionを描画する
    /// </summary>
    protected virtual void OnGraphGUI()
    {
        BeginWindows();
        OnStateMachineParameter();
        for (int index = 0; index < stateMachine.stateCount; index++)
        {
            S state = stateMachine.GetState(index);
            Styles.Color color = state.isDefault ? Styles.Color.Orange : Styles.Color.Gray;
            bool on = forcusedState == state;

            GUIStyle nodeStyle = Styles.GetNodeStyle("node", color, @on);

            state.position = GUI.Window(index, state.position, (id) =>
            {
                S _state = stateMachine.GetState(id);
                if (isClicked)
                {
                    forcusedState = _state;
                    Type inspectorWindow = Types.GetType("UnityEditor.InspectorWindow", "UnityEditor.dll");
                    if (stateMachineInspectorWindow == null)
                        stateMachineInspectorWindow = GetWindow<StateMachineInspectorWindow>(inspectorWindow);
                    Debug.Log(stateMachineInspectorWindow);
                    stateMachineInspectorWindow.SetStateMachine<M, S, T>(stateMachine, _state);
                    stateMachineInspectorWindow.Repaint();
                }

                OnStateGUI(_state);
                DisPlayStatePopupMenu(_state);
                GUI.DragWindow(new Rect(0, 0, state.position.width, 20));
            }, state.stateName, nodeStyle);
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
                    stateMachineInspectorWindow.SetStateMachine<M, S, T>(stateMachine, forcusedState);
                    stateMachineInspectorWindow.Repaint();
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
            DrawTransition(stateMachine.GetState(blendShapeTransition.fromStateUniqID), stateMachine.GetState(blendShapeTransition.toStateNameUniqID), blendShapeTransition.selected);
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

    private void DrawTransition(S start, S end, bool selected)
    {
        DrawNodeCurve(start.position, end.position, selected);
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

    void DrawNodeCurve(Rect start, Rect end, bool selected = false)
    {
        Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height * 0.5f, 0);
        Vector3 endPos = new Vector3(end.x, end.y + end.height * 0.5f, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Handles.color = selected ? Color.red : Color.white;
        Handles.DrawAAPolyLine(3, endPos + new Vector3(-1, 1, 0) * 10, endPos, endPos + new Vector3(-1, -1, 0) * 10);
        Handles.DrawBezier(startPos, endPos, startTan, endTan, selected ? Color.red : Color.white, null, 5);
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

    void DisPlayParameterPopupMenu()
    {
        var options = new GUIContent[]
            {
                new GUIContent("String"), 
                 new GUIContent("Bool"), 
                  new GUIContent("Int"), 
                   new GUIContent("Float"), 
                    new GUIContent("Vector2"), 
                     new GUIContent("Vector3"),
            };

        if (genericMenu.GetItemCount() == 0)
        {
            foreach (GUIContent guiContent in options)
            {
                genericMenu.AddItem(guiContent, false, (obj) =>
                {
                    switch ((string)obj)
                    {
                        case "String":
                            stateMachine.SetString("New String", "");
                            break;
                        case "Bool":
                            stateMachine.SetBool("New Bool", false);
                            break;
                        case "Int":
                            stateMachine.SetInt("New Int", 0);
                            break;
                        case "Float":
                            stateMachine.SetFloat("New Float", 0.0f);
                            break;
                        case "Vector2":
                            stateMachine.SetVector2("New Vector2", Vector2.zero);
                            break;
                        case "Vector3":
                            stateMachine.SetVector3("New Vector3", Vector3.zero);
                            break;
                    }
                }, guiContent.text);
            }
        }
    }

    void DisPlayStatePopupMenu(S state)
    {

        if (Event.current.type == EventType.ContextClick)
        {
            var options = new GUIContent[]
            {
                new GUIContent("Make Transition"),
                new GUIContent("Set Default"), 
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
                stateMachine.AddState("New State");
                controller.Sync(stateMachine);
                break;
            case "Set Default":
                state = userData as S;
                if (state == null) return;
                stateMachine.SetDefault(state);
                break;
            case "Duplicate State":
                state = userData as S;
                if (state == null) return;
                S clone = (S)state.Clone();
                clone.isDefault = false;
                stateMachine.AddState(clone);
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
