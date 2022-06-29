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
            DiContainer di)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._beatmapCallbacksController = beatmapCallbacksController;
            this._noteCutSoundEffectManager = noteCutSoundEffectManager;
            this._beatmapObjectManager = beatmapObjectManager;
            var callBackManager = Type.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager, NoodleExtensions");
            if (callBackManager != null) {
                this._noodleObjectsCallbacksManager = di.TryResolve(callBackManager);
            }
        }
        private readonly BeatmapCallbacksController _beatmapCallbacksController;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly BasicBeatmapObjectManager _beatmapObjectManager;
        private readonly object _noodleObjectsCallbacksManager;
        private static readonly Type s_customNotesControllerInfo = null;
        private static readonly MethodInfo s_handleNoteControllerNoteWasMissed = null;

        static SongSeekBeatmapHandler()
        {
            var info = PluginManager.GetPlugin("CustomNotes");
            if (info != null) {
                s_customNotesControllerInfo = Type.GetType("CustomNotes.Managers.CustomNoteController, CustomNotes");
                s_handleNoteControllerNoteWasMissed = s_customNotesControllerInfo.GetMethod("HandleNoteControllerNoteWasMissed", BindingFlags.Instance | BindingFlags.Public);
            }
        }

        public void OnSongTimeChanged(float newSongTime, float aheadTime)
        {
            this._audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            this._audioTimeSyncController.SetField("_songTime", newSongTime);
            this._noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            this._noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
            this._beatmapCallbacksController.SetField("_startFilterTime", newSongTime + aheadTime);
            this._beatmapCallbacksController.SetField("_prevSongTime", newSongTime);
            var dic = this._beatmapCallbacksController.GetField<Dictionary<float, CallbacksInTime>, BeatmapCallbacksController>("_callbacksInTimes");
            foreach (var item in dic.Values) {
                item.lastProcessedNode = null;
            }
            if (this._noodleObjectsCallbacksManager != null) {
                var noodleObjectsCallbacksManagerStartFilerSongTime = AccessTools.Field(Type.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager, NoodleExtensions"), "_startFilterTime");
                noodleObjectsCallbacksManagerStartFilerSongTime.SetValue(this._noodleObjectsCallbacksManager, newSongTime + aheadTime);
                var noodleObjectsCallbacksManagerPrevSongTime = AccessTools.Field(Type.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager, NoodleExtensions"), "_prevSongtime");
                noodleObjectsCallbacksManagerPrevSongTime.SetValue(this._noodleObjectsCallbacksManager, newSongTime);
                var noodleObjectsCallbacksManagerCallbacksIntime = AccessTools.Field(Type.GetType("NoodleExtensions.Managers.NoodleObjectsCallbacksManager, NoodleExtensions"), "_callbacksInTime");
                if (noodleObjectsCallbacksManagerCallbacksIntime.GetValue(this._noodleObjectsCallbacksManager) is CallbacksInTime callbacks) {
                    callbacks.lastProcessedNode = null;
                }
            }
            // Thank you Kyle 1413!
            var basicGameNotePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>, BasicBeatmapObjectManager>("_basicGameNotePoolContainer");
            var burstSliderHeadGameNotePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>, BasicBeatmapObjectManager>("_burstSliderHeadGameNotePoolContainer");
            var burstSliderGameNotePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<BurstSliderGameNoteController>, BasicBeatmapObjectManager>("_burstSliderGameNotePoolContainer");
            var burstSliderFillPoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<BurstSliderGameNoteController>, BasicBeatmapObjectManager>("_burstSliderFillPoolContainer");
            var obstaclePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<ObstacleController>, BasicBeatmapObjectManager>("_obstaclePoolContainer");
            var cutSoundPoolContainer = this._noteCutSoundEffectManager.GetField<MemoryPoolContainer<NoteCutSoundEffect>, NoteCutSoundEffectManager>("_noteCutSoundEffectPoolContainer");

            while (basicGameNotePoolContainer.activeItems.Any()) {
                var item = basicGameNotePoolContainer.activeItems.First();
                this.RaiseCustomNoteMissEvent(item);
                var movement = item?.GetField<NoteMovement, NoteController>("_noteMovement");
                if (movement?.movementPhase == NoteMovement.MovementPhase.MovingOnTheFloor) {
                    movement?.HandleFloorMovementDidFinish();
                }
                movement?.HandleNoteJumpDidFinish();
                item.gameObject.SetActive(false);
            }
            while (burstSliderHeadGameNotePoolContainer.activeItems.Any()) {
                var item = burstSliderHeadGameNotePoolContainer.activeItems.First();
                this.RaiseCustomNoteMissEvent(item);
                var movement = item?.GetField<NoteMovement, NoteController>("_noteMovement");
                if (movement?.movementPhase == NoteMovement.MovementPhase.MovingOnTheFloor) {
                    movement?.HandleFloorMovementDidFinish();
                }
                movement?.HandleNoteJumpDidFinish();
                item.gameObject.SetActive(false);
            }
            while (burstSliderGameNotePoolContainer.activeItems.Any()) {
                var item = burstSliderGameNotePoolContainer.activeItems.First();
                this.RaiseCustomNoteMissEvent(item);
                var movement = item?.GetField<NoteMovement, NoteController>("_noteMovement");
                if (movement?.movementPhase == NoteMovement.MovementPhase.MovingOnTheFloor) {
                    movement?.HandleFloorMovementDidFinish();
                }
                movement?.HandleNoteJumpDidFinish();
                item.gameObject.SetActive(false);
            }
            while (burstSliderFillPoolContainer.activeItems.Any()) {
                var item = burstSliderFillPoolContainer.activeItems.First();
                this.RaiseCustomNoteMissEvent(item);
                var movement = item?.GetField<NoteMovement, NoteController>("_noteMovement");
                if (movement?.movementPhase == NoteMovement.MovementPhase.MovingOnTheFloor) {
                    movement?.HandleFloorMovementDidFinish();
                }
                movement?.HandleNoteJumpDidFinish();
                item?.gameObject.SetActive(false);
            }
            while (cutSoundPoolContainer.activeItems.Any()) {
                var item = cutSoundPoolContainer.activeItems.First();
                item?.StopPlayingAndFinish();
            }
            foreach (var item in obstaclePoolContainer.activeItems) {
                item?.SetField("_finishMovementTime", -1f);
                item?.ManualUpdate();
            }
            foreach (var mang in Resources.FindObjectsOfTypeAll<SliderInteractionManager>()) {
                var activeSlider = mang.GetField<List<SliderController>, SliderInteractionManager>("_activeSliders");
                while (activeSlider.Any()) {
                    mang?.RemoveActiveSlider(activeSlider?.First());
                }
                foreach (var item in mang?.GetComponentsInChildren<SliderHapticFeedbackInteractionEffect>()) {
                    item.enabled = false;
                }
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
            this._audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            this._audioTimeSyncController.SetField("_startSongTime", newSongTime);
            var initData = this._audioTimeSyncController.GetField<AudioTimeSyncController.InitData, AudioTimeSyncController>("_initData");
            initData.SetField("startSongTime", newSongTime);
            this._audioTimeSyncController.SetField("_initData", initData);
            this._noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            this._noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
            this._beatmapCallbacksController.SetField("_startFilterTime", newSongTime + 1f);
            this._beatmapCallbacksController.SetField("_prevSongTime", newSongTime);
        }
    }
}