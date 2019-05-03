using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    static public GameInstance Singleton { get; private set; }

    public VariableManager variableManager;
    public SequenceManager sequenceManager;

    void Awake()
    {
        if (Singleton != null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }
}
