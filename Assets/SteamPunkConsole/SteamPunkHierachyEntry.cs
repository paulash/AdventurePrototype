using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnPressExpand(SteamPunkHierachyEntry entry);
public delegate void OnSelectGameObject(GameObject gameObject);

public class SteamPunkHierachyEntry : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Button expandBtn;
    public Sprite backArrow;

    public OnPressExpand onPressExpand;
    public OnSelectGameObject onSelectGameObject;

    public GameObject TargetGameObject { get; private set; }

    public void OnPressExpand()
    {
        if (onPressExpand != null)
            onPressExpand(this);
    }

    public void OnSelectGameObject()
    {
        if (onSelectGameObject != null)
            onSelectGameObject(TargetGameObject);
    }

    void SetGameObject(GameObject gameObject)
    {
        text.text = gameObject.name;
        expandBtn.gameObject.SetActive(gameObject.transform.childCount != 0);

        TargetGameObject = gameObject;
    }

    void SetParent()
    {
        text.text = "[PARENT] " + text.text;
        expandBtn.GetComponent<UnityEngine.UI.Image>().sprite = backArrow;
    }
}
