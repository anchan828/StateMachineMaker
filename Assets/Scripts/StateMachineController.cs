using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using StateMachineMaker;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace StateMachineMaker
{
    [System.Serializable]
    public class StateMachineController<M, S, TS> : ScriptableObject
        where M : StateMachine<S, TS>
        where S : State
        where TS : Transition
    {
        public StateMachineController()
        {
            // TODO どうにかしてIconを変更したい...
            //        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(GetInstanceID()));
            //        EditorApplication.projectWindowItemOnGUI += (_guid, rect) =>
            //        {
            //            if (guid == _guid)
            //            {
            //                rect.width = 16;
            //                rect.height = 16;
            //                GUI.DrawTexture(rect, (Texture)EditorGUIUtility.Load("Icons/Generated/State Icon.asset"));
            //            }
            //        };
            //
            //        Object defaultAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(GetInstanceID()), typeof(UnityEngine.Object));
            //        Debug.Log(defaultAsset);
            //
            //        MethodInfo methodInfo = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            //        methodInfo.Invoke(null, new object[] { defaultAsset, EditorGUIUtility.whiteTexture });
        }

        public int stateMahineCount
        {
            get
            {
                return stateMachines.Count;
            }
        }

        [HideInInspector]
        public int selectedStateMachine = 0; // Base

        public void MoveStateMachine(M stateMachine, int index)
        {
            if (index <= stateMachines.Count)
            {
                stateMachines.Remove(stateMachine);
                stateMachines.Insert(index, stateMachine);
            }
        }

        public M currentStateMachine
        {
            get
            {
                return GetStateMeshine(selectedStateMachine);
            }
        }

        [SerializeField]
        private List<M> stateMachines = new List<M>();

        public List<M> GetAllStateMachines()
        {
            return stateMachines;
        }

        public M GetStateMeshine(int index)
        {
            return index <= stateMahineCount ? stateMachines[index] : null;
        }

        public void AddStateMachine(string stateMachineName)
        {
            M stateMachine = Activator.CreateInstance<M>();
            stateMachine.AddState("New State");
            int count = stateMachines.Count(sm => sm.name.StartsWith(stateMachineName));
            stateMachine.name = count == 0 ? stateMachineName : stateMachineName + " " + count;
            stateMachines.Add(stateMachine);
#if UNITY_EDITOR
#endif
        }

        public void RemoveStateMachine(M stateMachine)
        {
            stateMachines.Remove(stateMachine);
        }
        protected static T CreateAssets<T>() where T : StateMachineController<M, S, TS>
        {
            T stateMachineController = CreateInstance<T>();
            string directoryPath = "Assets";
#if UNITY_EDITOR
            if (Selection.activeObject)
            {
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

                if (Directory.Exists(assetPath))
                {
                    directoryPath = assetPath;
                }
                else
                {
                    directoryPath = Path.GetDirectoryName(assetPath);
                }
            }
            stateMachineController.name = "New" + stateMachineController.GetType().Name;
            string uniqueAssetPath =
                AssetDatabase.GenerateUniqueAssetPath(directoryPath + "/" + stateMachineController.name + ".asset");

#endif
            stateMachineController.AddStateMachine("NewStateMachine");
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(stateMachineController, uniqueAssetPath);
            AssetDatabase.SaveAssets();
#endif
            return stateMachineController;
        }

        public M SetStateMachine(M stateMachine)
        {
            for (int i = 0; i < stateMachines.Count; i++)
            {
                if (stateMachines[i] == stateMachine)
                {
                    selectedStateMachine = i;
                    break;
                }
            }
            return stateMachines[selectedStateMachine];
        }
    }
}