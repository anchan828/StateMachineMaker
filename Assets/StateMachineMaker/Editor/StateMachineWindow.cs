using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if !UNITY_3_5
namespace StateMachineMaker
{
#endif
    /// <summary>
    ///     FIXME 継承が気持ち悪いのでどうにかする...
    /// </summary>
    public class StateMachineWindow<M, S, T> : EditorWindow
        where M : StateMachine<S, T>
        where S : State
        where T : Transition
    {
        private const int ToolbarHeight = 24;
        private static StateMachineController<M, S, T> controller;
        private Dictionary<S, StateNode> nodes = new Dictionary<S, StateNode>();
        private S[] copyStates, cutStates;
        private S[] forcusedStates = new S[0];
        private long lastCommandTime;

        private bool preDragging;
        private Vector2 scrollPos = Vector2.zero;
        private bool showNewParameterPopup;
        private S startMakeTransition;


        public StateMachineWindow()
        {
            GetController();
            if (controller == null || controller.stateMahineCount == 0) return;
        }

        private M stateMachine
        {
            get { return controller != null ? controller.currentStateMachine : null; }
        }

        /// <summary>
        ///     Button以外でクリックされたときにtrueを返す
        /// </summary>
        private bool isClicked
        {
            get { return (Event.current.button == 0) && (Event.current.type == EventType.MouseDown); }
        }


        private void OnSelectionChange()
        {
            SetUpControllerAndStateMachine(Selection.activeInstanceID);

            Save();
            Repaint();
        }

        private static void SetUpControllerAndStateMachine(int instanceID)
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
                EditorPrefs.SetInt("StateMachineMackerLastController", controller.GetInstanceID());
            }
        }

        public virtual void OnToolbarGUI(M stateMachine)
        {
            EditorGUILayout.BeginHorizontal();
            if (stateMachine != null)
                GUILayout.Label(stateMachine.name);

            EditorGUILayout.EndHorizontal();
        }

        private void OnStateMachineParameterGUI()
        {
            var height = stateMachine.parameters.Count * 22 + 50;
            Rect parametorWindow = new Rect(scrollPos.x, scrollPos.y + position.height - height, 300, height);
            GUI.Window(int.MaxValue, parametorWindow, windowID =>
            {
                foreach (var stateMachineParameter in stateMachine.parameters)
                {

                    EditorGUILayout.BeginHorizontal(GUILayout.Width(parametorWindow.width * 0.95f));
                    stateMachineParameter.name = GUILayout.TextField(stateMachineParameter.name, GUILayout.Width(100));
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
                            var val2 = (Vector2)val;
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
                            var val3 = (Vector3)_val3;
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
                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        stateMachine.DeleteParameter(stateMachineParameter);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                showNewParameterPopup = GUI.Toggle(new Rect(parametorWindow.width - 20, 0, 20, 16),
                    showNewParameterPopup,
                    GUIContent.none);
                if (showNewParameterPopup)
                {
                    DisPlayParameterPopupMenu();
                    showNewParameterPopup = false;
                }
            }, "Parameter");
        }

        protected virtual void OnEnable()
        {
            GetController();
        }

        private static void GetController()
        {
            int controllerInstanceID = EditorPrefs.GetInt("StateMachineMackerLastController", 0);
            SetUpControllerAndStateMachine(controllerInstanceID == 0 ? Selection.activeInstanceID : controllerInstanceID);
        }

        protected virtual void OnDisable()
        {
        }

        private void DrawGrid()
        {
            if (Event.current.type != EventType.Repaint)
                return;
            DrawGridLines(12, EditorGUIUtility.isProSkin ? new Color32(32, 32, 32, 255) : new Color32(60, 60, 60, 255));
            DrawGridLines(120, Color.black);
        }


        private void DrawGridLines(float gridSize, Color gridColor)
        {
            float xMax = position.width * 5, xMin = 0, yMax = position.height * 5, yMin = 0;
            Handles.color = gridColor;
            float x = xMin - xMin % gridSize;
            while (x < xMax)
            {
                Handles.DrawLine(new Vector2(x, yMin), new Vector2(x, yMax));
                x += gridSize;
            }
            float y = yMin - yMin % gridSize;
            while (y < yMax)
            {
                Handles.DrawLine(new Vector3(xMin, y), new Vector3(xMax, y));
                y += gridSize;
            }
        }

        /// <summary>
        ///     OnGUIは触らないほうがいいと思う
        ///     OnGUIの中をいじるのであればOnGraphGUIやOnStateGUIを使用する
        /// </summary>
        private void OnGUI()
        {
            Event e = Event.current;
            if (!string.IsNullOrEmpty(e.commandName))
            {
                CommandEvent();
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
                S[] states = stateMachine.GetAllStates().Where(s => s != null).Where(IsClicked).ToArray();

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
                            Selection.activeObject = nodes[states[0]];
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
                EditorStyles.toolbarButton.Draw(new Rect(0, 0, position.width, ToolbarHeight), false, false, false,
                    false);
            OnToolbarGUI(stateMachine);

            if (stateMachine.GetAllStates().Count != nodes.Count)
            {
                SyncNode(stateMachine);
            }

        }

        private void CommandEvent()
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
                        if (copyStates != null || cutStates != null)
                        {
                            DuplicatedState(copyStates ?? cutStates);
                            copyStates = null;
                            cutStates = null;
                        }
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
        }

        private bool IsClicked(S state)
        {
            Vector2 mousePos = Event.current.mousePosition;
            Rect rect = state.position;
            return rect.x < scrollPos.x + mousePos.x && scrollPos.x + mousePos.x < (rect.x + rect.width)
                   && rect.y < scrollPos.y + mousePos.y && scrollPos.y + mousePos.y < (rect.y + rect.height);
        }

        /// <summary>
        ///     StateやTransitionを描画する
        /// </summary>
        protected virtual void OnGraphGUI()
        {
            BeginWindows();
            OnStateMachineParameterGUI();
            DrawTransitions();
            for (int index = 0; index < stateMachine.stateCount; index++)
            {
                S state = stateMachine.GetState(index);

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
                Rect pos = GUI.Window(index, state.position, id =>
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
                    if (Event.current.button == 1 && Event.current.isMouse)
                    {
                        DisPlayStatePopupMenu(_state);
                    }
                    GUI.DragWindow();
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
                                var _pos = new Rect(forcusedState.position);
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

            if (Event.current.type == EventType.ContextClick)
            {
                DisPlayStateMachinePopupMenu();
            }
        }

        private void SyncNode(M stateMachine)
        {
            var states = stateMachine.GetAllStates();
            nodes = new Dictionary<S, StateNode>();
            foreach (var state in states)
            {
                var stateNode = CreateInstance<StateNode>();
                stateNode.stateID = state.uniqueID;
                stateNode.stateMachine = stateMachine;
                nodes.Add(state, stateNode);
            }
        }


        private void DrawTransitions()
        {
            for (int i = 0; i < stateMachine.transitionCount; i++)
            {
                T transition = stateMachine.GetTransition(i);

                if (transition == null)
                {
                    RegisterUndo("Removed Transition");
                    stateMachine.RemoveTransition(i);
                    break;
                }
                var start = stateMachine.UniqueIDToState(transition.fromStateUniqueID);
                var end = stateMachine.UniqueIDToState(transition.toStateNameUniqueID);
                DrawTransition(start, end, transition.selected);
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

        private float progress = 0;

        private void DrawNodeCurve(Rect start, Rect end, bool selected = false)
        {

            var startPos = new Vector3(start.x + start.width / 2, start.y + start.height * 0.5f, 0);
            var endPos = new Vector3(end.x + end.width / 2, end.y + end.height * 0.5f, 0);
            Handles.color = selected ? Color.red : Color.white;
            Vector3 cross = Vector3.Cross((startPos - endPos).normalized, Vector3.forward);
            Vector3 vector = endPos - startPos;
            Vector3 direction = vector.normalized;
            Vector3 center = vector * 0.5f + startPos;
            center -= cross * 0.5f;
            center += 13 * direction;

            var arraws = new List<Vector3>();
            for (int i = 0; i < 15; i++)
            {
                Vector3[] arraowPos =
                {
                    center + direction*i,
                    (center - direction*i) + cross*i,
                    (center - direction*i) - cross*i,
                    center + direction*i
                };
                arraws.AddRange(arraowPos);
            }

            if (selected)
            {
                Handles.color = Color.red;
            }
            if (1 <= progress)
            {
                progress = 0;
            }
            progress += 0.04f;
            if (moveFromState != null && moveToState != null)
            {
                Rect _start = moveFromState.position, _end = moveToState.position;
                var _startPos = new Vector3(_start.x + _start.width / 2, _start.y + _start.height * 0.5f, 0);
                var _endPos = new Vector3(_end.x + _end.width / 2, _end.y + _end.height * 0.5f, 0);
                var progpos1 = Vector3.Lerp(_startPos, _endPos, progress);
                Vector3 progpos2 = new Vector3(progpos1.x, progpos1.y + 50);
                Vector3 progpos3 = new Vector3(progpos1.x, progpos1.y - 50);
                Handles.DrawPolyLine(progpos1, progpos2, progpos3);
                if (1 <= progress)
                {
                    moveFromState = null;
                    moveToState = null;
                }
            }
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, new[] { startPos, endPos });
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 2f, arraws.ToArray());
            Repaint();
        }

        public void DrawTransitionMove(S from, S to)
        {
            moveFromState = from;
            moveToState = to;
            progress = 0f;
        }

        private S moveFromState, moveToState;
        private void DisPlayStateMachinePopupMenu()
        {
            var contents = new[]
                {
                     new ContentItem{content ="Add State",userData =  Event.current.mousePosition},
                     new ContentItem{content = "Paste",disabled = !(cutStates != null || copyStates != null)}
                };

            ShowGenericMenu(contents);
        }

        private void DisPlayParameterPopupMenu()
        {
            var contents = new[]
            {
                new ContentItem{content = "String"},
                new ContentItem{content = "Bool"},
                new ContentItem{content = "Int"},
                new ContentItem{content = "Float"},
                new ContentItem{content = "Vector2"},
                new ContentItem{content = "Vector3"}
            };
            ShowGenericMenu(contents);
        }
        private void DisPlayStatePopupMenu(S state)
        {
            var contents = new[]
                {
                   new ContentItem{content = "Make Transition",userData = state},
                   new ContentItem{content = "Set Default",userData = state},
                   new ContentItem{content = "",userData = state},
                   new ContentItem{content = "Duplicate State",userData = state},
                   new ContentItem{content = "Delete State",userData = state}
                };
            ShowGenericMenu(contents);

        }

        private void ShowGenericMenu(params ContentItem[] contents)
        {
            var menu = new GenericMenu();
            foreach (var item in contents)
            {
                if (item.disabled)
                {
                    menu.AddDisabledItem(new GUIContent(item.content));
                    continue;
                }

                menu.AddItem(new GUIContent(item.content), false, obj =>
                {
                    var _item = (ContentItem)obj;
                    S state;
                    switch (_item.content)
                    {
                        case "String":
                            stateMachine.CreateParameter("New String", ParameterType.String);
                            break;
                        case "Bool":
                            stateMachine.CreateParameter("New Bool", ParameterType.Bool);
                            break;
                        case "Int":
                            stateMachine.CreateParameter("New Int", ParameterType.Int);
                            break;
                        case "Float":
                            stateMachine.CreateParameter("New Float", ParameterType.Float);
                            break;
                        case "Vector2":
                            stateMachine.CreateParameter("New Vector2", ParameterType.Vector2);
                            break;
                        case "Vector3":
                            stateMachine.CreateParameter("New Vector3", ParameterType.Vector3);
                            break;
                        case "Make Transition":
                            state = _item.userData as S;
                            if (state == null) return;
                            forcusedStates = new[] { state };
                            Selection.objects = new[] { nodes[state] };
                            startMakeTransition = state;
                            break;
                        case "Add State":
                            RegisterUndo("Added State");
                            S s = stateMachine.AddState("New State");
                            stateMachine.SetPosition(s, (Vector2)_item.userData);
                            break;
                        case "Set Default":

                            state = _item.userData as S;
                            if (state == null) return;
                            RegisterUndo("Change Default");
                            stateMachine.SetDefault(state);
                            break;
                        case "Duplicate State":
                            state = _item.userData as S;
                            if (state == null) return;
                            DuplicatedState(state);
                            break;
                        case "Delete State":
                            state = _item.userData as S;
                            if (state == null) return;
                            DeletedState(state);
                            break;
                        case "Paste":
                            if (copyStates != null || cutStates != null)
                            {
                                DuplicatedState(copyStates ?? cutStates);
                                copyStates = null;
                                cutStates = null;
                            }
                            break;
                    }
                    Save();
                }, item);
            }
            menu.ShowAsContext();
            Event.current.Use();
        }
        public static void RegisterUndo(string undoName, StateMachineController<M, S, T> _controller = null)
        {
            Undo.RegisterUndo(_controller ?? controller, undoName);
        }

        public void Save()
        {
            if (controller)
                EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
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
                var clone = (S)state.Clone();
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
                    }
                }
            }
            Save();
            Repaint();
        }

        private void Update()
        {
            if (EditorApplication.isPlaying || startMakeTransition != null)
                Repaint();
        }

        private enum CommandName
        {
            Duplicate,
            Copy,
            Paste,
            Cut,
            Delete,
            SelectAll
        }
    }

    class ContentItem
    {
        public string content;
        public object userData;
        public bool disabled;
    }
#if !UNITY_3_5
}
#endif