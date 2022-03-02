using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PracticePlugin.Models
{
    public class LooperCursor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Type CursorType { get; private set; }
        public PointerEventData EventData { get; private set; }
        public float Position { get; set; }

        public event Action<LooperCursor, PointerEventData> BeginDragEvent;
        public event Action<LooperCursor, PointerEventData> EndDragEvent;

        private RectTransform _rectTransform;
        public void Init(Type cursorType)
        {
            this.CursorType = cursorType;
            this._rectTransform = this.transform as RectTransform;
        }

        public void LateUpdate()
        {
            this._rectTransform.anchoredPosition = new Vector2(this.Position, 0);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
            this.EventData = eventData;
            BeginDragEvent?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
            this.EventData = eventData;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
            this.EventData = eventData;
            EndDragEvent?.Invoke(this, eventData);
        }

        public enum Type
        {
            Start,
            End
        }
    }
}