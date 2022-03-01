using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace PracticePlugin.Views
{
    [HotReload]
    public class SongSeeker : BSMLAutomaticViewController, IBeginDragHandler, IDragHandler, IPointerDownHandler
    {
        public float PlaybackPosition { get; private set; }
        public string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");
        private AudioTimeSyncController _audioTimeSyncController;
        private AudioSource _songAudioSource;
        private SongSeekBeatmapHandler _songSeekBeatmapHandler;

        private ImageView _seekBackg;
        private ImageView _seekBar;
        private ImageView _seekCursor;
        private TMP_Text _currentTime;
        private TMP_Text _timeLength;
        private const float AheadTime = 1f;

        public static readonly Vector2 SeekBarSize = new Vector2(100, 2);
        public static readonly float HalfSeekBarSize = SeekBarSize.x / 2;

        private static readonly Vector2 ParentSize = new Vector2(100, 4);
        private static readonly Color BackgroundColor = new Color(0, 0, 0, 0.25f);
        private static readonly Color ForegroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        private static readonly Vector2 SeekCursorSize = new Vector2(4, 4);
        private static readonly Color SeekCursorColor = new Color(1, 1, 1, 1f);

        private static readonly Vector2 TimeTextSize = new Vector2(16, 8);
        private const float TimeTextMargin = 4;

        private const float LooperUITopMargin = -5f;
        internal int _startTimeSamples;

        public event Action<PointerEventData> OnDraged;
        public event Action<PointerEventData> OnPointerDowned;

        [UIObject("root-object")]
        private GameObject _root;
        [UIObject("seek-bar")]
        private GameObject _seekbarRoot;
        [UIObject("seek-cursor")]
        private GameObject _seekCursorRoot;

        [Inject]
        public void Constractor(AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._songSeekBeatmapHandler = songSeekBeatmapHandler;
        }

        [UIAction("#post-parse")]
        public void PostParse()
        {
            Logger.Debug("PostParse");
            var tex = Texture2D.whiteTexture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);
            try {
                this._songAudioSource = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            //try {
            //    var rectTransform = this.transform as RectTransform;
            //    rectTransform.anchorMin = Vector2.right * 0.5f;
            //    rectTransform.anchorMax = Vector2.right * 0.5f;
            //    rectTransform.sizeDelta = ParentSize;
            //    rectTransform.anchoredPosition = new Vector2(0, 16);
            //}
            //catch (Exception e) {
            //    Logger.Error(e);
            //}
            try {
                this._seekBackg = this._root.GetComponent<ImageView>();
                var rectTransform = this._seekBackg.rectTransform;
                rectTransform.sizeDelta = SeekBarSize + new Vector2(0, 7);
                //rectTransform.anchoredPosition = new Vector2(0, -1);
                this._seekBackg.sprite = sprite;
                this._seekBackg.type = Image.Type.Simple;
                this._seekBackg.color = BackgroundColor;
                this._seekBackg.material = Utilities.ImageResources.NoGlowMat;
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            try {
                this._seekBar = this._seekbarRoot.GetComponent<ImageView>();
                var rectTransform = this._seekBar.rectTransform;
                rectTransform.sizeDelta = SeekBarSize;

                this._seekBar.sprite = sprite;
                this._seekBar.type = Image.Type.Filled;
                this._seekBar.fillMethod = Image.FillMethod.Horizontal;
                this._seekBar.color = ForegroundColor;
                this._seekBar.material = Utilities.ImageResources.NoGlowMat;
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            try {
                this._seekCursor = _seekCursorRoot.GetComponent<ImageView>();
                var rectTransform = this._seekCursor.rectTransform;
                rectTransform.anchorMin = Vector2.up * 0.5f;
                rectTransform.anchorMax = Vector2.up * 0.5f;
                rectTransform.sizeDelta = SeekCursorSize;

                this._seekCursor.sprite = sprite;
                this._seekCursor.type = Image.Type.Simple;
                this._seekCursor.color = SeekCursorColor;
                this._seekCursor.material = Utilities.ImageResources.NoGlowMat;

            }
            catch (Exception e) {
                Logger.Error(e);
            }
            try {
                this._currentTime = BeatSaberUI.CreateText(this.GetComponent<RectTransform>(), "0:00", new Vector2(-83f, -1f));
                this._currentTime.fontSize = 5f;
                //rectTransform = _currentTime.rectTransform;
                //rectTransform.anchorMin = Vector2.up * 0.5f;
                //rectTransform.anchorMax = Vector2.up * 0.5f;
                //rectTransform.sizeDelta = TimeTextSize;
                //rectTransform.anchoredPosition = new Vector2(-(TimeTextSize.x / 2) - TimeTextMargin, 0);
                //_currentTime.enableAutoSizing = true;
                //_currentTime.fontSizeMin = 1;
                this._currentTime.alignment = TextAlignmentOptions.Right;

                this._timeLength = BeatSaberUI.CreateText(this.GetComponent<RectTransform>(), "0:00", new Vector2(87f, -1f));
                this._timeLength.fontSize = 5f;
                this._timeLength.alignment = TextAlignmentOptions.Left;
            }
            catch (Exception e) {
                Logger.Error(e);
            }
            try {
                var looperObj = new GameObject("Looper UI");
                looperObj.transform.SetParent(this._seekBar.rectTransform, false);
                var rectTransform = looperObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = SeekBarSize;
                rectTransform.anchoredPosition = new Vector2(0, LooperUITopMargin - 2f);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        public void SetPlaybackPosition(float value, float start, float end)
        {
            this.PlaybackPosition = Mathf.Clamp(value, start, end);
        }

        private void OnEnable()
        {
            if (this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }

            this._startTimeSamples = this._songAudioSource.timeSamples;
            this.PlaybackPosition = (float)this._songAudioSource.timeSamples / this._songAudioSource.clip.samples;
            this._timeLength.text = FormatTimeSpan(TimeSpan.FromSeconds(this._songAudioSource.clip.length));
            this.UpdateCurrentTimeText(this.PlaybackPosition);
        }


        private void OnDisable()
        {
            if (this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }

            var newTimeSamples = Mathf.RoundToInt(Mathf.Lerp(0, this._songAudioSource.clip.samples, this.PlaybackPosition));
            if (this._startTimeSamples == newTimeSamples) {
                return;
            }

            this.ApplyPlaybackPosition();
        }

        public void OnUpdate()
        {
            if (this.gameObject.activeInHierarchy || this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }

            if (this.enabled) {
                this.PlaybackPosition = (float)this._songAudioSource.timeSamples / this._songAudioSource.clip.samples;
            }
        }

        private void LateUpdate()
        {
            this._seekBar.fillAmount = this.PlaybackPosition;
            this._seekCursor.rectTransform.anchoredPosition =
                new Vector2(Mathf.Lerp(0, SeekBarSize.x, this.PlaybackPosition), 0);
            this.UpdateCurrentTimeText(this.PlaybackPosition);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            var hovering = (eventData.hovered.Count > 0);
            if (!hovering) { return; }

            var clampedX = Mathf.Clamp(eventData.position.x, -1.0f, 1.0f);
            this.PlaybackPosition = (clampedX + 1f) * 0.5f; // seekbar position [0.0 - 1.0]
            this.OnDraged?.Invoke(eventData);
            this.UpdateCurrentTimeText(this.PlaybackPosition);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
            var y = eventData.position.y - this._seekBar.gameObject.transform.position.y;
            if (y < -0.03f || y > 0.06) {
                return;
            }

            var clampedX = Mathf.Clamp(eventData.position.x, -1.0f, 1.0f);
            this.PlaybackPosition = (clampedX + 1f) * 0.5f; // seekbar position [0.0 - 1.0]
            this.OnPointerDowned?.Invoke(eventData);
            this.UpdateCurrentTimeText(this.PlaybackPosition);
        }
        public void ApplyPlaybackPosition()
        {
            this._songAudioSource.timeSamples = Mathf.RoundToInt(Mathf.Lerp(0, this._songAudioSource.clip.samples, this.PlaybackPosition));
            this._songAudioSource.time = this._songAudioSource.time - Mathf.Min(AheadTime, this._songAudioSource.time);
            this._songSeekBeatmapHandler.OnSongTimeChanged(this._songAudioSource.time, Mathf.Min(AheadTime, this._songAudioSource.time));
            NoFailGameEnergy.hasFailed = false;
        }

        private void UpdateCurrentTimeText(float playbackPos)
        {
            this._currentTime.text = FormatTimeSpan(TimeSpan.FromSeconds(Mathf.Lerp(0, this._songAudioSource.clip.length, playbackPos)));
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            return ts.ToString((int)ts.TotalHours > 0 ? @"h\:m\:ss" : @"m\:ss");
        }
    }
}
