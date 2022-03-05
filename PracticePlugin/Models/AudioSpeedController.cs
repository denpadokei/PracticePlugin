using IPA.Utilities;
using PracticePlugin.Configuration;
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
            var spawnMovementData = this._spawnController.GetField<BeatmapObjectSpawnMovementData, BeatmapObjectSpawnController>("_beatmapObjectSpawnMovementData");
            var bpm = this._spawnController.GetField<VariableBpmProcessor, BeatmapObjectSpawnController>("_variableBpmProcessor").currentBpm;
            if (PluginConfig.Instance.AdjustNJSWithSpeed) {
                var newNJS = njs * (1 / this.TimeScale);
                njs = newNJS;
            }
            spawnMovementData.SetField("_startNoteJumpMovementSpeed", njs);
            spawnMovementData.SetField("_noteJumpStartBeatOffset", noteJumpStartBeatOffset);
            spawnMovementData.Update(bpm, this._spawnController.GetField<float, BeatmapObjectSpawnController>("_jumpOffsetY"));
        }
        private void PracticeUI_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PracticeUI.Offset) || e.PropertyName == nameof(PracticeUI.NJS)) {
                this.UpdateSpawnMovementData(this._practiceUI.NJS, this._practiceUI.Offset);
            }
            else if (e.PropertyName == nameof(PracticeUI.Speed)) {
                Logger.Debug($"{this._practiceUI.Speed}");
                this.TimeScale = (float)(this._practiceUI.Speed / 100d);
            }
        }
        private void LevelFinisher_StandardLevelFinished(LevelCompletionResults obj)
        {
            Logger.Debug("LevelFinisher_StandardLevelFinished");
            var endTime = obj.endSongTime;
            var length = this._audioTimeSyncController.songLength;
            this._songTimeInfoEntity.FailTimeText = $@"<color=#ff0000>Failed At</color> - {Math.Floor(endTime / 60):N0}:{Math.Floor(endTime % 60):00}  /  {Math.Floor(length / 60):N0}:{Math.Floor(length % 60):00}";
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
            var initData = this._audioTimeSyncController.GetField<AudioTimeSyncController.InitData, AudioTimeSyncController>("_initData");
            var newInitData = new AudioTimeSyncController.InitData(
                initData.audioClip,
                this._audioTimeSyncController.songTime,
                initData.songTimeOffset,
                field);
            this._audioTimeSyncController.SetField("_initData", newInitData);
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
            //       AudioTimeSync.SetField("_timeScale", value);
            //        AudioTimeSync.Init(_songAudio.clip, _songAudio.time, AudioTimeSync.GetField<float>("_songTimeOffset"), value);
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
                //     AudioTimeSync.SetField("_timeScale", _timeScale); // = _timeScale;
                //     AudioTimeSync.Init(_songAudio.clip, _songAudio.time, 
                //           AudioTimeSync.GetField<float>("_songTimeOffset") - AudioTimeSync.GetField<FloatSO>("_audioLatency").value, _timeScale);
                Logger.Debug("Called TimeScale");

                if (_songAudio != null) {
                    _songAudio.pitch = field;
                }
                //         AudioTimeSync.forcedNoAudioSync = true;
                //         float num = AudioTimeSync.GetField<float>("_startSongTime") + AudioTimeSync.GetField<float>("_songTimeOffset");
                //     AudioTimeSync.SetField("_audioStartTimeOffsetSinceStart", (Time.timeSinceLevelLoad * _timeScale) - num);
                //   AudioTimeSync.SetField("_fixingAudioSyncError", false);
                //   AudioTimeSync.SetField("_prevAudioSamplePos", _songAudio.timeSamples);
                //   AudioTimeSync.SetField("_playbackLoopIndex", 0);
                //          AudioTimeSync.SetField("_dspTimeOffset", AudioSettings.dspTime - (double)num);
                //    AudioTimeSync.SetField("_timeScale", _timeScale); // = _timeScale;
            }
#endif
        }

        //private bool IsEqualToOne(float value)
        //{
        //    return Math.Abs(value - 1) < 0.000000001f;
        //}
        public static void ResetTimeSync(AudioTimeSyncController timeSync, float newTimeScale, AudioTimeSyncController.InitData newData)
        {
            timeSync.SetField("_timeScale", newTimeScale);
            timeSync.SetField("_startSongTime", timeSync.songTime);
            timeSync.SetField("_audioStartTimeOffsetSinceStart", timeSync.GetProperty<float, AudioTimeSyncController>("timeSinceStart") - (timeSync.songTime + newData.songTimeOffset));
            timeSync.SetField("_fixingAudioSyncError", false);
            timeSync.SetField("_playbackLoopIndex", 0);
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
        private ILevelFinisher _levelFinisher;
        private bool _disposedValue;
        private const float s_aheadTime = 1f;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public void Constractor(AudioManagerSO audioManagerSO, ILevelFinisher levelFinisher, SongTimeInfoEntity songTimeInfoEntity, BeatmapObjectSpawnController beatmapObjectSpawnController, GameplayCoreSceneSetupData gameplayCoreSceneSetupData, AudioTimeSyncController audioTimeSyncController, SongSeekBeatmapHandler songSeekBeatmapHandler, LooperUI looperUI, PracticeUI practiceUI, SongSeeker songSeeker)
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
            this._levelFinisher = levelFinisher;
            if (this._songTimeInfoEntity.LastLevelID != this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID
                && !string.IsNullOrEmpty(this._songTimeInfoEntity.LastLevelID)) {
                this._songTimeInfoEntity.PlayingNewSong = true;
                this._songTimeInfoEntity.LastLevelID = this._gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            }
            else {
                this._songTimeInfoEntity.PlayingNewSong = false;
            }

            this._levelFinisher.StandardLevelFinished += this.LevelFinisher_StandardLevelFinished;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this._levelFinisher.StandardLevelFinished -= this.LevelFinisher_StandardLevelFinished;
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
