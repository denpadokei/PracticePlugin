using BeatSaberMarkupLanguage;
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
    public class SongSeeker : MonoBehaviour, IDragHandler, IPointerDownHandler, IInitializable
    {
        public float PlaybackPosition { get; set; }
        private AudioTimeSyncController _audioTimeSyncController;
        private AudioSource _songAudioSource;
        private SongSeekBeatmapHandler _songSeekBeatmapHandler;
        private PauseController _pauseController;
        private ImageView _seekBackg;
        private ImageView _seekBar;
        private ImageView _seekCursor;
        private LooperUI _looperUI;
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
        private const float StickToLooperCursorDistance = 0.02f;
        private bool init = false;

        private const float LooperUITopMargin = -5f;
        internal int _startTimeSamples;
        [Inject]
        public void Constractor(AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler, PauseController pauseController, LooperUI looperUI)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._songSeekBeatmapHandler = songSeekBeatmapHandler;
            this._pauseController = pauseController;
            this._looperUI = looperUI;
        }
        public void SetPlaybackPosition(float value, float start, float end)
        {
            this.PlaybackPosition = Mathf.Clamp(value, start, end);
        }
        public void Initialize()
        {
            Logger.Debug("Initialize");
            this.gameObject.AddComponent<RectTransform>();
            this._songAudioSource = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            var tex = Texture2D.whiteTexture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);

            var rectTransform = this.transform as RectTransform;
            rectTransform.anchorMin = Vector2.right * 0.5f;
            rectTransform.anchorMax = Vector2.right * 0.5f;
            rectTransform.sizeDelta = ParentSize;
            rectTransform.anchoredPosition = new Vector2(0, 13);

            this._seekBackg = new GameObject("Background").AddComponent<ImageView>();
            rectTransform = this._seekBackg.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.sizeDelta = SeekBarSize + new Vector2(0, 7);
            rectTransform.anchoredPosition = new Vector2(0, -1);
            this._seekBackg.sprite = sprite;
            this._seekBackg.type = Image.Type.Simple;
            this._seekBackg.color = BackgroundColor;
            this._seekBackg.material = Utilities.ImageResources.NoGlowMat;

            this._seekBar = new GameObject("Seek Bar").AddComponent<ImageView>();
            rectTransform = this._seekBar.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.sizeDelta = SeekBarSize;

            this._seekBar.sprite = sprite;
            this._seekBar.type = Image.Type.Filled;
            this._seekBar.fillMethod = Image.FillMethod.Horizontal;
            this._seekBar.color = ForegroundColor;
            this._seekBar.material = Utilities.ImageResources.NoGlowMat;

            this._seekCursor = new GameObject("Seek Cursor").AddComponent<ImageView>();
            rectTransform = this._seekCursor.rectTransform;
            rectTransform.SetParent(this._seekBar.transform, false);
            rectTransform.anchorMin = Vector2.up * 0.5f;
            rectTransform.anchorMax = Vector2.up * 0.5f;
            rectTransform.sizeDelta = SeekCursorSize;

            this._seekCursor.sprite = sprite;
            this._seekCursor.type = Image.Type.Simple;
            this._seekCursor.color = SeekCursorColor;
            this._seekCursor.material = Utilities.ImageResources.NoGlowMat;
            this._currentTime = BeatSaberUI.CreateText(this.GetComponent<RectTransform>(), "0:00", new Vector2(-83f, -1f));
            this._currentTime.fontSize = 5f;
            this._currentTime.alignment = TextAlignmentOptions.Right;

            this._timeLength = BeatSaberUI.CreateText(this.GetComponent<RectTransform>(), "0:00", new Vector2(87f, -1f));
            this._timeLength.fontSize = 5f;

            this._looperUI.gameObject.transform.SetParent(this._seekBar.rectTransform, false);
            rectTransform = this._looperUI.transform as RectTransform;
            rectTransform.sizeDelta = SeekBarSize;
            rectTransform.anchoredPosition = new Vector2(0, LooperUITopMargin - 2f);
            this._looperUI.OnDragEndEvent += this.LooperUIOnOnDragEndEvent;
            if (this._looperUI.StartTime != 0) {
                this.PlaybackPosition = this._looperUI.StartTime;
            }
        }

        private void LooperUIOnOnDragEndEvent()
        {
            this.PlaybackPosition = Mathf.Clamp(this.PlaybackPosition, this._looperUI.StartTime, this._looperUI.EndTime);
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
            this.init = true;
            if (this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }

            var newTimeSamples = Mathf.RoundToInt(Mathf.Lerp(0, this._songAudioSource.clip.samples, this.PlaybackPosition));
            if (this._startTimeSamples == newTimeSamples) {
                return;
            }

            this.ApplyPlaybackPosition();
        }


        public void Update()
        {
            if (this._looperUI == null || this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }
            if (!this.init) {
                this.PlaybackPosition = (float)this._songAudioSource.timeSamples / this._songAudioSource.clip.samples;
            }
            this._looperUI.UpdateCursorPosition(this.PlaybackPosition);
        }

        private void LateUpdate()
        {
            var clampedPos = Mathf.Clamp(this.PlaybackPosition, this._looperUI.StartTime, this._looperUI.EndTime);
            this._seekBar.fillAmount = clampedPos;
            this._seekCursor.rectTransform.anchoredPosition =
                new Vector2(Mathf.Lerp(0, SeekBarSize.x, clampedPos), 0);
            this.UpdateCurrentTimeText(clampedPos);
        }
        protected void OnDestroy()
        {
            this._looperUI.OnDragEndEvent -= this.LooperUIOnOnDragEndEvent;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var hovering = (eventData.hovered.Count > 0);
            if (!hovering) { return; }

            var clampedX = Mathf.Clamp(eventData.position.x, -1.0f, 1.0f);
            this.PlaybackPosition = (clampedX + 1f) * 0.5f; // seekbar position [0.0 - 1.0]
            this.CheckLooperCursorStick();
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
            this.CheckLooperCursorStick();
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

        private void CheckLooperCursorStick()
        {
            if (Mathf.Abs(this.PlaybackPosition - this._looperUI.StartTime) <= StickToLooperCursorDistance) {
                this.PlaybackPosition = this._looperUI.StartTime;
            }
            else if (Mathf.Abs(this.PlaybackPosition - this._looperUI.EndTime) <= StickToLooperCursorDistance) {
                this.PlaybackPosition = this._looperUI.EndTime;
            }

            this.PlaybackPosition = Mathf.Clamp(this.PlaybackPosition, this._looperUI.StartTime, this._looperUI.EndTime);
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            return ts.ToString((int)ts.TotalHours > 0 ? @"h\:m\:ss" : @"m\:ss");
        }
    }
}
