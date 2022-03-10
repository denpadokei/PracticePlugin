using IPA.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            IReadonlyBeatmapData beatmapData,
            BasicBeatmapObjectManager beatmapObjectManager)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._beatmapCallbacksController = beatmapCallbacksController;
            this._noteCutSoundEffectManager = noteCutSoundEffectManager;
            this._beatmapObjectManager = beatmapObjectManager;
        }
        private readonly BeatmapCallbacksController _beatmapCallbacksController;
        private readonly BasicBeatmapObjectManager _beatmapObjectManager;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;

        public void OnSongTimeChanged(float newSongTime, float aheadTime)
        {
            Logger.Debug("OnSongTimeChanged");
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
            var basicGameNotePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>, BasicBeatmapObjectManager>("_basicGameNotePoolContainer");
            var burstSliderHeadGameNotePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>, BasicBeatmapObjectManager>("_burstSliderHeadGameNotePoolContainer");
            var burstSliderGameNotePoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<BurstSliderGameNoteController>, BasicBeatmapObjectManager>("_burstSliderGameNotePoolContainer");
            var burstSliderFillPoolContainer = this._beatmapObjectManager.GetField<MemoryPoolContainer<BurstSliderGameNoteController>, BasicBeatmapObjectManager>("_burstSliderFillPoolContainer");
            var bombs = this._beatmapObjectManager.GetField<MemoryPoolContainer<BombNoteController>, BasicBeatmapObjectManager>("_bombNotePoolContainer");
            var walls = this._beatmapObjectManager.GetField<MemoryPoolContainer<ObstacleController>, BasicBeatmapObjectManager>("_obstaclePoolContainer");
            
            while (basicGameNotePoolContainer.activeItems.Any()) {
                var item = basicGameNotePoolContainer.activeItems.First();
                item.Hide(true);
                basicGameNotePoolContainer.Despawn(item);
            }
            while (burstSliderHeadGameNotePoolContainer.activeItems.Any()) {
                var item = burstSliderHeadGameNotePoolContainer.activeItems.First();
                item.Hide(true);
                burstSliderHeadGameNotePoolContainer.Despawn(item);
            }
            while (burstSliderGameNotePoolContainer.activeItems.Any()) {
                var item = burstSliderGameNotePoolContainer.activeItems.First();
                item.Hide(true);
                burstSliderGameNotePoolContainer.Despawn(item);
            }
            while (burstSliderFillPoolContainer.activeItems.Any()) {
                var item = burstSliderFillPoolContainer.activeItems.First();
                item.Hide(true);
                burstSliderFillPoolContainer.Despawn(item);
            }

            while (bombs.activeItems.Any()) {
                var item = bombs.activeItems.First();
                item.Hide(true);
                bombs.Despawn(item);
            }
            while (walls.activeItems.Any()) {
                var item = walls.activeItems.First();
                item.Hide(true);
                walls.Despawn(item);
            }
        }
    }
}