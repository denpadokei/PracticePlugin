﻿using IPA.Utilities;
using PracticePlugin.Configuration;
using PracticePlugin.Extentions;
using PracticePlugin.Views;
using SiraUtil.Services;
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
        #region // コマンド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // コマンド用メソッド
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // オーバーライドメソッド
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
            if (!this._songTimeInfoEntity.PracticeMode) {
                this.TimeScale = Mathf.Clamp(this.TimeScale, 1, SpeedMaxSize);
            }
            if (this._songTimeInfoEntity.PracticeMode) {
                if (!this.IsEqualToOne(this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul)) {
                    this.TimeScale = this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
                }
                else {
                    this.TimeScale = this._gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
                }
            }
        }
        public void Update()
        {
            if (!this._songTimeInfoEntity.PracticeMode) {
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
            if (!PluginConfig.Instance.DisablePitchCorrection) {
                if (this.IsEqualToOne(field)) {
                    this._mixer.musicPitch = 1;
                }
                else {
                    this._mixer.musicPitch = 1f / field;
                }
            }
            else {
                this._mixer.musicPitch = 1f;
            }
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
            initData.Update(njs, noteJumpStartBeatOffset);
            this._spawnController.SetField("_isInitialized", false);
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_obstacleDataCallbackWrapper"));
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_noteDataCallbackWrapper"));
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_sliderDataCallbackWrapper"));
            this._beatmapCallbackController.RemoveBeatmapCallback(this._spawnController.GetField<BeatmapDataCallbackWrapper, BeatmapObjectSpawnController>("_spawnRotationCallbackWrapper"));
            this._spawnController.Start();
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
            this._audioSource.timeSamples = Mathf.RoundToInt(Mathf.Lerp(0, this._audioSource.clip.samples, this._songSeeker.PlaybackPosition));
            this._audioSource.time -= Mathf.Min(s_aheadTime, this._audioSource.time);
            this._songSeekBeatmapHandler.OnSongTimeChanged(this._audioSource.time, Mathf.Min(s_aheadTime, this._audioSource.time));
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
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public void Constractor(AudioManagerSO audioManagerSO, SongTimeInfoEntity songTimeInfoEntity, BeatmapObjectSpawnController beatmapObjectSpawnController, GameplayCoreSceneSetupData gameplayCoreSceneSetupData, AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler, LooperUI looperUI, PracticeUI practiceUI, SongSeeker songSeeker, IBpmController bpmController, BeatmapCallbacksController beatmapCallbacksController)
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
