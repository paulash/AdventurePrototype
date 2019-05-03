using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SPPPointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject target;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        target.SendMessage("OnPointerEnter", eventData);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        target.SendMessage("OnPointerExit", eventData);
    }
}
