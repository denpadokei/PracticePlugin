using BeatSaberMarkupLanguage;
using HMUI;
using PracticePlugin.Models;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace PracticePlugin.Views
{
    public class LooperUI : MonoBehaviour, IInitializable
    {
        public float StartTime => Mathf.InverseLerp(0, SongSeeker.SeekBarSize.x, this._startCursor.Position);

        public float EndTime => Mathf.InverseLerp(0, SongSeeker.SeekBarSize.x, this._endCursor.Position);

        public string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");

        public event Action OnDragEndEvent;

        private static readonly Vector2 CursorSize = new Vector2(3, 3);

        private static readonly Color StartColor = new Color(0.15f, 0.35f, 0.8f, 0.75f);
        private static readonly Color EndColor = new Color(0.85f, 0.12f, 0.25f, 0.75f);
        private static readonly Color LineDurationColor = new Color(1, 1, 1, 0.4f);

        private const float LineDurationWidth = 1f;
        private const float MinCursorDistance = 4f;
        private const float StickToSeekerCursorDistance = 2f;

        private static float _prevStartTime;
        private static float _prevEndTime = 1f;
        private ImageView _lineDuration;

        private LooperCursor _startCursor;
        private LooperCursor _endCursor;

        private LooperCursor _draggingCursor;
        private SongTimeInfoEntity _songTimeInfo;
        [Inject]
        public void Constractor(SongTimeInfoEntity songTimeInfoEntity)
        {
            this._songTimeInfo = songTimeInfoEntity;
        }

        public void Initialize()
        {
            Logger.Debug("Initialize");
            this.gameObject.AddComponent<RectTransform>();
            if (this._songTimeInfo.PlayingNewSong) {
                _prevStartTime = 0;
                _prevEndTime = 1;
            }
            var tex = Texture2D.whiteTexture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);

            var bg = new GameObject("Background").AddComponent<ImageView>();
            var rectTransform = bg.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.sizeDelta = SongSeeker.SeekBarSize + new Vector2(0, 4);
            rectTransform.anchoredPosition = new Vector2(0, -1);
            bg.sprite = sprite;
            bg.type = Image.Type.Simple;
            bg.color = new Color(0, 0, 0, 0);
            bg.material = Utilities.ImageResources.NoGlowMat;

            this._lineDuration = new GameObject("Line Duration").AddComponent<ImageView>();
            rectTransform = this._lineDuration.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.anchorMin = Vector2.up * 0.5f;
            rectTransform.anchorMax = Vector2.up * 0.5f;
            rectTransform.sizeDelta = Vector2.zero;
            this._lineDuration.sprite = sprite;
            this._lineDuration.type = Image.Type.Simple;
            this._lineDuration.color = LineDurationColor;
            this._lineDuration.material = Utilities.ImageResources.NoGlowMat;

            var startCursorImage = new GameObject("Start Cursor").AddComponent<ImageView>();
            rectTransform = startCursorImage.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.anchorMin = Vector2.up * 0.5f;
            rectTransform.anchorMax = Vector2.up * 0.5f;
            rectTransform.sizeDelta = CursorSize;
            rectTransform.localEulerAngles = new Vector3(0, 0, 45);
            startCursorImage.sprite = sprite;
            startCursorImage.type = Image.Type.Simple;
            startCursorImage.color = StartColor;
            startCursorImage.material = Utilities.ImageResources.NoGlowMat;

            this._startCursor = startCursorImage.gameObject.AddComponent<LooperCursor>();
            this._startCursor.BeginDragEvent += this.CursorOnBeginDragEvent;
            this._startCursor.EndDragEvent += this.CursorOnEndDragEvent;
            this._startCursor.Position = Mathf.Lerp(0, SongSeeker.SeekBarSize.x, _prevStartTime);

            var endCursorImage = new GameObject("End Cursor").AddComponent<ImageView>();
            rectTransform = endCursorImage.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.anchorMin = Vector2.up * 0.5f;
            rectTransform.anchorMax = Vector2.up * 0.5f;
            rectTransform.sizeDelta = CursorSize;
            rectTransform.localEulerAngles = new Vector3(0, 0, 45);
            endCursorImage.sprite = sprite;
            endCursorImage.type = Image.Type.Simple;
            endCursorImage.color = EndColor;
            endCursorImage.material = Utilities.ImageResources.NoGlowMat;

            this._endCursor = endCursorImage.gameObject.AddComponent<LooperCursor>();
            this._endCursor.BeginDragEvent += this.CursorOnBeginDragEvent;
            this._endCursor.EndDragEvent += this.CursorOnEndDragEvent;
            this._endCursor.Position = Mathf.Lerp(0, SongSeeker.SeekBarSize.x, _prevEndTime);

            this._startCursor.Init(LooperCursor.Type.Start);
            this._endCursor.Init(LooperCursor.Type.End);
        }

        private void CursorOnBeginDragEvent(LooperCursor cursor, PointerEventData eventData)
        {
            this._draggingCursor = cursor;
        }

        private void CursorOnEndDragEvent(LooperCursor cursor, PointerEventData eventData)
        {
            this._draggingCursor = null;
            this.OnDragEndEvent?.Invoke();
        }

        public void UpdateCursorPosition(float playbackPosition)
        {
            if (this._draggingCursor != null) {
                var eventData = this._draggingCursor.EventData;
                var hovering = (eventData.hovered.Count > 0);
                if (!hovering) { return; }
                var newPos = Mathf.Lerp(0, SongSeeker.SeekBarSize.x, Mathf.InverseLerp(-1, 1, Mathf.Clamp(eventData.position.x, -1f, 1f)));

                var seekerPos = playbackPosition;
                if (Mathf.Abs(newPos - seekerPos) <= StickToSeekerCursorDistance) {
                    newPos = seekerPos;
                }

                if (this._draggingCursor.CursorType == LooperCursor.Type.Start) {
                    this._draggingCursor.Position = Mathf.Clamp(newPos, 0, this._endCursor.Position - MinCursorDistance);
                }
                else {
                    this._draggingCursor.Position = Mathf.Clamp(newPos, this._startCursor.Position + MinCursorDistance,
                        SongSeeker.SeekBarSize.x);
                }
            }
            this._lineDuration.rectTransform.sizeDelta = new Vector2(this._endCursor.Position - this._startCursor.Position, LineDurationWidth);
            this._lineDuration.rectTransform.anchoredPosition = new Vector2((this._startCursor.Position + this._endCursor.Position) / 2, 0);
        }

        protected void OnDestroy()
        {
            this._startCursor.BeginDragEvent -= this.CursorOnBeginDragEvent;
            this._startCursor.EndDragEvent -= this.CursorOnEndDragEvent;

            this._endCursor.BeginDragEvent -= this.CursorOnBeginDragEvent;
            this._endCursor.EndDragEvent -= this.CursorOnEndDragEvent;

            _prevStartTime = this.StartTime;
            _prevEndTime = this.EndTime;
        }
    }
}