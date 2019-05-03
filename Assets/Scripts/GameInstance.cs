using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    static public GameInstance Singleton { get; private set; }

    public VariableManager VariableManager { get; private set; }
    public SequenceManager SequenceManager { get; private set; }

    void Awake()
    {
        if (Singleton != null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);

        VariableManager = GetComponent<VariableManager>();
        SequenceManager = GetComponent<SequenceManager>();
    }
}
