using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public delegate void OnClickedSuggestion(string suggestion);

public class SteampunkAutoComplete : MonoBehaviour
{
    public GameObject container;
    public GameObject content;
    public OnClickedSuggestion onClickedSuggestion;

    List<string> activeList = new List<string>(6);
    List<Text> entries = new List<Text>();

    public string Suggestion
    {
        get
        {
            if (activeList.Count == 0)
                return null;
            return activeList[0];
        }
    }

    private void Start()
    {
        foreach (Transform child in content.transform)
        {
            entries.Add(child.GetComponent<Text>());
            child.gameObject.SetActive(false);
        }
        container.SetActive(false);
    }

    public void UpdateList(string input, GameObject target, ref Component[] targetComponents, List<SteamPunkCommandEntry> entries)
    {
        activeList.Clear();
        if (input != "")
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].macro.command.ToLower().StartsWith(input.ToLower()))
                {
                    if (entries[i].method.IsStatic)
                        activeList.Add(entries[i].macro.command);
                    else if (target != null)
                    {
                        for (int c=0; c < targetComponents.Length; c++)
                        {
                            if (targetComponents[c].GetType() == entries[i].method.DeclaringType ||
                                targetComponents[c].GetType().IsSubclassOf(entries[i].method.DeclaringType))
                            {
                                activeList.Add(entries[i].macro.command);
                                break;
                            }
                        }
                    }
                }
            }
        }
        UpdateList();
    }

    void UpdateList()
    {
        for (int i=0; i < entries.Count; i++)
        {
            if (i >= activeList.Count) // no more entries.
                entries[i].gameObject.SetActive(false);
            else
            {
                entries[i].gameObject.SetActive(true);
                entries[i].text = activeList[i];
            }
        }

        container.SetActive(activeList.Count != 0);
    }

    public void OnClickSuggestion(int index)
    {
        if (onClickedSuggestion != null)
            onClickedSuggestion(activeList[index]);
    }
}
