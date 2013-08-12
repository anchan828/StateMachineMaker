using System;
using System.Linq;
using System.Reflection;
using Kyusyukeigo.StateMachine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Graphs;
using UnityEngine;
using System.Collections.Generic;
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
    private S startMakeTransition = null;
    private S forcusedState = null;
    private Dictionary<S, StateNode> nodes = new Dictionary<S, StateNode>();

    public StateMachineWindow()
    {
        GetController();
        if (controller == null || controller.stateMahineCount == 0) return;

        for (int index = 0; index < stateMachine.stateCount; index++)
        {
            S state = stateMachine.GetState(index);
            SyncNode(state, stateMachine);
        }
    }

    private M stateMachine
    {
        get
        {
            return controller ? controller.currentStateMachine : null;
        }
    }

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
        SetUpControllerAndStateMachine(Selection.activeInstanceID);

        Save();
        Repaint();
    }

    void SetUpControllerAndStateMachine(int instanceID)
    {
        string assetPath = AssetDatabase.GetAssetPath(instanceID);
        Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        if (!obj) return;
        if (obj is StateMachineController<M, S, T>)
        {
            controller = (StateMachineController<M, S, T>)obj;
            EditorUserSettings.SetConfigValue("LastController", controller.GetInstanceID().ToString());
        }
    }

    void OnToolbarGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (stateMachine != null)
            GUILayout.Label(stateMachine.name);

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
                EditorGUI.BeginChangeCheck();
                switch (stateMachineParameter.parameterType)
                {
                    case ParameterType.String:
                        string stringValue = GUILayout.TextField(stateMachineParameter.stringValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync StringValue");
                            stateMachineParameter.stringValue = stringValue;
                        }
                        break;
                    case ParameterType.Bool:
                        bool boolValue = GUILayout.Toggle(stateMachineParameter.boolValue, GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync BoolValue");
                            stateMachineParameter.boolValue = boolValue;
                        }
                        break;
                    case ParameterType.Int:
                        int intValue = EditorGUILayout.IntField(stateMachineParameter.intValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync IntValue");
                            stateMachineParameter.intValue = intValue;
                        }
                        break;
                    case ParameterType.Float:
                        float floatValue = EditorGUILayout.FloatField(stateMachineParameter.floatValue);
                        if (EditorGUI.EndChangeCheck())
                        {
                            stateMachineParameter.floatValue = floatValue;
                        }
                        break;
                    case ParameterType.Vector2:
                        float x = stateMachineParameter.vector2Value.x, y = stateMachineParameter.vector2Value.y;
                        x = EditorGUILayout.FloatField(x);
                        y = EditorGUILayout.FloatField(y);

                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync Vector2Value");
                            stateMachineParameter.vector2Value = new Vector2(x, y);
                        }
                        break;
                    case ParameterType.Vector3:
                        x = stateMachineParameter.vector3Value.x;
                        y = stateMachineParameter.vector3Value.y;
                        float z = stateMachineParameter.vector3Value.z;
                        x = EditorGUILayout.FloatField(x);
                        y = EditorGUILayout.FloatField(y);
                        z = EditorGUILayout.FloatField(z);

                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync Vector3Value");
                            stateMachineParameter.vector3Value = new Vector3(x, y, z);
                        }
                        break;
                    default:
                        break;
                }
                EditorGUILayout.EndHorizontal();
            }

            showNewParameterPopup = GUI.Toggle(new Rect(parametorWindow.width - 20, 0, 20, 16), showNewParameterPopup, GUIContent.none);
            if (showNewParameterPopup)
            {
                if (genericMenu.GetItemCount() == 0)
                    DisPlayParameterPopupMenu();
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
        GetController();
        OnInitializeGraph();
    }

    private void GetController()
    {
        string controllerValue = EditorUserSettings.GetConfigValue("LastController");
        int controllerInstanceID;

        if (int.TryParse(controllerValue, out controllerInstanceID))
        {
            SetUpControllerAndStateMachine(controllerInstanceID);
        }
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
        DrawTransitions();
        for (int index = 0; index < stateMachine.stateCount; index++)
        {
            S state = stateMachine.GetState(index);

            SyncNode(state, stateMachine);

            Styles.Color color = state.isDefault ? Styles.Color.Orange : Styles.Color.Gray;
            bool on = forcusedState == state;

            if (stateMachine.currentState == state && !state.isDefault)
            {
                color = Styles.Color.Aqua;
            }
            GUIStyle nodeStyle = Styles.GetNodeStyle("node", color, @on);
            EditorGUI.BeginChangeCheck();
            state.position.height = GetStateHeight(state);
            Rect pos = GUI.Window(index, state.position, (id) =>
            {
                S _state = stateMachine.GetState(id);
                if (isClicked)
                {
                    forcusedState = _state;
                    if (nodes.ContainsKey(forcusedState))
                        Selection.activeObject = nodes[forcusedState];
                    Repaint();
                }

                EditorGUI.BeginChangeCheck();
                OnStateGUI(_state);
                if (EditorGUI.EndChangeCheck())
                {
                    Save();
                }

                DisPlayStatePopupMenu(_state);
                GUI.DragWindow(new Rect(0, 0, state.position.width, 20));
            }, state.stateName, nodeStyle);

            if (state.position != pos)
            {
                state.position = pos;
            }
        }
        EndWindows();

        if (startMakeTransition != null)
        {
            DrawMakeTransition();

            if (Event.current.type == EventType.MouseDown)
            {
                if (forcusedState != null && startMakeTransition != forcusedState)
                {
                    RegisterUndo("Added Transion");
                    stateMachine.AddTransition(startMakeTransition, forcusedState);
                }
                startMakeTransition = null;
                Repaint();
            }
        }


        DisPlayStateMachinePopupMenu();
    }

    private void SyncNode(S state, M stateMachine)
    {
        if (!nodes.ContainsKey(state))
        {
            StateNode stateNode = CreateInstance<StateNode>();
            stateNode.stateID = state.uniqueID;
            stateNode.stateMachine = stateMachine;
            nodes.Add(state, stateNode);
        }
    }

    private void Save()
    {
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
    }


    void DrawTransitions()
    {
        for (int i = 0; i < stateMachine.transitionCount; i++)
        {
            T blendShapeTransition = stateMachine.GetTransition(i);

            if (blendShapeTransition == null)
            {
                RegisterUndo("Removed Transition");
                stateMachine.RemoveTransition(i);
                break;
            }
            DrawTransition(stateMachine.UniqueIDToState(blendShapeTransition.fromStateUniqueID), stateMachine.UniqueIDToState(blendShapeTransition.toStateNameUniqueID), blendShapeTransition.selected);
        }
    }
    public virtual void OnStateGUI(S state)
    {
        // StateのGUI (Window)

    }

    public virtual float GetStateHeight(S state)
    {
        return 60;
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
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height * 0.5f, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height * 0.5f, 0);
        Handles.color = selected ? Color.red : Color.white;
        Vector3 cross = Vector3.Cross((startPos - endPos).normalized, Vector3.forward);
        startPos += cross * 5;
        Vector3 vector = endPos - startPos;
        Vector3 direction = vector.normalized;
        Vector3 center = vector * 0.5f + startPos;
        center -= cross * 0.5f;
        center += 13 * direction;

        List<Vector3> arraws = new List<Vector3>();
        for (int i = 0; i < 15; i++)
        {
            Vector3[] arraowPos = new Vector3[]
        {
            center + direction* i,
            (center - direction* i) + cross* i,
            (center - direction* i) - cross* i,
            center + direction* i
        };
            arraws.AddRange(arraowPos);
        }

        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, arraws.ToArray());
        Handles.DrawAAPolyLine((Texture2D)UnityEditor.Graphs.Styles.connectionTexture.image, 5f, new Vector3[] { startPos, endPos });
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
                    RegisterUndo("New Parameter");
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
                    Save();
                }, guiContent.text);
            }
        }
    }

    private void RegisterUndo(string undoName)
    {
        RegisterUndo(undoName);
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
                RegisterUndo("Added State");
                stateMachine.AddState("New State");
                break;
            case "Set Default":

                state = userData as S;
                if (state == null) return;
                RegisterUndo("Change Default");
                stateMachine.SetDefault(state);
                break;
            case "Duplicate State":
                state = userData as S;
                if (state == null) return;
                RegisterUndo("Duplicated State");
                S clone = (S)state.Clone();
                clone.isDefault = false;
                stateMachine.AddState(clone);
                break;
            case "Delete State":
                state = userData as S;
                if (state == null) return;
                RegisterUndo("Deleted State");
                stateMachine.RemoveState(state);
                break;

            default: break;
        }
        Save();
    }

    void Update()
    {
        if (EditorApplication.isPlaying || startMakeTransition != null)
            Repaint();
    }
}
