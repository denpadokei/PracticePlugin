using BeatSaberMarkupLanguage;
using IPA.Utilities;
using PracticePlugin.Models;
using PracticePlugin.Views;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PracticePlugin
{
    public class UIElementsCreator : MonoBehaviour, IInitializable
    {
        public event Action<float> ValueChangedEvent;
        private SongSeeker _songSeeker;
        internal static float defaultNJS;
        internal static float defaultOffset;
        private SongTimeInfoEntity _songTimeInfoEntity;
        public BeatmapObjectSpawnController _spawnController;
        private AudioTimeSyncController _audioTimeSyncController;
        private PracticeUI _practiceUI;
        internal static float _newTimeScale { get; private set; } = 1f;

        [Inject]
        public void Constractor(BeatmapObjectSpawnController beatmapObjectSpawnController, SongTimeInfoEntity songTimeInfoEntity, SongSeeker songSeeker, AudioTimeSyncController audioTimeSyncController, PracticeUI practiceUI)
        {
            this._spawnController = beatmapObjectSpawnController;
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._songSeeker = songSeeker;
            this._audioTimeSyncController = audioTimeSyncController;
            this._practiceUI = practiceUI;
        }
        public void Initialize()
        {
            this.gameObject.AddComponent<RectTransform>();
            if (this._songTimeInfoEntity.PracticeMode) {
                var canvas = GameObject.Find("PauseMenu").transform.Find("Wrapper").transform.Find("MenuWrapper").transform.Find("Canvas");

                if (canvas == null) {
                    Console.WriteLine("Canvas Null");
                }
                var uiObj = new GameObject("PracticePlugin Seeker UI", typeof(RectTransform));

                (uiObj.transform as RectTransform).anchorMin = new Vector2(0, 0);
                (uiObj.transform as RectTransform).anchorMax = new Vector2(1, 1);
                (uiObj.transform as RectTransform).sizeDelta = new Vector2(0, 0);
                this.transform.SetParent(uiObj.transform as RectTransform, false);
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this._practiceUI.ResourceName), canvas.gameObject, this._practiceUI);
                _practiceUI.PropertyChanged += this.PracticeUI_PropertyChanged;
                uiObj.transform.SetParent(canvas, false);
                uiObj.transform.localScale = new Vector3(1, 1, 1);
                uiObj.transform.localPosition = new Vector3(0f, -3f, 0f);
                //var seekerObj = new GameObject("Song Seeker");
                //seekerObj.transform.SetParent(this.transform as RectTransform, false);
                this._songSeeker.gameObject.transform.SetParent(canvas, false);
            }
        }

        private void PracticeUI_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PracticeUI.offset) || e.PropertyName == nameof(PracticeUI.njs)) {
                this.UpdateSpawnMovementData(_practiceUI.njs, _practiceUI.offset);
            }
        }

        public void UpdateSpawnMovementData(float njs, float noteJumpStartBeatOffset)
        {
            var spawnMovementData = this._spawnController.GetPrivateField<BeatmapObjectSpawnMovementData>("_beatmapObjectSpawnMovementData");

            var bpm = this._spawnController.GetPrivateField<VariableBpmProcessor>("_variableBpmProcessor").currentBpm;


            if (this._songTimeInfoEntity.adjustNJSWithSpeed) {
                var newNJS = njs * (1 / this._songTimeInfoEntity.TimeScale);
                njs = newNJS;
            }



            spawnMovementData.SetPrivateField("_startNoteJumpMovementSpeed", njs);
            spawnMovementData.SetPrivateField("_noteJumpStartBeatOffset", noteJumpStartBeatOffset);

            spawnMovementData.Update(bpm, this._spawnController.GetPrivateField<float>("_jumpOffsetY"));


        }
        private void OnDisable()
        {
            try {
                ValueChangedEvent?.Invoke(_newTimeScale);
                if (this._audioTimeSyncController.songTime > 0) {
                    var audioSouece = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
                    this._songSeeker._startTimeSamples = audioSouece.timeSamples - 1;
                    this._songSeeker.ApplyPlaybackPosition();
                }
                //      Destroy(_speedSettings);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
        }

        public void OnDestroy()
        {
            _practiceUI.PropertyChanged -= this.PracticeUI_PropertyChanged;
        }
        private void SpeedControllerOnValueChangedEvent(float timeScale)
        {
            _newTimeScale = timeScale;
            //      njsController.Refresh(true);
            //     spawnOffsetController.Refresh(true);
            /*
            _newTimeScale = timeScale;
            if (Math.Abs(_newTimeScale - 1) > 0.0000000001f)
            {
                //      spawnOffsetController.enabled = false;
                //       njsController.enabled = false;

            }
            else
            {
                //       spawnOffsetController.enabled = true;
                //        njsController.enabled = true;

            }
            if (Plugin.PracticeMode) return;
            if (!Plugin.HasTimeScaleChanged && Math.Abs(_newTimeScale - 1) > 0.0000000001f)
            {
                _leaderboardText.text = "Leaderboard will be disabled!";
            }
            else
            {
                _leaderboardText.text = Plugin.HasTimeScaleChanged ? "Leaderboard has been disabled\nSet speed to 100% and restart to enable again" : string.Empty;
            }
            */
        }
    }
}