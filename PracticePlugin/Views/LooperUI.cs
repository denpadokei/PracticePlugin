using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using PracticePlugin.Models;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace PracticePlugin.Views
{
    [HotReload]
    public class LooperUI : BSMLAutomaticViewController, IInitializable
    {
        public float StartTime => Mathf.InverseLerp(0, SongSeeker.SeekBarSize.x, this._startCursor.Position);

        public float EndTime => Mathf.InverseLerp(0, SongSeeker.SeekBarSize.x, this._endCursor.Position);

        public string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");

        public event Action OnDragEndEvent;

        private static readonly Vector2 CursorSize = new Vector2(3, 3);

        private static readonly Color StartColor = new Color(0.15f, 0.35f, 0.8f, 0.75f);
        private static readonly Color EndColor = new Color(0.85f, 0.12f, 0.25f, 0.75f);
        private static readonly Color LineDurationColor = new Color(0, 0, 0, 0);

        private const float LineDurationWidth = 1f;
        private const float MinCursorDistance = 4f;
        private const float StickToSeekerCursorDistance = 2f;
        private const float StickToLooperCursorDistance = 0.02f;

        private static float _prevStartTime;
        private static float _prevEndTime = 1f;

        private AudioTimeSyncController _syncController;
        private SongSeeker _songSeeker;

        private ImageView _lineDuration;

        private LooperCursor _startCursor;
        private LooperCursor _endCursor;

        private LooperCursor _draggingCursor;
        private SongTimeInfoEntity _songTimeInfo;

        [UIObject("root-object")]
        private GameObject _root;

        [UIAction("#post-parse")]
        public void PostParse()
        {
            Logger.Debug("PostParse");
            HMMainThreadDispatcher.instance.Enqueue(this.Setup());
        }

        public IEnumerator Setup()
        {
            Logger.Debug("Setup");
            var tex = Texture2D.whiteTexture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);
            yield return new WaitWhile(() => this._root == null);
            Logger.Debug($"{this._root}:{this._root.name}");
            Logger.Debug($"{this._root}:{this._root.GetType()}");
            var bg = this._root.GetComponent<ImageView>(); //new GameObject("Background").AddComponent<ImageView>();
            Logger.Debug($"{bg}:{bg.name}");
            Logger.Debug($"{bg}:{bg.GetType()}");
            var rectTransform = bg.rectTransform;
            rectTransform.sizeDelta = SongSeeker.SeekBarSize + new Vector2(0, 4);
            //rectTransform.anchoredPosition = new Vector2(0, -1);
            bg.sprite = sprite;
            bg.type = Image.Type.Simple;
            bg.color = new Color(0, 0, 0, 0.5f);
            bg.material = Utilities.ImageResources.NoGlowMat;

            this._lineDuration = new GameObject("Line Duration").AddComponent<ImageView>();
            rectTransform = this._lineDuration.rectTransform;
            rectTransform.SetParent(bg.rectTransform, false);
            rectTransform.anchorMin = Vector2.up * 0.5f;
            rectTransform.anchorMax = Vector2.up * 0.5f;
            rectTransform.sizeDelta = Vector2.zero;
            this._lineDuration.sprite = sprite;
            this._lineDuration.type = Image.Type.Simple;
            this._lineDuration.color = LineDurationColor;
            this._lineDuration.material = Utilities.ImageResources.NoGlowMat;

            var startCursorImage = new GameObject("Start Cursor").AddComponent<ImageView>();
            rectTransform = startCursorImage.rectTransform;
            rectTransform.SetParent(bg.rectTransform, false);
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
            rectTransform.SetParent(bg.rectTransform, false);
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

            if (this.StartTime != 0) {
                this._songSeeker.SetPlaybackPosition(this.StartTime, this.StartTime, this.EndTime);
            }
        }

        [Inject]
        public void Constractor(SongSeeker songSeeker, SongTimeInfoEntity songTimeInfoEntity, AudioTimeSyncController audioTimeSyncController)
        {
            this._songSeeker = songSeeker;
            this._syncController = audioTimeSyncController;
            this._songTimeInfo = songTimeInfoEntity;
        }

        public void Initialize()
        {
            Logger.Debug("Initialize");
            if (this._songTimeInfo.PlayingNewSong) {
                _prevStartTime = 0;
                _prevEndTime = 1;
            }
            this._songSeeker.OnDraged += this.SongSeeker_OnDraged;
            this._songSeeker.OnPointerDowned += this.SongSeeker_OnPointerDowned;
            var canvas = GameObject.Find("PauseMenu").transform.Find("Wrapper").transform.Find("MenuWrapper").transform.Find("Canvas");
            if (canvas == null) {
                Logger.Debug("Canvas Null");
            }

            var uiObj = new GameObject("PracticePlugin Seeker UI", typeof(RectTransform));

            (uiObj.transform as RectTransform).anchorMin = new Vector2(0, 0);
            (uiObj.transform as RectTransform).anchorMax = new Vector2(1, 1);
            (uiObj.transform as RectTransform).sizeDelta = new Vector2(0, 0);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this.ResourceName), canvas.gameObject, this);
            uiObj.transform.SetParent(canvas, false);
            uiObj.transform.localScale = new Vector3(1, 1, 1);
            uiObj.transform.localPosition = new Vector3(0f, -3f, 0f);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this._songSeeker.ResourceName), this.gameObject, this._songSeeker);
        }
        private void SongSeeker_OnPointerDowned(PointerEventData obj)
        {
            this.CheckLooperCursorStick();
        }

        private void SongSeeker_OnDraged(PointerEventData obj)
        {
            this.CheckLooperCursorStick();
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

        private void CheckLooperCursorStick()
        {
            if (Mathf.Abs(this._songSeeker.PlaybackPosition - this.StartTime) <= StickToLooperCursorDistance) {
                this._songSeeker.SetPlaybackPosition(this.StartTime, this.StartTime, this.EndTime);
            }
            else if (Mathf.Abs(this._songSeeker.PlaybackPosition - this.EndTime) <= StickToLooperCursorDistance) {
                this._songSeeker.SetPlaybackPosition(this.EndTime, this.StartTime, this.EndTime);
            }
            this._songSeeker.SetPlaybackPosition(this._songSeeker.PlaybackPosition, this.StartTime, this.EndTime);
        }

        private void Update()
        {
            var newSeekPos = (this._syncController.songTime + 0.1f) / this._syncController.songLength;
            if (newSeekPos >= this.EndTime && this.EndTime != 1) {
                this._songSeeker.SetPlaybackPosition(this.StartTime, this.StartTime, this.EndTime);
                this._songSeeker.ApplyPlaybackPosition();
            }
            if (this._draggingCursor != null) {
                var eventData = this._draggingCursor.EventData;
                var hovering = (eventData.hovered.Count > 0);
                if (!hovering) { return; }
                var newPos = Mathf.Lerp(0, SongSeeker.SeekBarSize.x, Mathf.InverseLerp(-1, 1, Mathf.Clamp(eventData.position.x, -1f, 1f)));

                var seekerPos = this._songSeeker.PlaybackPosition;
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
            this._songSeeker.SetPlaybackPosition(this._songSeeker.PlaybackPosition, this.StartTime, this.EndTime);
        }

        protected override void OnDestroy()
        {
            this._startCursor.BeginDragEvent -= this.CursorOnBeginDragEvent;
            this._startCursor.EndDragEvent -= this.CursorOnEndDragEvent;

            this._endCursor.BeginDragEvent -= this.CursorOnBeginDragEvent;
            this._endCursor.EndDragEvent -= this.CursorOnEndDragEvent;

            this._songSeeker.OnDraged -= this.SongSeeker_OnDraged;
            this._songSeeker.OnPointerDowned -= this.SongSeeker_OnPointerDowned;

            _prevStartTime = this.StartTime;
            _prevEndTime = this.EndTime;
            base.OnDestroy();
        }
    }
}