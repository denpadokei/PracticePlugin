using System;
using TMPro;
using UnityEngine;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using Zenject;
using PracticePlugin.Models;
using PracticePlugin.Views;
using IPA.Utilities;

namespace PracticePlugin
{
    public class UIElementsCreator : MonoBehaviour, IInitializable
    {
        public event Action<float> ValueChangedEvent;
        private SongSeeker _songSeeker;
        internal static float defaultNJS;
        internal static float defaultOffset;
        internal PracticeUI practiceUI;
        private SongTimeInfoEntity _songTimeInfoEntity;
        public BeatmapObjectSpawnController _spawnController;
        AudioTimeSyncController _audioTimeSyncController;
        internal static float _newTimeScale { get; private set; } = 1f;

        [Inject]
        public void Constractor(BeatmapObjectSpawnController beatmapObjectSpawnController, SongTimeInfoEntity songTimeInfoEntity, PracticeUI practiceUI, SongSeeker songSeeker, AudioTimeSyncController audioTimeSyncController)
        {
            this._spawnController = beatmapObjectSpawnController;
            this._songTimeInfoEntity = songTimeInfoEntity;
            this.practiceUI = practiceUI;
            this._songSeeker = songSeeker;
            this._audioTimeSyncController = audioTimeSyncController;
        }
        public void Initialize()
        {
            InitDelayed();
        }
        private void InitDelayed()
        {
            if (_songTimeInfoEntity.PracticeMode)
            {
                new GameObject("No Fail Game Energy").AddComponent<NoFailGameEnergy>();
                defaultNJS = _spawnController.GetPrivateField<BeatmapObjectSpawnController.InitData>("_initData").noteJumpMovementSpeed;
           //     PracticeUI.instance.njs = defaultNJS;
                //        Logger.Debug("NJS: " + UIElementsCreator.defaultNJS);
                defaultOffset = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.noteJumpStartBeatOffset;
          //      PracticeUI.instance.offset = defaultOffset;
                //        Logger.Debug("Offset: " + UIElementsCreator.defaultOffset);
            }
        }

        public void UpdateSpawnMovementData(float njs, float noteJumpStartBeatOffset)
        {
            BeatmapObjectSpawnMovementData spawnMovementData = _spawnController.GetPrivateField<BeatmapObjectSpawnMovementData>("_beatmapObjectSpawnMovementData");

            float bpm = _spawnController.GetPrivateField<VariableBpmProcessor>("_variableBpmProcessor").currentBpm;


            if (this._songTimeInfoEntity.adjustNJSWithSpeed) {
                float newNJS = njs * (1 / this._songTimeInfoEntity.TimeScale);
                njs = newNJS;
            }



            spawnMovementData.SetPrivateField("_startNoteJumpMovementSpeed", njs);
            spawnMovementData.SetPrivateField("_noteJumpStartBeatOffset", noteJumpStartBeatOffset);

            spawnMovementData.Update(bpm, _spawnController.GetPrivateField<float>("_jumpOffsetY"));


        }


        public void SpawnOffsetController_ValueChangedEvent(float offset)
        {
            //  Plugin.AdjustNjsAndOffset();
            UpdateSpawnMovementData(practiceUI.njs, practiceUI.offset);
        }

        public void NjsController_ValueChangedEvent(float njs)
        {
            //  Plugin.AdjustNjsAndOffset();
            UpdateSpawnMovementData(practiceUI.njs, practiceUI.offset);

        }

        private void OnDisable()
        {
            try {
                ValueChangedEvent?.Invoke(_newTimeScale);
                if (this._audioTimeSyncController.songTime > 0) {
                    var audioSouece = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
                    _songSeeker._startTimeSamples = audioSouece.timeSamples - 1;
                    _songSeeker.ApplyPlaybackPosition();
                    this._songTimeInfoEntity.TimeScale = practiceUI.speed;
                }
                //      Destroy(_speedSettings);
            }
            catch (Exception e) {
                Logger.Error(e);
            }
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