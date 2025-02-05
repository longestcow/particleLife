using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickEvent : MonoBehaviour, IPointerDownHandler
{
    GameManager manager;
    void Start()
    {
       manager = GameObject.Find("GameManager").GetComponent<GameManager>(); 
    }
    public virtual void OnPointerDown(PointerEventData eventData){
        int y = transform.GetSiblingIndex();
        int x = transform.parent.GetSiblingIndex();

        if(x==0 || y==0) return;

        if(eventData.button == PointerEventData.InputButton.Left){
            manager.updateMatrix(x-1,y-1,false,gameObject);
        }
        else if(eventData.button == PointerEventData.InputButton.Right){
            manager.updateMatrix(x-1,y-1,true,gameObject);
        }
    }
}
