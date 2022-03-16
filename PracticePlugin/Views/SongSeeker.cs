using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using PracticePlugin.Models;
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
        private ImageView _seekBackg;
        private ImageView _seekBar;
        private ImageView _seekCursor;
        private LooperUI _looperUI;
        private TMP_Text _currentTime;
        private TMP_Text _timeLength;
        private const float s_aheadTime = 1f;

        public static readonly Vector2 SeekBarSize = new Vector2(100, 2);
        public static readonly float HalfSeekBarSize = SeekBarSize.x / 2;

        private static readonly Vector2 s_parentSize = new Vector2(100, 4);
        private static readonly Color s_backgroundColor = new Color(0, 0, 0, 0.25f);
        private static readonly Color s_foregroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);

        private static readonly Vector2 s_seekCursorSize = new Vector2(4, 4);
        private static readonly Color s_seekCursorColor = new Color(1, 1, 1, 1f);

        private const float s_stickToLooperCursorDistance = 0.02f;
        private bool _init = false;

        private const float s_looperUITopMargin = -5f;
        internal int _startTimeSamples;
        private bool _isPractice = false;
        [Inject]
        public void Constractor(AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler, LooperUI looperUI, GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._songSeekBeatmapHandler = songSeekBeatmapHandler;
            this._looperUI = looperUI;
            this._songAudioSource = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            this._isPractice = gameplayCoreSceneSetupData.practiceSettings != null;
        }
        public void SetPlaybackPosition(float value, float start, float end)
        {
            this.PlaybackPosition = Mathf.Clamp(value, start, end);
        }
        public void Initialize()
        {
            this.gameObject.AddComponent<RectTransform>();
            var tex = Texture2D.whiteTexture;
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);

            var rectTransform = this.transform as RectTransform;
            rectTransform.anchorMin = Vector2.right * 0.5f;
            rectTransform.anchorMax = Vector2.right * 0.5f;
            rectTransform.sizeDelta = s_parentSize;
            rectTransform.anchoredPosition = new Vector2(0, 13);

            this._seekBackg = new GameObject("Background").AddComponent<ImageView>();
            rectTransform = this._seekBackg.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.sizeDelta = SeekBarSize + new Vector2(0, 7);
            rectTransform.anchoredPosition = new Vector2(0, -1);
            this._seekBackg.sprite = sprite;
            this._seekBackg.type = Image.Type.Simple;
            this._seekBackg.color = s_backgroundColor;
            this._seekBackg.material = Utilities.ImageResources.NoGlowMat;

            this._seekBar = new GameObject("Seek Bar").AddComponent<ImageView>();
            rectTransform = this._seekBar.rectTransform;
            rectTransform.SetParent(this.transform, false);
            rectTransform.sizeDelta = SeekBarSize;

            this._seekBar.sprite = sprite;
            this._seekBar.type = Image.Type.Filled;
            this._seekBar.fillMethod = Image.FillMethod.Horizontal;
            this._seekBar.color = s_foregroundColor;
            this._seekBar.material = Utilities.ImageResources.NoGlowMat;

            this._seekCursor = new GameObject("Seek Cursor").AddComponent<ImageView>();
            rectTransform = this._seekCursor.rectTransform;
            rectTransform.SetParent(this._seekBar.transform, false);
            rectTransform.anchorMin = Vector2.up * 0.5f;
            rectTransform.anchorMax = Vector2.up * 0.5f;
            rectTransform.sizeDelta = s_seekCursorSize;

            this._seekCursor.sprite = sprite;
            this._seekCursor.type = Image.Type.Simple;
            this._seekCursor.color = s_seekCursorColor;
            this._seekCursor.material = Utilities.ImageResources.NoGlowMat;
            this._currentTime = BeatSaberUI.CreateText(this.GetComponent<RectTransform>(), "0:00", new Vector2(-83f, -1f));
            this._currentTime.fontSize = 5f;
            this._currentTime.alignment = TextAlignmentOptions.Right;

            this._timeLength = BeatSaberUI.CreateText(this.GetComponent<RectTransform>(), "0:00", new Vector2(87f, -1f));
            this._timeLength.fontSize = 5f;

            this._looperUI.gameObject.transform.SetParent(this._seekBar.rectTransform, false);
            rectTransform = this._looperUI.transform as RectTransform;
            rectTransform.sizeDelta = SeekBarSize;
            rectTransform.anchoredPosition = new Vector2(0, s_looperUITopMargin - 2f);
            this._looperUI.OnDragEndEvent += this.LooperUIOnOnDragEndEvent;
            if (this._looperUI.StartTime != 0) {
                this.PlaybackPosition = this._looperUI.StartTime;
            }
        }

        private void LooperUIOnOnDragEndEvent()
        {
            this.PlaybackPosition = Mathf.Clamp(this.PlaybackPosition, this._looperUI.StartTime, this._looperUI.EndTime);
        }

        public void OnEnable()
        {
            if (this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }

            this._startTimeSamples = this._songAudioSource.timeSamples;
            this.PlaybackPosition = (float)this._songAudioSource.timeSamples / this._songAudioSource.clip.samples;
            this._timeLength.text = FormatTimeSpan(TimeSpan.FromSeconds(this._songAudioSource.clip.length));
            this.UpdateCurrentTimeText(this.PlaybackPosition);

        }

        public void OnDisable()
        {
            this._init = true;
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
            if (!this._isPractice) {
                return;
            }
            if (this._looperUI == null || this._songAudioSource == null || this._songAudioSource.clip == null) {
                return;
            }
            if (!this._init) {
                this.PlaybackPosition = (float)this._songAudioSource.timeSamples / this._songAudioSource.clip.samples;
            }
            this._looperUI.UpdateCursorPosition(this.PlaybackPosition);
        }

        public void LateUpdate()
        {
            if (!this._isPractice) {
                return;
            }
            if (this._looperUI == null || this._seekBar == null || this._seekCursor == null) {
                return;
            }
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
            this._songAudioSource.time -= Mathf.Min(s_aheadTime, this._songAudioSource.time);
            this._songSeekBeatmapHandler.OnSongTimeChanged(this._songAudioSource.time, Mathf.Min(s_aheadTime, this._songAudioSource.time));
        }

        private void UpdateCurrentTimeText(float playbackPos)
        {
            if (this._currentTime == null || this._songAudioSource == null) {
                return;
            }
            this._currentTime.text = FormatTimeSpan(TimeSpan.FromSeconds(Mathf.Lerp(0, this._songAudioSource.clip.length, playbackPos)));
        }

        private void CheckLooperCursorStick()
        {
            if (Mathf.Abs(this.PlaybackPosition - this._looperUI.StartTime) <= s_stickToLooperCursorDistance) {
                this.PlaybackPosition = this._looperUI.StartTime;
            }
            else if (Mathf.Abs(this.PlaybackPosition - this._looperUI.EndTime) <= s_stickToLooperCursorDistance) {
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
