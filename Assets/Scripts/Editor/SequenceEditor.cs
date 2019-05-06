using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(Sequence))]
public class SequenceEditor : Editor
{
    Sequence targetSequence = null;

    [System.NonSerialized]
    static List<System.Type> actionClasses = null;

    [System.NonSerialized]
    static string[] actionClassNames = null;

    Stack<ActionState> removedStates = new Stack<ActionState>();
    Stack<int> insertState = new Stack<int>();
    Stack<ActionState> cloneState = new Stack<ActionState>();

    [System.NonSerialized]
    static Dictionary<string, ActionVariable[]> variablesByType = new Dictionary<string, ActionVariable[]>();

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

    public override void OnInspectorGUI()
    {
        targetSequence = (Sequence)target;

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
            if (targetSequence.actionStates.Count == 0)
                insertState.Push(0);

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
                    state.ActionClass = GetActionName(EditorGUILayout.Popup("Action", GetActionIndex(state.ActionClass), actionClassNames));
                    state.targetActorName = EditorGUILayout.TextField("Target Actor Name", state.targetActorName);


                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        ActionVariable[] variables = null;

                        if (variablesByType.TryGetValue(state.ActionClass, out variables))
                        {
                            for (int v=0; v < variables.Length; v++)
                            {
                                ActionVariable variable = variables[v];
                                switch (variable.type)
                                {
                                    case VariableType.Item:
                                        state.Set(variable, EditorGUILayout.ObjectField(variable.name, state.Get<Item>(variable), typeof(Item), false));
                                        break;
                                    case VariableType.Sequence:
                                        state.Set(variable, EditorGUILayout.ObjectField(variable.name, state.Get<Sequence>(variable), typeof(Sequence), false));
                                        break;
                                    case VariableType.Float:
                                        state.Set(variable, EditorGUILayout.FloatField(variable.name, state.Get<float>(variable)));
                                        break;
                                    case VariableType.String:
                                        state.Set(variable, EditorGUILayout.TextField(variable.name, state.Get<string>(variable)));
                                        break;
                                    case VariableType.Vector2:
                                        state.Set(variable, EditorGUILayout.Vector2Field(variable.name, state.Get<Vector2>(variable)));
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
                EditorUtility.SetDirty(targetSequence);
        }
        GUILayout.EndVertical();
        DrawDefaultInspector();
    }
}
