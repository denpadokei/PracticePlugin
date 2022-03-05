using IPA.Utilities;
using System.Collections.Generic;
using Zenject;

namespace PracticePlugin.Models
{
    public class SongSeekBeatmapHandler
    {

        [Inject]
        public SongSeekBeatmapHandler(
            AudioTimeSyncController audioTimeSyncController,
            BeatmapObjectCallbackController beatmapObjectCallbackController,
            NoteCutSoundEffectManager noteCutSoundEffectManager,
            BasicBeatmapObjectManager beatmapObjectManager)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._beatmapObjectCallbackController = beatmapObjectCallbackController;
            this._noteCutSoundEffectManager = noteCutSoundEffectManager;
            this._beatmapObjectManager = beatmapObjectManager;
            this._callbackList = this._beatmapObjectCallbackController
                        .GetField<List<BeatmapObjectCallbackData>, BeatmapObjectCallbackController>(
                        "_beatmapObjectCallbackData");
            this._beatmapData = this._beatmapObjectCallbackController
                .GetField<IReadonlyBeatmapData, BeatmapObjectCallbackController>("_beatmapData");
        }

        private List<BeatmapObjectCallbackData> CallbackList => this._callbackList;

        private readonly List<BeatmapObjectCallbackData> _callbackList;
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;
        private readonly BasicBeatmapObjectManager _beatmapObjectManager;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private IReadonlyBeatmapData _beatmapData;

        public void OnSongTimeChanged(float newSongTime, float aheadTime)
        {
            Logger.Debug("OnSongTimeChanged");
            if (this._beatmapObjectCallbackController) {
                this._beatmapData = this._beatmapObjectCallbackController.GetField<IReadonlyBeatmapData, BeatmapObjectCallbackController>("_beatmapData");
            }

            foreach (var callbackData in this.CallbackList) {
                for (var i = 0; i < this._beatmapData.beatmapLinesData.Count; i++) {
                    callbackData.nextObjectIndexInLine[i] = 0;
                    while (callbackData.nextObjectIndexInLine[i] < this._beatmapData.beatmapLinesData[i].beatmapObjectsData.Count) {
                        var beatmapObjectData = this._beatmapData.beatmapLinesData[i].beatmapObjectsData[callbackData.nextObjectIndexInLine[i]];
                        if (beatmapObjectData.time - aheadTime >= newSongTime) {
                            break;
                        }

                        callbackData.nextObjectIndexInLine[i]++;
                    }
                }
            }

            var newNextEventIndex = 0;

            while (newNextEventIndex < this._beatmapData.beatmapEventsData.Count) {
                var beatmapEventData = this._beatmapData.beatmapEventsData[newNextEventIndex];
                if (beatmapEventData.time >= newSongTime) {
                    break;
                }

                newNextEventIndex++;
            }

            this._beatmapObjectCallbackController.SetField("_nextEventIndex", newNextEventIndex);
            var notes = this._beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>, BasicBeatmapObjectManager>("_gameNotePoolContainer");
            var bombs = this._beatmapObjectManager.GetField<MemoryPoolContainer<BombNoteController>, BasicBeatmapObjectManager>("_bombNotePoolContainer");
            var walls = this._beatmapObjectManager.GetField<MemoryPoolContainer<ObstacleController>, BasicBeatmapObjectManager>("_obstaclePoolContainer");
            foreach (var note in notes.activeItems) {
                if (note == null) {
                    continue;
                }

                note.hide = false;
                note.pause = false;
                note.enabled = true;
                note.gameObject.SetActive(true);
                note.Dissolve(0f);
            }
            foreach (var bomb in bombs.activeItems) {
                if (bomb == null) {
                    continue;
                }

                bomb.hide = false;
                bomb.pause = false;
                bomb.enabled = true;
                bomb.gameObject.SetActive(true);
                bomb.Dissolve(0f);
            }
            foreach (var wall in walls.activeItems) {
                if (wall == null) {
                    continue;
                }

                wall.hide = false;
                wall.pause = false;
                wall.enabled = true;
                wall.gameObject.SetActive(true);
                wall.Dissolve(0f);
            }
            this._audioTimeSyncController.SetField("_prevAudioSamplePos", -1);
            this._audioTimeSyncController.SetField("_songTime", newSongTime);
            this._noteCutSoundEffectManager.SetField("_prevNoteATime", -1f);
            this._noteCutSoundEffectManager.SetField("_prevNoteBTime", -1f);
        }
    }
}