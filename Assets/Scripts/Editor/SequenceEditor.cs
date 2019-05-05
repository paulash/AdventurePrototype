using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

public class SequenceEditor : EditorWindow
{
    Sequence targetSequence = null;

    [System.NonSerialized]
    List<System.Type> actionClasses = null;

    [System.NonSerialized]
    string[] actionClassNames = null;

    Stack<ActionState> removedStates = new Stack<ActionState>();
    Stack<int> insertState = new Stack<int>();
    Stack<ActionState> cloneState = new Stack<ActionState>();

    Dictionary<string, ActionVariable[]> variablesByType = new Dictionary<string, ActionVariable[]>();

    [MenuItem("Window/Sequence Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        SequenceEditor window = (SequenceEditor)EditorWindow.GetWindow(typeof(SequenceEditor));
        window.Show();
    }

    void OnSelectionChange()
    {
        if (Selection.activeObject is Sequence)
        {
            targetSequence = Selection.activeObject as Sequence;
            for (int i = 0; i < targetSequence.actionStates.Count; i++)
                targetSequence.actionStates[i].InitializeVariables();
        }
        else
            targetSequence = null;

        removedStates.Clear();
        Repaint();
    }

    void CollectActionClasses()
    {
        UnityEditor.Compilation.Assembly[] assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player);

        for (int a = 0; a < assemblies.Length; a++)
        {
            if (!assemblies[a].outputPath.EndsWith("Assembly-CSharp.dll"))
                continue;

            actionClasses = new List<System.Type>();
            System.Type[] allTypes = Assembly.LoadFile(assemblies[a].outputPath).GetTypes();

            actionClasses = new List<System.Type>(allTypes.Where(t => t.IsSubclassOf(typeof(Action))));
            variablesByType.Clear();

            actionClassNames = new string[actionClasses.Count];
            for (int i = 0; i < actionClasses.Count; i++)
            {
                actionClassNames[i] = actionClasses[i].Name;
                variablesByType.Add(actionClasses[i].Name, actionClasses[i].GetCustomAttributes<ActionVariable>().ToArray());
            }
        }
    }

    int GetActionIndex(string name)
    {
        for (int i= 0; i < actionClassNames.Length; i++)
        {
            if (name == actionClassNames[i])
                return i;
        }

        return 0;
    }

    string GetActionName(int index)
    {
        if (index < 0 || actionClassNames.Length <= index)
            return "";

        return actionClassNames[index];
    }

    private void OnGUI()
    {
        if (actionClasses == null)
            CollectActionClasses();

        if (targetSequence != null)
            GUILayout.Label(targetSequence.name);
        else
        {
            GUILayout.Label("None");
            return;
        }

        GUILayout.BeginVertical(GUILayout.MaxWidth(500));
        {
            for (int i = 0; i < targetSequence.actionStates.Count; i++)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Remove"))
                            removedStates.Push(targetSequence.actionStates[i]);

                        if (GUILayout.Button("Insert"))
                            insertState.Push(i);

                        if (GUILayout.Button("Clone"))
                            cloneState.Push(targetSequence.actionStates[i]);
                    }
                    GUILayout.EndHorizontal();

                    ActionState state = targetSequence.actionStates[i];
                    state.actionClass = GetActionName(EditorGUILayout.Popup(GetActionIndex(state.actionClass), actionClassNames));
                    state.targetActorName = EditorGUILayout.TextField("Target Actor Name", state.targetActorName);

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        ActionVariable[] variables = null;
                        if (variablesByType.TryGetValue(state.actionClass, out variables))
                        {
                            for (int v=0; v < variables.Length; v++)
                            {
                                switch (variables[v].type)
                                {
                                    case ActionVariableType.Item:
                                        state.Set(variables[v].name, EditorGUILayout.ObjectField(variables[v].name, state.Get<Item>(variables[v].name), typeof(Item)));
                                        break;
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }

            while (removedStates.Count != 0)
                targetSequence.actionStates.Remove(removedStates.Pop());

            while (insertState.Count != 0)
                targetSequence.actionStates.Insert(insertState.Pop(), new ActionState());

            if (GUI.changed)
            {
                string path = AssetDatabase.GetAssetPath(targetSequence);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
        GUILayout.EndVertical();
    }
}
