using IPA.Utilities;
using PracticePlugin.Configuration;
using PracticePlugin.Extentions;
using PracticePlugin.Views;
using System;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Models
{
    public class AudioSpeedController : MonoBehaviour, IInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        private float _timeScale = 1;
        public float TimeScale
        {
            get => this._timeScale;
            set => this.SetTimeScale(ref this._timeScale, value);
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // パブリックメソッド
        public void Initialize()
        {
            if (!this._songTimeInfoEntity.PracticeMode) {
                return;
            }
            this._audioSource = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            this._practiceUI.PropertyChanged += this.PracticeUI_PropertyChanged;
            this._songTimeInfoEntity.LastLevelID = this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            if (this._songTimeInfoEntity.PracticeMode) {
                this._timeScale = !this.IsEqualToOne(this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul)
                    ? this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul
                    : this._gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
            }
            this.ChangeMusicPitch(this.TimeScale);
        }
        public void Update()
        {
            if (!this._songTimeInfoEntity.PracticeMode) {
                return;
            }

            if (this._gamePause != null && this._gamePause.isPaused) {
                return;
            }

            var newPos = (this._audioTimeSyncController.songTime + 0.1f) / this._audioTimeSyncController.songLength;
            if (newPos >= this._looperUI.EndTime && !this.IsEqualToOne(this._looperUI.EndTime)) {
                this._songSeeker.PlaybackPosition = this._looperUI.StartTime;
                this.ApplyPlaybackPosition();
            }
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void SetTimeScale(ref float field, float timeScale)
        {
            if (field == timeScale) {
                return;
            }
            field = timeScale;
            if (!this._audioTimeSyncController) {
                return;
            }

            if (!this._spawnController) {
                return;
            }
            var initData = this._audioTimeSyncController.GetField<AudioTimeSyncController.InitData, AudioTimeSyncController>("_initData");
            var newInitData = new AudioTimeSyncController.InitData(
                initData.audioClip,
                this._audioTimeSyncController.songTime,
                initData.songTimeOffset,
                field);
            this._audioTimeSyncController.SetField("_initData", newInitData);
            //Chipmunk Removal as per base game
            this.ChangeMusicPitch(field);
            this.ResetTimeSync(this._audioTimeSyncController, field, newInitData);
        }
        private bool IsEqualToOne(float value)
        {
            return Mathf.Approximately(value, 1f);
        }
        private void PracticeUI_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PracticeUI.Offset) || e.PropertyName == nameof(PracticeUI.NJS)) {
                this.UpdateSpawnMovementData(this._practiceUI.NJS, this._practiceUI.Offset);
            }
            else if (e.PropertyName == nameof(PracticeUI.Speed)) {
                this.TimeScale = (float)(this._practiceUI.Speed / 100d);
                this.UpdateSpawnMovementData(this._practiceUI.NJS, this._practiceUI.Offset);
            }
        }

        private void UpdateSpawnMovementData(float njs, float noteJumpStartBeatOffset)
        {
            var spawnMovementData = this._spawnController.GetField<BeatmapObjectSpawnMovementData, BeatmapObjectSpawnController>("_beatmapObjectSpawnMovementData");
            var initData = this._spawnController.GetField<BeatmapObjectSpawnController.InitData, BeatmapObjectSpawnController>("_initData");
            var bpm = this._bpmController.currentBpm;
            if (PluginConfig.Instance.AdjustNJSWithSpeed) {
                var newNJS = njs * (1 / this.TimeScale);
                njs = newNJS;
            }
            var oldAheadTime = spawnMovementData.spawnAheadTime;
            var lastProcessedNode = this._beatmapCallbackController.GetLastNode(oldAheadTime);
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_obstacleDataCallbackWrapper"));
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_noteDataCallbackWrapper"));
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_sliderDataCallbackWrapper"));
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_spawnRotationCallbackWrapper"));
            initData.Update(njs, noteJumpStartBeatOffset);
            this._spawnController.SetField("_isInitialized", false);
            this._spawnController.Start();
            var newAheadTime = spawnMovementData.spawnAheadTime;
            if (lastProcessedNode != null) {
                this._beatmapCallbackController.SetNewLastNodeForCallback(lastProcessedNode, newAheadTime);
            }
        }
        private void ResetTimeSync(AudioTimeSyncController timeSync, float newTimeScale, AudioTimeSyncController.InitData newData)
        {
            timeSync.SetField("_timeScale", newTimeScale);
            timeSync.SetField("_startSongTime", timeSync.songTime);
            timeSync.SetField("_audioStartTimeOffsetSinceStart", timeSync.GetProperty<float, AudioTimeSyncController>("timeSinceStart") - (timeSync.songTime + newData.songTimeOffset));
            timeSync.SetField("_fixingAudioSyncError", false);
            timeSync.SetField("_playbackLoopIndex", 0);
            timeSync.GetField<AudioSource, AudioTimeSyncController>("_audioSource").pitch = newTimeScale;
        }
        private void ApplyPlaybackPosition()
        {
            var newSongTime = Mathf.Lerp(0, this._audioTimeSyncController.songEndTime, this._songSeeker.PlaybackPosition);
            this._songSeekBeatmapHandler.OnSongTimeChanged(newSongTime);
        }
        private void ChangeMusicPitch(float pitch)
        {
            this._mixer.musicPitch = this.IsEqualToOne(pitch) ? 1 : 1f / pitch;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        public const float SpeedMaxSize = 5.05f;
        public const float SpeedStepSize = 0.05f;

        public const int NjsMaxSize = 100;
        public const int NjstepSize = 1;
        private BeatmapObjectSpawnController _spawnController;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private AudioTimeSyncController _audioTimeSyncController;
        private AudioSource _audioSource;
        private AudioManagerSO _mixer;
        private SongTimeInfoEntity _songTimeInfoEntity;
        private SongSeekBeatmapHandler _songSeekBeatmapHandler;
        private LooperUI _looperUI;
        private SongSeeker _songSeeker;
        private PracticeUI _practiceUI;
        private bool _disposedValue;
        private const float s_aheadTime = 1f;
        private IBpmController _bpmController;
        private BeatmapCallbacksController _beatmapCallbackController;
        private IGamePause _gamePause;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public void Constractor(AudioManagerSO audioManagerSO, SongTimeInfoEntity songTimeInfoEntity, BeatmapObjectSpawnController beatmapObjectSpawnController, GameplayCoreSceneSetupData gameplayCoreSceneSetupData, AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler, LooperUI looperUI, PracticeUI practiceUI, SongSeeker songSeeker, IBpmController bpmController, BeatmapCallbacksController beatmapCallbacksController, IGamePause gamePause)
        {
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._spawnController = beatmapObjectSpawnController;
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._audioTimeSyncController = audioTimeSyncController;
            this._songSeekBeatmapHandler = songSeekBeatmapHandler;
            this._looperUI = looperUI;
            this._practiceUI = practiceUI;
            this._songSeeker = songSeeker;
            this._mixer = audioManagerSO;
            this._bpmController = bpmController;
            this._beatmapCallbackController = beatmapCallbacksController;
            this._gamePause = gamePause;
            if (this._songTimeInfoEntity.LastLevelID != this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID
                && !string.IsNullOrEmpty(this._songTimeInfoEntity.LastLevelID)) {
                this._songTimeInfoEntity.PlayingNewSong = true;
                this._songTimeInfoEntity.LastLevelID = this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            }
            else {
                this._songTimeInfoEntity.PlayingNewSong = false;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    this._practiceUI.PropertyChanged -= this.PracticeUI_PropertyChanged;
                    this._mixer = null;
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
