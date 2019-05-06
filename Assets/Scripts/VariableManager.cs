using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class VariablePair
{
    public string name;

    public int vInt;
    public float vFloat;
    public Item vItem;
    public string vString;
    public Vector2 vVector2;
    public bool vBoolean;
    public Sequence vSequence;

    public object GetValue()
    {
        switch (type)
        {
            case VariableType.Item:
                return vItem;
            case VariableType.Int:
                return vInt;
            case VariableType.Float:
                return vFloat;
            case VariableType.String:
                return vString;
            case VariableType.Bool:
                return vBoolean;
            case VariableType.Vector2:
                return vVector2;
            case VariableType.Sequence:
                return vSequence;
        }
        return null;
    }

    public void SetValue(object value)
    {
        switch (type)
        {
            case VariableType.Item:
                vItem = (Item)value;
                return;
            case VariableType.Int:
                vInt = (int)value;
                return;
            case VariableType.Float:
                vFloat = (float)value;
                return;
            case VariableType.String:
                vString = (string)value;
                return;
            case VariableType.Bool:
                vBoolean = (bool)value;
                return;
            case VariableType.Vector2:
                vVector2 = (Vector2)value;
                return;
            case VariableType.Sequence:
                vSequence = (Sequence)value;
                return;
        }
    }

    public VariableType type;
}

public enum VariableType
{
    Invalid, Vector2, Float, String, Int, Bool, Item, Sequence, VariableTest
}

[System.Serializable]
public class VariableGroup
{
    [SerializeField]
    public string name;

    [SerializeField]
    public List<VariablePair> variables = new List<VariablePair>();

    public VariablePair GetPairByName(string name)
    {
        for (int i=0; i < variables.Count; i++)
        {
            if (variables[i].name == name)
                return variables[i];
        }
        return null;
    }
}

public class VariableManager : MonoBehaviour
{
    List<VariableGroup> variables = new List<VariableGroup>();

    void Awake()
    {
        variables.Add(new VariableGroup() { name = "Global", variables = new List<VariablePair>() });
    }

    public VariableGroup GetVariableSetByName(string owner)
    {
        for (int i=0; i < variables.Count; i++)
        {
            if (variables[i].name == owner)
                return variables[i];
        }
        return null;
    }

    public T Get<T>(string owner, string name)
    {
        VariableGroup set = GetVariableSetByName(owner);
        if (set == null) return default(T);
        VariablePair pair = set.GetPairByName(name);
        if (pair == null) return default(T);

        return (T)pair.GetValue();
    }

    public void Set(string groupName, string variableName, object value)
    {
        VariableGroup set = GetVariableSetByName(groupName);
        if (set == null) return;
        VariablePair pair = set.GetPairByName(variableName);
        if (pair == null) return;
        pair.SetValue(value);
    }

    public void CreateGroup(string groupName)
    {

    }

    public void CreateGroupVariable(string groupName, string variableName, VariableType variableType)
    {

    }

    public void RemoveGroup(string groupName)
    {

    }

    public void RemoveGroupVariable(string groupName, string variableName)
    {

    }
}
