using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Demo.Tools
{
    public class LongPressBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Action pointDownAction;
        public Action pointUpAction;

        public void OnPointerDown(PointerEventData eventData)
        {
            pointDownAction?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointUpAction?.Invoke();
        }
    }
}