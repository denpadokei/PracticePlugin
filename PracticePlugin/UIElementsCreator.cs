using IPA.Utilities;
using PracticePlugin.Models;
using PracticePlugin.Views;
using System;
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
        internal static float _newTimeScale { get; private set; } = 1f;

        [Inject]
        public void Constractor(BeatmapObjectSpawnController beatmapObjectSpawnController, SongTimeInfoEntity songTimeInfoEntity, SongSeeker songSeeker, AudioTimeSyncController audioTimeSyncController)
        {
            this._spawnController = beatmapObjectSpawnController;
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._songSeeker = songSeeker;
            this._audioTimeSyncController = audioTimeSyncController;
        }
        public void Initialize()
        {
            this.InitDelayed();
        }
        private void InitDelayed()
        {
            if (this._songTimeInfoEntity.PracticeMode) {
                new GameObject("No Fail Game Energy").AddComponent<NoFailGameEnergy>();
                defaultNJS = this._spawnController.GetPrivateField<BeatmapObjectSpawnController.InitData>("_initData").noteJumpMovementSpeed;
                //     PracticeUI.instance.njs = defaultNJS;
                //        Logger.Debug("NJS: " + UIElementsCreator.defaultNJS);
                defaultOffset = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.noteJumpStartBeatOffset;
                //      PracticeUI.instance.offset = defaultOffset;
                //        Logger.Debug("Offset: " + UIElementsCreator.defaultOffset);
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