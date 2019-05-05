using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void OnRoomChange(Room room, Actor controlledActor);

public class GameInstance : MonoBehaviour
{
    static public GameInstance Singleton { get; private set; }

    public VariableManager VariableManager { get; private set; }
    public SequenceManager SequenceManager { get; private set; }
    public InputManager InputManager { get; private set; }

    public Actor ControlledActor { get; private set; }

    public Actor testPlayerControlled = null;

    public OnRoomChange onRoomChange;

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
        InputManager = GetComponent<InputManager>();
        ControlledActor = testPlayerControlled;

        //InputManager.
    }

    private void Start()
    {
        if (onRoomChange != null)
            onRoomChange(null, ControlledActor);
    }
}
