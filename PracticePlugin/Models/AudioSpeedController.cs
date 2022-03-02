using BS_Utils.Utilities;
using IPA.Utilities;
using PracticePlugin.Configuration;
using PracticePlugin.Views;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Models
{
    public class AudioSpeedController : MonoBehaviour, IInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        public float TimeScale
        {
            get => this._timeScale;
            set => this.SetTimeScale(ref this._timeScale, value);
        }
        private float _timeScale = 1;
        public bool PlayingNewSong { get; private set; }
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
            if (!_songTimeInfoEntity.PracticeMode) {
                return;
            }
            this._audioSource = this._audioTimeSyncController.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
            this._practiceUI.PropertyChanged += this.PracticeUI_PropertyChanged;
            BSEvents.levelFailed += this.BSEvents_levelFailed;

            if (this._songTimeInfoEntity.LastLevelID != this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID &&
                !string.IsNullOrEmpty(this._songTimeInfoEntity.LastLevelID)) {
                this.PlayingNewSong = true;
                // TimeScale = 1;
                this._songTimeInfoEntity.LastLevelID = this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            }
            else {
                this.PlayingNewSong = false;
            }
            this._songTimeInfoEntity.LastLevelID = this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            if (!this._songTimeInfoEntity.PracticeMode) {
                this.TimeScale = Mathf.Clamp(this.TimeScale, 1, SpeedMaxSize);
            }
            if (this._songTimeInfoEntity.PracticeMode) {
                if (this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul != 1f) {
                    this.TimeScale = this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
                }
                else {
                    this.TimeScale = this._gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
                }
            }
        }
        public void UpdateSpawnMovementData(float njs, float noteJumpStartBeatOffset)
        {
            var spawnMovementData = this._spawnController.GetPrivateField<BeatmapObjectSpawnMovementData>("_beatmapObjectSpawnMovementData");
            var bpm = this._spawnController.GetPrivateField<VariableBpmProcessor>("_variableBpmProcessor").currentBpm;
            if (PluginConfig.Instance.AdjustNJSWithSpeed) {
                var newNJS = njs * (1 / this.TimeScale);
                njs = newNJS;
            }
            spawnMovementData.SetPrivateField("_startNoteJumpMovementSpeed", njs);
            spawnMovementData.SetPrivateField("_noteJumpStartBeatOffset", noteJumpStartBeatOffset);
            spawnMovementData.Update(bpm, this._spawnController.GetPrivateField<float>("_jumpOffsetY"));
        }
        private void PracticeUI_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Logger.Debug("PracticeUI_PropertyChanged");
            if (e.PropertyName == nameof(PracticeUI.Offset) || e.PropertyName == nameof(PracticeUI.NJS)) {
                this.UpdateSpawnMovementData(this._practiceUI.NJS, this._practiceUI.Offset);
            }
            else if (e.PropertyName == nameof(PracticeUI.Speed)) {
                Logger.Debug($"{this._practiceUI.Speed}");
                this.TimeScale = (float)(this._practiceUI.Speed / 100d);
            }
        }

        private void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults results)
        {
            var endTime = results.endSongTime;
            var length = this._audioTimeSyncController.songTime;
            this._songTimeInfoEntity.FailTimeText = $"<#ff0000>Failed At</color> - {Math.Floor(endTime / 60):N0}:{Math.Floor(endTime % 60):00}  /  {Math.Floor(length / 60):N0}:{Math.Floor(length % 60):00}";
            this._songTimeInfoEntity.ShowFailTextNext = true;
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

            var initData = this._audioTimeSyncController.GetPrivateField<AudioTimeSyncController.InitData>("_initData");
            var newInitData = new AudioTimeSyncController.InitData(
                initData.audioClip,
                this._audioTimeSyncController.songTime,
                initData.songTimeOffset,
                field);
            this._audioTimeSyncController.SetPrivateField("_initData", newInitData);
            //Chipmunk Removal as per base game
            if (!PluginConfig.Instance.DisablePitchCorrection) {
                if (field == 1f) {
                    this._mixer.musicPitch = 1;
                }
                else {
                    this._mixer.musicPitch = 1f / field;
                }
            }
            else {
                this._mixer.musicPitch = 1f;
            }
            ResetTimeSync(this._audioTimeSyncController, field, newInitData);
#if false
            //       AudioTimeSync.SetPrivateField("_timeScale", value);
            //        AudioTimeSync.Init(_songAudio.clip, _songAudio.time, AudioTimeSync.GetPrivateField<float>("_songTimeOffset"), value);
            if (field == 1f)
                _mixer.musicPitch = 1;
            else
                _mixer.musicPitch = 1f / field;
            if (!IsEqualToOne(field)) {

                if (AudioTimeSync != null) {
                    //           AudioTimeSync.forcedNoAudioSync = true;
                }
            }
            else {
                if (AudioTimeSync != null) {
                    //           AudioTimeSync.forcedNoAudioSync = false;
                }
            }
            if (AudioTimeSync != null) {
                //     AudioTimeSync.SetPrivateField("_timeScale", _timeScale); // = _timeScale;
                //     AudioTimeSync.Init(_songAudio.clip, _songAudio.time, 
                //           AudioTimeSync.GetPrivateField<float>("_songTimeOffset") - AudioTimeSync.GetPrivateField<FloatSO>("_audioLatency").value, _timeScale);
                Logger.Debug("Called TimeScale");

                if (_songAudio != null) {
                    _songAudio.pitch = field;
                }
                //         AudioTimeSync.forcedNoAudioSync = true;
                //         float num = AudioTimeSync.GetPrivateField<float>("_startSongTime") + AudioTimeSync.GetPrivateField<float>("_songTimeOffset");
                //     AudioTimeSync.SetPrivateField("_audioStartTimeOffsetSinceStart", (Time.timeSinceLevelLoad * _timeScale) - num);
                //   AudioTimeSync.SetPrivateField("_fixingAudioSyncError", false);
                //   AudioTimeSync.SetPrivateField("_prevAudioSamplePos", _songAudio.timeSamples);
                //   AudioTimeSync.SetPrivateField("_playbackLoopIndex", 0);
                //          AudioTimeSync.SetPrivateField("_dspTimeOffset", AudioSettings.dspTime - (double)num);
                //    AudioTimeSync.SetPrivateField("_timeScale", _timeScale); // = _timeScale;
            }
#endif
        }

        private bool IsEqualToOne(float value)
        {
            return Math.Abs(value - 1) < 0.000000001f;
        }
        public static void ResetTimeSync(AudioTimeSyncController timeSync, float newTimeScale, AudioTimeSyncController.InitData newData)
        {
            timeSync.SetPrivateField("_timeScale", newTimeScale);
            timeSync.SetPrivateField("_startSongTime", timeSync.songTime);
            timeSync.SetPrivateField("_audioStartTimeOffsetSinceStart", timeSync.GetProperty<float>("timeSinceStart") - (timeSync.songTime + newData.songTimeOffset));
            timeSync.SetPrivateField("_fixingAudioSyncError", false);
            timeSync.SetPrivateField("_playbackLoopIndex", 0);
            timeSync.GetField<AudioSource, AudioTimeSyncController>("_audioSource").pitch = newTimeScale;
        }

        public void Update()
        {
            var newPos = (this._audioTimeSyncController.songTime + 0.1f) / this._audioTimeSyncController.songLength;
            if (newPos >= this._looperUI.EndTime && this._looperUI.EndTime != 1) {
                this._songSeeker.PlaybackPosition = this._looperUI.StartTime;
                this.ApplyPlaybackPosition();
            }
        }
        public void ApplyPlaybackPosition()
        {
            this._audioSource.timeSamples = Mathf.RoundToInt(Mathf.Lerp(0, this._audioSource.clip.samples, this._songSeeker.PlaybackPosition));
            this._audioSource.time = this._audioSource.time - Mathf.Min(AheadTime, this._audioSource.time);
            this._songSeekBeatmapHandler.OnSongTimeChanged(this._audioSource.time, Mathf.Min(AheadTime, this._audioSource.time));
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
        private const float AheadTime = 1f;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public void Constractor(SongTimeInfoEntity songTimeInfoEntity, BeatmapObjectSpawnController beatmapObjectSpawnController, GameplayCoreSceneSetupData gameplayCoreSceneSetupData, AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler, LooperUI looperUI, PracticeUI practiceUI, SongSeeker songSeeker)
        {
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._spawnController = beatmapObjectSpawnController;
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._audioTimeSyncController = audioTimeSyncController;
            this._songSeekBeatmapHandler = songSeekBeatmapHandler;
            this._looperUI = looperUI;
            this._practiceUI = practiceUI;
            this._songSeeker = songSeeker;
            // メモリリークしそう
            this._mixer = Resources.FindObjectsOfTypeAll<AudioManagerSO>().SingleOrDefault();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    BSEvents.levelFailed -= this.BSEvents_levelFailed;
                    this._practiceUI.PropertyChanged -= this.PracticeUI_PropertyChanged;
                    this._mixer = null;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                this._disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~AudioSpeedController()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
