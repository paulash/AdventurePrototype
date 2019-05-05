using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceManager : MonoBehaviour
{
    List<SequenceInstance> activeSequences = new List<SequenceInstance>(32);

    Queue<SequenceInstance> addedSequences = new Queue<SequenceInstance>();
    Queue<SequenceInstance> removedSequences = new Queue<SequenceInstance>();

    // Start is called before the first frame update
    void Start()
    {
    }

    void OnSequenceComplete(SequenceInstance sequence)
    {
        removedSequences.Enqueue(sequence);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SequenceInstance sequence in activeSequences)
            sequence.Tick();

        while (addedSequences.Count != 0)
        {
            SequenceInstance sequence = addedSequences.Dequeue();

            activeSequences.Add(sequence);
            sequence.onSequenceComplete += OnSequenceComplete;
            sequence.Start();
        }

        while (removedSequences.Count != 0)
            activeSequences.Remove(removedSequences.Dequeue());
    }

    public SequenceInstance ActivateSequence(Sequence sequence)
    {
        SequenceInstance instance = new SequenceInstance(sequence);
        addedSequences.Enqueue(instance);
        return instance;
    }

    public void AbortSequence(SequenceInstance sequence)
    {
        if (!activeSequences.Contains(sequence))
            return;

        removedSequences.Enqueue(sequence);
    }

    public int GetEffectingSequences(string name, ref SequenceInstance[] sequences)
    {
        if (sequences == null) return 0;

        int index = 0;
        for (int i=0; i < activeSequences.Count; i++)
        {
            if (activeSequences[i].IsEffectingActor(name)) // make a hashset of this too? would make this real fast.
            {
                sequences[index++] = activeSequences[i];
                if (index <= sequences.Length)
                    break;
            }
        }

        return index;
    }
}
