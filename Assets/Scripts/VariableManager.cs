using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum VariableType
{
    None,
    String
}

[System.Serializable]
public struct VariableTest
{
    public string owner;
    public string name;
    public string value;
    public bool isTrue;
}

[System.Serializable]
public struct Variable
{
    public VariableType type;
    public string value;

    static public Variable Invalid = new Variable() { type = VariableType.None };
}

public class VariableManager : MonoBehaviour
{
    //Dictionary<string, Dictionary<string, Variable>> variables = new Dictionary<string, Dictionary<string, Variable>>();


    void Awake()
    {
        // load saved variables into dictionary.
        //Dictionary<string, Variable> ownerVar = new Dictionary<string, Variable>();
        //ownerVar.Add("test_v", new Variable() { type = VariableType.String, value = "yes" });
        //variables.Add("test", ownerVar);

    }

    public Variable GetVariable(string owner, string name)
    {
        /*
        Dictionary<string, Variable> ownerSet = null;
        if (variables.TryGetValue(owner, out ownerSet))
        {
            Variable variable = new Variable();
            if (ownerSet.TryGetValue(name, out variable))
                return variable;
        }
        */
        return Variable.Invalid;
    }

    public bool IsVariableTrue(VariableTest test)
    {
        Variable variable = GetVariable(test.owner, test.name);
        return test.isTrue ? variable.value == test.value : variable.value != test.value;
    }
}
