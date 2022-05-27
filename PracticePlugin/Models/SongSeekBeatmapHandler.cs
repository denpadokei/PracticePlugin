using HarmonyLib;
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
        private readonly MethodInfo _noteControllerDespawn = AccessTools.Method(typeof(BasicBeatmapObjectManager), "Despawn", new Type[] { typeof(NoteController) });

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
            var cutSoundPoolContainer = this._noteCutSoundEffectManager.GetField<MemoryPoolContainer<NoteCutSoundEffect>, NoteCutSoundEffectManager>("_noteCutSoundEffectPoolContainer");
            while (basicGameNotePoolContainer.activeItems.Any()) {
                var item = basicGameNotePoolContainer.activeItems.First();
                this._noteControllerDespawn?.Invoke(this._beatmapObjectManager, new[] { item });
            }
            while (burstSliderHeadGameNotePoolContainer.activeItems.Any()) {
                var item = burstSliderHeadGameNotePoolContainer.activeItems.First();
                this._noteControllerDespawn?.Invoke(this._beatmapObjectManager, new[] { item });
            }
            while (burstSliderGameNotePoolContainer.activeItems.Any()) {
                var item = burstSliderGameNotePoolContainer.activeItems.First();
                this._noteControllerDespawn?.Invoke(this._beatmapObjectManager, new[] { item });
            }
            while (burstSliderFillPoolContainer.activeItems.Any()) {
                var item = burstSliderFillPoolContainer.activeItems.First();
                this._noteControllerDespawn?.Invoke(this._beatmapObjectManager, new[] { item });
            }
            while (cutSoundPoolContainer.activeItems.Any()) {
                var item = cutSoundPoolContainer.activeItems.First();
                item.StopPlayingAndFinish();
                this._noteCutSoundEffectManager.HandleNoteCutSoundEffectDidFinish(item);
            }
            foreach (var mang in Resources.FindObjectsOfTypeAll<SliderInteractionManager>()) {
                var activeSlider = mang.GetField<List<SliderController>, SliderInteractionManager>("_activeSliders");
                while (activeSlider.Any()) {
                    mang.RemoveActiveSlider(activeSlider.First());
                }
                foreach (var item in mang.GetComponentsInChildren<SliderHapticFeedbackInteractionEffect>()) {
                    item.enabled = false;
                }
            }
        }
    }
}