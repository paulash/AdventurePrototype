using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item", order = 1)]
public class Item : ScriptableObject
{
    public Sprite icon;
    public string displayName;
    public string displayDescription;

    public Sequence dropSequence;
}
