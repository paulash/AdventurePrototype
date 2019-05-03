using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void OnSelectTarget(GameObject gameObject);

public class SteamPunkHierachy : MonoBehaviour
{
    public GameObject container;
    public GameObject entryPrefab;
    public GameObject content;
    public OnSelectTarget onSelectTarget;

    bool isInside = false;
    bool closedFrame = false;
    bool openFrame = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isInside = true;
    }

    void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isInside = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (openFrame)
        {
            openFrame = false;
            return;
        }

        closedFrame = false;
        if (Input.GetMouseButtonUp(0) && !isInside && container.activeSelf)
        {
            OnCloseHierachy();
            closedFrame = true;
        }
    }

    public void OnOpenHierachy()
    {
        openFrame = true;
        container.SetActive(true);

        foreach (Transform child in content.transform)
            GameObject.Destroy(child.gameObject);

        for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
        {
            GameObject[] allGameObjects = UnityEngine.SceneManagement.SceneManager.GetSceneAt(s).GetRootGameObjects();
            for (int i = 0; i < allGameObjects.Length; i++)
                AddEntry(allGameObjects[i]);
        }
    }

    void ExpandGameObject(GameObject targetGameObject)
    {
        foreach (Transform child in content.transform)
            GameObject.Destroy(child.gameObject);

        AddEntry(targetGameObject, true);

        foreach (Transform child in targetGameObject.transform)
            AddEntry(child.gameObject);
    }

    void AddEntry(GameObject targetGameObject, bool parent = false)
    {
        // TODO: optimize by recycling the gameobjects for the entries.
        GameObject entryGO = GameObject.Instantiate(entryPrefab, content.transform);
        SteamPunkHierachyEntry entry =  entryGO.GetComponent<SteamPunkHierachyEntry>();

        entry.SendMessage("SetGameObject", targetGameObject);

        if (parent)
        {
            entry.SendMessage("SetParent");
            entry.onPressExpand += OnCollapseEntry;
        }
        else
            entry.onPressExpand += OnExpandEntry;

        entry.onSelectGameObject += OnSelectGameObject;
    }

    void OnExpandEntry(SteamPunkHierachyEntry entry)
    {
        ExpandGameObject(entry.TargetGameObject);
    }

    void OnCollapseEntry(SteamPunkHierachyEntry entry)
    {
        if (entry.TargetGameObject.transform.parent != null)
            ExpandGameObject(entry.TargetGameObject.transform.parent.gameObject);
        else
            OnOpenHierachy();
    }

    public void OnCloseHierachy()
    {
        container.SetActive(false);
    }

    void OnSelectGameObject(GameObject targetGameObject)
    {
        if (onSelectTarget != null)
            onSelectTarget(targetGameObject);

        OnCloseHierachy();
    }

    public void ToggleHierachy()
    {
        if (closedFrame) return;
        if (!container.activeSelf)
            OnOpenHierachy();
        else
            OnCloseHierachy();
    }

}
