using System;
using System.Linq;
using System.Reflection;
using StateMachineMaker;
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

    private enum CommandName
    {
        Depulicate,
        Copy,
        Paste,
        Cut,
        Delete,
        SelectAll,
    }

    protected Graph stateMachineGraph;
    protected GraphGUI stateMachineGraphGUI;
    protected static EditorWindow window;
    private const int ToolbarHeight = 17;
    private S startMakeTransition = null;
    private S[] forcusedStates = new S[0];
    private Dictionary<S, StateNode> nodes = new Dictionary<S, StateNode>();
    private static StateMachineController<M, S, T> controller;

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



    void OnSelectionChange()
    {
        SetUpControllerAndStateMachine(Selection.activeInstanceID);

        Save();
        Repaint();
    }

    static void SetUpControllerAndStateMachine(int instanceID)
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
                        object val = stateMachineParameter.value ?? "";
                        string stringValue = GUILayout.TextField((string)val);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync StringValue");
                            stateMachineParameter.value = stringValue;
                        }
                        break;
                    case ParameterType.Bool:
                        val = stateMachineParameter.value ?? false;
                        bool boolValue = GUILayout.Toggle((bool)val, GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync BoolValue");
                            stateMachineParameter.value = boolValue;
                        }
                        break;
                    case ParameterType.Int:
                        val = stateMachineParameter.value ?? 0;
                        int intValue = EditorGUILayout.IntField((int)val);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync IntValue");
                            stateMachineParameter.value = intValue;
                        }
                        break;
                    case ParameterType.Float:
                        val = stateMachineParameter.value ?? 0f;
                        float floatValue = EditorGUILayout.FloatField((float)val);
                        if (EditorGUI.EndChangeCheck())
                        {
                            stateMachineParameter.value = floatValue;
                        }
                        break;
                    case ParameterType.Vector2:
                        val = stateMachineParameter.value ?? Vector2.zero;
                        Vector2 val2 = (Vector2)val;
                        float x = val2.x, y = val2.y;
                        x = EditorGUILayout.FloatField(x);
                        y = EditorGUILayout.FloatField(y);

                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync Vector2Value");
                            stateMachineParameter.value = new Vector2(x, y);
                        }
                        break;
                    case ParameterType.Vector3:
                        object _val3 = stateMachineParameter.value ?? Vector3.zero;
                        Vector3 val3 = (Vector3)_val3;
                        x = val3.x;
                        y = val3.y;
                        float z = val3.z;
                        x = EditorGUILayout.FloatField(x);
                        y = EditorGUILayout.FloatField(y);
                        z = EditorGUILayout.FloatField(z);

                        if (EditorGUI.EndChangeCheck())
                        {
                            RegisterUndo("Sync Vector3Value");
                            stateMachineParameter.value = new Vector3(x, y, z);
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

    private static void GetController()
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

    private bool preDragging;
    private S[] copyStates, cutStates;
    private long lastCommandTime = 0;
    /// <summary>
    /// OnGUIは触らないほうがいいと思う
    /// OnGUIの中をいじるのであればOnGraphGUIやOnStateGUIを使用する
    /// </summary>
    void OnGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.ValidateCommand)
        {
            long now = DateTime.Now.Ticks;
            e.Use();
            if (now - lastCommandTime > 5000000)
            {
                if (e.commandName == CommandName.Depulicate.ToString())
                {
                    DuplicatedState(forcusedStates);
                }
                else if (e.commandName == CommandName.Copy.ToString())
                {
                    copyStates = forcusedStates.ToArray();
                }
                else if (e.commandName == CommandName.Paste.ToString())
                {
                    if (copyStates != null)
                        DuplicatedState(copyStates);
                }
                else if (e.commandName == CommandName.Cut.ToString())
                {
                    cutStates = (S[])forcusedStates.ToArray().Clone();
                    DeletedState(forcusedStates);
                }
                else if (e.commandName == CommandName.Delete.ToString())
                {
                    DeletedState(forcusedStates);
                }
                else if (e.commandName == CommandName.SelectAll.ToString())
                {
                    forcusedStates = stateMachine.GetAllStates().ToArray();
                }
            }
            lastCommandTime = now;
        }
        if (Event.current.type == EventType.MouseDrag)
        {
            if (preDragging)
            {
                RegisterUndo("Moved State Position", controller);
                preDragging = false;
            }
        }

        if (isClicked)
        {
            S[] array = stateMachine.GetAllStates().Where(state => state.position.Contains(Event.current.mousePosition)).ToArray();
        }

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
            bool on = forcusedStates.Contains(state);

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
                    if (1 >= forcusedStates.Length)
                    {
                        forcusedStates = new S[] {_state};
                        if (nodes.ContainsKey(_state))
                            Selection.activeObject = nodes[_state];
                        Repaint();
                    }
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
                if (1 < forcusedStates.Length)
                {
                    foreach (S forcusedState in forcusedStates)
                    {
                        if (state != forcusedState)
                        {
                            Rect _pos = new Rect(forcusedState.position);
                            _pos.x -= state.position.x - pos.x;
                            _pos.y -= state.position.y - pos.y;
                            forcusedState.position = _pos;
                        }
                    }
                }
                state.position = pos;
                Repaint();
            }
        }
        EndWindows();

        if (startMakeTransition != null)
        {
            DrawMakeTransition();

            if (Event.current.type == EventType.MouseDown)
            {
                if (forcusedStates != null && forcusedStates.Length == 1 && forcusedStates.Contains(startMakeTransition))
                {
                    RegisterUndo("Added Transion");
                    stateMachine.AddTransition(startMakeTransition, forcusedStates[0]);
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

    public static void RegisterUndo(string undoName, StateMachineController<M, S, T> _controller = null)
    {
        if (_controller)
        {
            Undo.RegisterUndo(_controller, undoName);
        }
        else
        {
            Undo.RegisterUndo(controller, undoName);
        }
    }

    public static void Save()
    {
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
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
                DuplicatedState(state);
                break;
            case "Delete State":
                state = userData as S;
                if (state == null) return;
                DeletedState(state);
                break;

            default: break;
        }
        Save();
    }

    private void DeletedState(params S[] states)
    {
        RegisterUndo("Deleted State");
        foreach (S state in states)
        {
            stateMachine.RemoveState(state);
        }

    }

    private void DuplicatedState(params S[] states)
    {
        RegisterUndo("Duplicated State");
        foreach (S state in states)
        {
            S clone = (S)state.Clone();
            clone.isDefault = false;
            stateMachine.AddState(clone);
        }
    }

    void Update()
    {
        if (EditorApplication.isPlaying || startMakeTransition != null)
            Repaint();
    }
}
