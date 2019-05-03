using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    public Sequence testSequence;
    List<Sequence> activeSequences = new List<Sequence>(32);

    Queue<Sequence> addedSequences = new Queue<Sequence>();
    Queue<Sequence> removedSequences = new Queue<Sequence>();

    // Start is called before the first frame update
    void Start()
    {
        //testSequence.onSequenceComplete += OnSequenceComplete;
    }

    void OnSequenceComplete(Sequence sequence)
    {
        //Debug.Log("OnSequenceComplete: " + sequence.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ActivateSequence(testSequence);
        }

        foreach (Sequence sequence in activeSequences)
            sequence.Tick();

        while (removedSequences.Count != 0)
            activeSequences.Remove(removedSequences.Dequeue());

        while (addedSequences.Count != 0)
        {
            Sequence sequence = addedSequences.Dequeue();

            activeSequences.Add(sequence);
            sequence.onSequenceComplete += OnSequenceComplete;
            sequence.Start();
        }
    }

    public void ActivateSequence(Sequence sequence)
    {
        /*
        activeSequences.Add(sequence);
        sequence.onSequenceComplete += OnSequenceComplete;
        sequence.Start();
        */
        addedSequences.Enqueue(sequence);
    }

    public void AbortSequence(Sequence sequence)
    {
        removedSequences.Enqueue(sequence);
    }
}
