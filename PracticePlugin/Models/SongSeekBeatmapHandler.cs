using HarmonyLib;
using IPA.Loader;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Models
{
    public class SongSeekBeatmapHandler
    {

        [Inject]
        public SongSeekBeatmapHandler(
            AudioTimeSyncController audioTimeSyncController,
            BeatmapCallbacksController beatmapCallbacksController,
            NoteCutSoundEffectManager noteCutSoundEffectManager,
            BasicBeatmapObjectManager beatmapObjectManager,
            IReadonlyBeatmapData beatmapData,
            GameplayModifiers gameplayModifiers,
            IGameEnergyCounter gameEnergyCounter,
            DiContainer di)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._beatmapCallbacksController = beatmapCallbacksController;
            this._noteCutSoundEffectManager = noteCutSoundEffectManager;
            this._beatmapObjectManager = beatmapObjectManager;
            this._gameEnergyCounter = gameEnergyCounter;
            this._diContainer = di;
            this._gameEnergyCounter.gameEnergyDidReach0Event += this.OnGameEnergyCounter_gameEnergyDidReach0Event;
            this._noFailOn0Energy = gameplayModifiers.noFailOn0Energy;
            this._failed = false;
            var noodleManagerType = Type.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager, NoodleExtensions");
            if (noodleManagerType != null) {
                this._noodleManagerType = noodleManagerType;
                this._noodleStartFilterTimeField = AccessTools.Field(noodleManagerType, "_startFilterTime");
                this._noodlePrevSongTimeField = AccessTools.Field(noodleManagerType, "_prevSongtime");
                this._noodleCallbacksInTimeField = AccessTools.Field(noodleManagerType, "_callbacksInTime");
                if (this._noodleStartFilterTimeField == null || this._noodlePrevSongTimeField == null || this._noodleCallbacksInTimeField == null) {
                    Logger.Info("Noodle Extensions callback manager fields were not found; the seek/loop reset will not apply to Noodle objects.");
                    this._noodleManagerType = null;
                }
            }
            Logger.Debug($"[SongSeekBeatmapHandler] Noodle callback manager type found: {this._noodleManagerType != null}");
        }

        private void OnGameEnergyCounter_gameEnergyDidReach0Event()
        {
            if (!this._noFailOn0Energy) {
                _failed = true;
            }
            this._gameEnergyCounter.gameEnergyDidReach0Event -= OnGameEnergyCounter_gameEnergyDidReach0Event;
        }

        private readonly BeatmapCallbacksController _beatmapCallbacksController;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly BasicBeatmapObjectManager _beatmapObjectManager;
        private readonly DiContainer _diContainer;
        private readonly Type _noodleManagerType;
        private readonly FieldInfo _noodleStartFilterTimeField;
        private readonly FieldInfo _noodlePrevSongTimeField;
        private readonly FieldInfo _noodleCallbacksInTimeField;
        private object _noodleObjectsCallbacksManager;
        private static readonly Type s_customNotesControllerInfo = null;
        private static readonly MethodInfo s_handleNoteControllerNoteWasMissed = null;
        private static readonly float s_minAheadTime = 1f;
        private SliderInteractionManager[] _sliderInteractionManager = null;
        private readonly IGameEnergyCounter _gameEnergyCounter;
        private bool _noFailOn0Energy;
        private bool _failed;

        static SongSeekBeatmapHandler()
        {
            var info = PluginManager.GetPlugin("CustomNotes");
            if (info != null) {
                s_customNotesControllerInfo = Type.GetType("CustomNotes.Components.CustomNoteController, CustomNotes");
                s_handleNoteControllerNoteWasMissed = s_customNotesControllerInfo.GetMethod("HandleNoteControllerNoteWasMissed", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        public void OnSongTimeChanged(float newSongTime)
        {
            if (this._failed) {
                return;
            }

            var samplePos = newSongTime / this._audioTimeSyncController.songEndTime;
            var audioSource = this._audioTimeSyncController._audioSource;
            audioSource.timeSamples = Mathf.RoundToInt(Mathf.Lerp(0, audioSource.clip.samples, samplePos));
            var aheadTime = Mathf.Min(newSongTime, s_minAheadTime);
            audioSource.time -= aheadTime;
            this._audioTimeSyncController._prevAudioSamplePos = -1;
            this._audioTimeSyncController._songTime = newSongTime;
            this._noteCutSoundEffectManager._prevNoteATime = -1f;
            this._noteCutSoundEffectManager._prevNoteBTime = -1f;
            this._beatmapCallbacksController.SetField("_startFilterTime", newSongTime + aheadTime);
            this._beatmapCallbacksController._prevSongTime = newSongTime;
            var dic = this._beatmapCallbacksController._callbacksInTimes;
            foreach (var item in dic.Values) {
                item.lastProcessedNode = null;
            }
            if (this._noodleManagerType != null) {
                var noodleManager = this.GetNoodleManager();
                if (noodleManager != null) {
                    // Noodle keeps its own copy of the filter time / progress, so it has to be
                    // reset the same way the base controller is, otherwise Noodle objects are
                    // spawned from the old position.
                    this._noodleStartFilterTimeField.SetValue(noodleManager, newSongTime + aheadTime);
                    this._noodlePrevSongTimeField.SetValue(noodleManager, newSongTime);
                    if (this._noodleCallbacksInTimeField.GetValue(noodleManager) is CallbacksInTime callbacks) {
                        callbacks.lastProcessedNode = null;
                    }
                }
            }
            // Thank you Kyle 1413!
            var basicGameNotePoolContainer = this._beatmapObjectManager._basicGameNotePoolContainer;
            var burstSliderHeadGameNotePoolContainer = this._beatmapObjectManager._burstSliderHeadGameNotePoolContainer;
            var burstSliderGameNotePoolContainer = this._beatmapObjectManager._burstSliderGameNotePoolContainer;
            var obstaclePoolContainer = this._beatmapObjectManager._obstaclePoolContainer;
            var cutSoundPoolContainer = this._noteCutSoundEffectManager._noteCutSoundEffectPoolContainer;
            Logger.Debug($"[OnSongTimeChanged] active objects before despawn: notes={basicGameNotePoolContainer?.activeItems?.Count}, burstHead={burstSliderHeadGameNotePoolContainer?.activeItems?.Count}, burst={burstSliderGameNotePoolContainer?.activeItems?.Count}, obstacles={obstaclePoolContainer?.activeItems?.Count}");
            this.DespawnNotes(basicGameNotePoolContainer);
            this.DespawnNotes(burstSliderHeadGameNotePoolContainer);
            this.DespawnNotes(burstSliderGameNotePoolContainer);
            Logger.Debug($"[OnSongTimeChanged] active objects after despawn: notes={basicGameNotePoolContainer?.activeItems?.Count}, burstHead={burstSliderHeadGameNotePoolContainer?.activeItems?.Count}, burst={burstSliderGameNotePoolContainer?.activeItems?.Count}");
            foreach (var item in obstaclePoolContainer.activeItems) {
                if (item == null) {
                    continue;
                }
                item._finishMovementTime = -1f;
                item.ManualUpdate();
            }
            while (cutSoundPoolContainer.activeItems.Any()) {
                var item = cutSoundPoolContainer.activeItems.First();
                item?.StopPlayingAndFinish();
            }
            if (this._sliderInteractionManager == null) {
                this._sliderInteractionManager = Resources.FindObjectsOfTypeAll<SliderInteractionManager>();
            }
            foreach (var mang in this._sliderInteractionManager) {
                var activeSlider = mang._activeSliders;
                while (activeSlider.Any()) {
                    mang?.RemoveActiveSlider(activeSlider?.First());
                }
                foreach (var item in mang?.GetComponentsInChildren<SliderHapticFeedbackInteractionEffect>()) {
                    item.enabled = false;
                }
            }
        }

        /// <summary>
        /// Resolves the Noodle Extensions callback manager lazily, because it is only bound
        /// for maps that actually use Noodle data.
        /// </summary>
        private object GetNoodleManager()
        {
            if (this._noodleObjectsCallbacksManager == null && this._noodleManagerType != null && this._diContainer != null) {
                this._noodleObjectsCallbacksManager = this._diContainer.TryResolve(this._noodleManagerType);
            }
            return this._noodleObjectsCallbacksManager;
        }

        /// <summary>
        /// Deapawn notes
        /// </summary>
        /// <typeparam name="T"><see cref="NoteController"/></typeparam>
        /// <param name="memoryPoolContainer"></param>
        private void DespawnNotes<T>(MemoryPoolContainer<T> memoryPoolContainer) where T : NoteController
        {
            if (memoryPoolContainer == null) {
                return;
            }
            while (memoryPoolContainer.activeItems.Any()) {
                var item = memoryPoolContainer.activeItems.First();
                this.RaiseCustomNoteMissEvent(item);
                var movement = item?._noteMovement;
#if false
                if (movement?.movementPhase == NoteMovement.MovementPhase.MovingOnTheFloor) {
                    movement?.HandleFloorMovementDidFinish();
                }
#endif
                movement?.HandleNoteJumpDidFinish();
            }
        }

        private void RaiseCustomNoteMissEvent(NoteController nc)
        {
            if (s_customNotesControllerInfo == null) {
                return;
            }
            var customNote = nc.gameObject.GetComponentInChildren(s_customNotesControllerInfo);
            if (customNote != null) {
                s_handleNoteControllerNoteWasMissed.Invoke(customNote, new object[] { nc });
            }
        }

        public void ChangeSongStartTime(float newSongTime)
        {
            this._audioTimeSyncController._prevAudioSamplePos = -1;
            this._audioTimeSyncController._startSongTime = newSongTime;
            var initData = this._audioTimeSyncController._initData;
            initData.SetField("startSongTime", newSongTime);
            this._audioTimeSyncController.SetField("_initData", initData);
            this._noteCutSoundEffectManager._prevNoteATime = -1f;
            this._noteCutSoundEffectManager._prevNoteBTime = -1f;
            this._beatmapCallbacksController.SetField("_startFilterTime", newSongTime + 1f);
            this._beatmapCallbacksController._prevSongTime = newSongTime;
        }
    }
}