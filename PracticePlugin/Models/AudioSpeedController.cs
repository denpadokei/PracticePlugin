using BS_Utils.Utilities;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Models
{
    public class AudioSpeedController : MonoBehaviour, IInitializable, IDisposable
    {
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プロパティ
        public string failTime { get; private set; }
        internal bool showFailTextNext { get; set; }
        //       public static GameObject SpeedSettingsObject { get; private set; }
        //       public static GameObject NjsSettingsObject { get; private set; }
        //       public static GameObject SpawnOffsetSettingsObject { get; private set; }
        internal bool startWithFullEnergy = false;
        internal bool disablePitchCorrection = false;
        internal bool adjustNJSWithSpeed = false;
        internal float TimeScale
        {
            get => _timeScale;
            set => this.SetTimeScale(ref _timeScale, value);
        }

        private static float _timeScale = 1;

        public static bool PracticeMode { get; private set; }

        public static bool PlayingNewSong { get; private set; }
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
            //var levelData = BS_Utils.Plugin.LevelData;
            //if (levelData.IsSet == false) return;
            BSEvents.levelFailed += this.BSEvents_levelFailed;

            if (_songTimeInfoEntity.LastLevelID != _gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID &&
                !string.IsNullOrEmpty(_songTimeInfoEntity.LastLevelID)) {
                PlayingNewSong = true;
                // TimeScale = 1;
                _songTimeInfoEntity.LastLevelID = _gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            }
            else {
                PlayingNewSong = false;
            }


            _songTimeInfoEntity.LastLevelID = _gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;
            //_mixer = Resources.FindObjectsOfTypeAll<AudioManagerSO>().LastOrDefault();
            //AudioTimeSync = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            _songAudio = _audioTimeSyncController.GetPrivateField<AudioSource>("_audioSource");

            PracticeMode = this._gameplayCoreSceneSetupData.practiceSettings != null;


            if (!PracticeMode) {
                _timeScale = Mathf.Clamp(TimeScale, 1, SpeedMaxSize);
            }
            if (PracticeMode) {
                if (_gameplayCoreSceneSetupData.practiceSettings.songSpeedMul != 1f)
                    _timeScale = _gameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
                else
                    _timeScale = _gameplayCoreSceneSetupData.gameplayModifiers.songSpeedMul;
            }
        }

        public void Update()
        {
            if (_uiElementsCreator == null || _songSeeker == null) return;
            _songSeeker.OnUpdate();
        }

        private void BSEvents_levelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults results)
        {
            float endTime = results.endSongTime;
            float length = _audioTimeSyncController.songTime;
            _songTimeInfoEntity.FailTimeText = $"<#ff0000>Failed At</color> - {Math.Floor(endTime / 60):N0}:{Math.Floor(endTime % 60):00}  /  {Math.Floor(length / 60):N0}:{Math.Floor(length % 60):00}";
            this._songTimeInfoEntity.ShowFailTextNext = true;
        }
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // プライベートメソッド
        private void SetTimeScale(ref float field, float timeScale)
        {
            field = timeScale;
            if (!_audioTimeSyncController) return;
            if (!_spawnController) return;
            AudioTimeSyncController.InitData initData = this._audioTimeSyncController.GetPrivateField<AudioTimeSyncController.InitData>("_initData");
            AudioTimeSyncController.InitData newInitData = new AudioTimeSyncController.InitData(initData.audioClip,
                this._audioTimeSyncController.songTime, initData.songTimeOffset, field);
            this._audioTimeSyncController.SetPrivateField("_initData", newInitData);
            //Chipmunk Removal as per base game
            if (!disablePitchCorrection) {
                if (field == 1f)
                    _mixer.musicPitch = 1;
                else
                    _mixer.musicPitch = 1f / field;
            }
            else {
                _mixer.musicPitch = 1f;
            }
            ResetTimeSync(this._audioTimeSyncController, field, newInitData);
            //   AudioTimeSync.StartSong();
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
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // メンバ変数
        public const float SpeedMaxSize = 5.05f;
        public const float SpeedStepSize = 0.05f;

        public const int NjsMaxSize = 100;
        public const int NjstepSize = 1;
        private BeatmapObjectSpawnController _spawnController;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        public AudioTimeSyncController _audioTimeSyncController;
        private AudioManagerSO _mixer;
        private SongTimeInfoEntity _songTimeInfoEntity;
        private AudioSource _songAudio;
        private UIElementsCreator _uiElementsCreator;
        private SongSeeker _songSeeker;
        private bool _disposedValue;
        #endregion
        //ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*ﾟ+｡｡+ﾟ*｡+ﾟ ﾟ+｡*
        #region // 構築・破棄
        [Inject]
        public AudioSpeedController(SongTimeInfoEntity songTimeInfoEntity, BeatmapObjectSpawnController beatmapObjectSpawnController, GameplayCoreSceneSetupData gameplayCoreSceneSetupData, AudioTimeSyncController audioTimeSyncController, SongSeeker songSeeker, UIElementsCreator uIElementsCreator)
        {
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._spawnController = beatmapObjectSpawnController;
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._audioTimeSyncController = audioTimeSyncController;
            this._songSeeker = songSeeker;
            this._uiElementsCreator = uIElementsCreator;
            songTimeInfoEntity.PracticeMode = gameplayCoreSceneSetupData.practiceSettings != null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    BSEvents.levelFailed -= this.BSEvents_levelFailed;
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                _disposedValue = true;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
