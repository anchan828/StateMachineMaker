using System;
using System.Linq;
using StateMachineMaker;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Styles = StateMachineMaker.Styles;

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
        Duplicate,
        Copy,
        Paste,
        Cut,
        Delete,
        SelectAll,
    }

    private const int ToolbarHeight = 24;
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
            return controller != null ? controller.currentStateMachine : null;
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
        if (!obj && Selection.activeObject)
        {
            assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        }
        if (obj is StateMachineController<M, S, T>)
        {
            controller = (StateMachineController<M, S, T>)obj;
            EditorUserSettings.SetConfigValue("LastController", controller.GetInstanceID().ToString());
        }
    }

    public virtual void OnToolbarGUI(M stateMachine)
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

    protected virtual void OnEnable()
    {
        GetController();
    }

    private static void GetController()
    {
        string controllerValue = EditorUserSettings.GetConfigValue("LastController");
        int controllerInstanceID;

        if (int.TryParse(controllerValue, out controllerInstanceID))
        {
            SetUpControllerAndStateMachine(controllerInstanceID);
        }
        else
        {
            SetUpControllerAndStateMachine(Selection.activeInstanceID);
        }
    }

    protected virtual void OnDisable()
    {
    }

    private bool preDragging;
    private S[] copyStates, cutStates;
    private long lastCommandTime = 0;

    private void DrawGrid()
    {
        if (Event.current.type != EventType.Repaint)
            return;
        Profiler.BeginSample("DrawGrid");
        HandleUtility.handleMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(1);
        this.DrawGridLines(20, Color.black);
        GL.End();
        GL.PopMatrix();
        Profiler.EndSample();
    }

    private float xMin = 0, yMin = 0, xMax = 100, yMax = 100;
    private void DrawGridLines(float gridSize, Color gridColor)
    {
        Handles.color = gridColor;
        UpdateGraphExtents();
        GL.Color(gridColor);
        float x = xMin - xMin % gridSize;
        while ((double)x < (double)xMax)
        {
            DrawLine(new Vector2(x, yMin), new Vector2(x, yMax));
            x += gridSize;
        }
        GL.Color(gridColor);
        float y = yMin - yMin % gridSize;
        while ((double)y < (double)yMax)
        {
            DrawLine(new Vector3(xMin, y, -1), new Vector3(xMax, y, 1));
            y += gridSize;
        }
    }
    private void DrawLine(Vector3 p1, Vector3 p2)
    {
        GL.Vertex(p1);
        GL.Vertex(p2);
    }
    private void UpdateGraphExtents()
    {
        xMax = position.width * 5;
        yMax = position.height * 5;
    }
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

            if (now - lastCommandTime > 5000000)
            {
                e.Use();
                if (e.commandName == CommandName.Duplicate.ToString())
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
            S[] states = stateMachine.GetAllStates().Where(IsClicked).ToArray();
            if (states.Length == 0)
            {
                forcusedStates = new S[0];

            }
            else
            {
                if (Event.current.command || Event.current.shift)
                {
                    var objects = new List<Object>();
                    if (!forcusedStates.Contains(states[0]))
                    {
                        ArrayUtility.Add(ref forcusedStates, states[0]);
                        objects.AddRange(Selection.objects);
                        objects.Add(nodes[states[0]]);

                    }
                    else
                    {
                        ArrayUtility.Remove(ref forcusedStates, states[0]);
                        objects.AddRange(Selection.objects);
                        objects.Remove(nodes[states[0]]);
                    }
                    if (objects.Count != 0)
                        Selection.objects = objects.ToArray();
                }
                else
                {
                    if (forcusedStates.Length <= 1 || !forcusedStates.Contains(states[0]))
                    {
                        forcusedStates = new[] { states[0] };
                        Selection.objects =
                            new[] { states[0] }.Where(nodes.ContainsKey).Select<S, StateNode>(s => nodes[s]).ToArray();
                    }
                }
            }
            Repaint();
        }

        if (!controller)
        {
            GUIUtility.ExitGUI();
            return;
        }



        if (e.type == EventType.Repaint)
        {
            Styles.BackgroudStyle.Draw(new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight),
                false, false, false, false);
            DrawGrid();
        }


        if (stateMachine != null)
        {
            scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos,
               new Rect(0, 0, position.width * 2f, position.height * 2f), false, false);
            OnGraphGUI();
            GUI.EndScrollView();
        }
        if (e.type == EventType.Repaint)
            EditorStyles.toolbarButton.Draw(new Rect(0, 0, position.width, ToolbarHeight), false, false, false, false);
        OnToolbarGUI(stateMachine);
    }

    private bool IsClicked(S state)
    {
        Vector2 mousePos = Event.current.mousePosition;
        Rect rect = state.position;
        return rect.x < scrollPos.x + mousePos.x && scrollPos.x + mousePos.x < (rect.x + rect.width)
               && rect.y < scrollPos.y + mousePos.y && scrollPos.y + mousePos.y < (rect.y + rect.height);
    }

    private Vector2 scrollPos = Vector2.zero;

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

            StateColor stateColor = state.isDefault ? StateColor.Orange : state.color;

            bool on = forcusedStates.Contains(state);
            if (EditorApplication.isPlaying && stateMachine.currentState == state && !state.isDefault)
            {
                stateColor = StateColor.Aqua;
            }

            GUIStyle nodeStyle = Styles.GetStateStyle(stateColor, @on);
            EditorGUI.BeginChangeCheck();
            Vector2 stateSize = GetStateSize(state);
            state.position.width = stateSize.x;
            state.position.height = stateSize.y;
            Rect pos = GUI.Window(index, state.position, (id) =>
            {
                S _state;
                try
                {
                    _state = stateMachine.GetState(id);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
                if (_state == null) return;

                EditorGUI.BeginChangeCheck();
                OnStateGUI(_state);
                if (EditorGUI.EndChangeCheck())
                {
                    Save();
                }
                DisPlayStatePopupMenu(_state);
                GUI.DragWindow();
                //                Debug.Log("click " + (Event.current.type == EventType.ContextClick));

            }, state.stateName, nodeStyle);

            pos.x = Mathf.Clamp(pos.x, -pos.width * 0.5f, float.MaxValue);
            pos.y = Mathf.Clamp(pos.y, ToolbarHeight, float.MaxValue);
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
            if (Event.current.type == EventType.Used)
            {
                if (forcusedStates != null && forcusedStates.Length == 1 && startMakeTransition != forcusedStates[0])
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

    public virtual Vector2 GetStateSize(S state)
    {
        return new Vector2(170, 60);
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


        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, new Vector3[] { startPos, endPos });
        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, arraws.ToArray());
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

        if (Event.current.button == 1 && Event.current.isMouse)
        {
            var options = new GUIContent[]
            {
                new GUIContent("Make Transition"),
                new GUIContent("Set Default"), 
                new GUIContent(""),
                new GUIContent("Duplicate State"),
                new GUIContent("Delete State"),  
            };

            EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y / 2, 150, 100), options, -1,
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
                forcusedStates = new S[] { state };
                Selection.objects = new[] { nodes[state] };
                startMakeTransition = state;
                break;
            case "Add State":
                RegisterUndo("Added State");
                S s = stateMachine.AddState("New State");
                stateMachine.SetPosition(s, (Vector2)userData);
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
        Repaint();
    }

    private void DeletedState(params S[] states)
    {
        RegisterUndo("Deleted State");
        foreach (S state in states)
        {
            stateMachine.RemoveState(state);
        }
        Save();
        Repaint();
    }

    private void DuplicatedState(params S[] states)
    {
        RegisterUndo("Duplicated State");
        foreach (S state in states)
        {
            S clone = (S)state.Clone();
            clone.isDefault = false;
            stateMachine.AddState(clone);

            //Update Transition

            List<T> transitionOfState = stateMachine.GetTransitionOfState(state);

            foreach (T transition in transitionOfState)
            {
                if (transition.fromStateUniqueID == state.uniqueID)
                {
                    stateMachine.AddTransition(clone, stateMachine.UniqueIDToState(transition.toStateNameUniqueID));
                    continue;
                }
                if (transition.toStateNameUniqueID == state.uniqueID)
                {
                    stateMachine.AddTransition(stateMachine.UniqueIDToState(transition.fromStateUniqueID), clone);
                    continue;
                }
            }

        }
        Save();
        Repaint();
    }

    void Update()
    {
        if (EditorApplication.isPlaying || startMakeTransition != null)
            Repaint();
    }
}
