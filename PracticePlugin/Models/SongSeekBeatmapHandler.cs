using BS_Utils.Utilities;
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
            //this._beatmapObjectSpawnController = beatmapObjectSpawnController;
            //this._beatmapObjectExecutionRatingsRecorder = beatmapObjectExecutionRatingsRecorder;
            this._noteCutSoundEffectManager = noteCutSoundEffectManager;
            this._beatmapObjectManager = beatmapObjectManager;
            this._callbackList = this._beatmapObjectCallbackController
                        .GetPrivateField<List<BeatmapObjectCallbackData>>(
                        "_beatmapObjectCallbackData");
            this._beatmapData = this._beatmapObjectCallbackController
                .GetPrivateField<BeatmapData>("_beatmapData");
            //this._notePool = this._beatmapObjectManager.GetPrivateField<MemoryPoolContainer<GameNoteController>>("_gameNotePoolContainer");
            //this._bombNotePool = this._beatmapObjectManager.GetPrivateField<MemoryPoolContainer<BombNoteController>>("_bombNotePoolContainer");
            //this._obstaclePool = this._beatmapObjectManager.GetPrivateField<MemoryPoolContainer<ObstacleController>>("_obstaclePoolContainer");
        }

        private List<BeatmapObjectCallbackData> CallbackList => this._callbackList;

        private readonly List<BeatmapObjectCallbackData> _callbackList;
        private readonly BeatmapObjectCallbackController _beatmapObjectCallbackController;
        //private readonly BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private readonly BasicBeatmapObjectManager _beatmapObjectManager;
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        //private readonly BeatmapObjectExecutionRatingsRecorder _beatmapObjectExecutionRatingsRecorder;

        //private readonly MemoryPoolContainer<GameNoteController> _notePool;
        //private readonly MemoryPoolContainer<BombNoteController> _bombNotePool;
        //private readonly MemoryPoolContainer<ObstacleController> _obstaclePool;

        private BeatmapData _beatmapData;

        public void OnSongTimeChanged(float newSongTime, float aheadTime)
        {
            Logger.Debug("OnSongTimeChanged");
            if (this._beatmapObjectCallbackController) {
                this._beatmapData = this._beatmapObjectCallbackController.GetPrivateField<BeatmapData>("_beatmapData");
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

            this._beatmapObjectCallbackController.SetPrivateField("_nextEventIndex", newNextEventIndex);
            //  _beatmapObjectManager.DissolveAllObjects();
            var notes = this._beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>>("_gameNotePoolContainer");
            var bombs = this._beatmapObjectManager.GetField<MemoryPoolContainer<BombNoteController>>("_bombNotePoolContainer");
            var walls = this._beatmapObjectManager.GetField<MemoryPoolContainer<ObstacleController>>("_obstaclePoolContainer");
            foreach (var note in notes.activeItems) {
                if (note == null) {
                    continue;
                }

                note.hide = false;
                note.pause = false;
                note.enabled = true;
                note.gameObject.SetActive(true);
                note.Dissolve(0f);
                //    _beatmapObjectManager.InvokeMethod<BeatmapObjectManager>("Despawn", note as NoteController);
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
                //    _beatmapObjectManager.InvokeMethod<BeatmapObjectManager>("Despawn", bomb as NoteController);
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
                //_beatmapObjectManager.InvokeMethod<BeatmapObjectManager>("Despawn", wall);
            }
            /*
            var notesA = _notePool.activeItems.ToList();
            foreach (var noteA in notesA)
            {
                //               Logger.Debug("Despawning, Length: " + notesA.Count);
                _beatmapObjectManager.DissolveAllObjects(noteA);
            }
            
            var notesB = _noteBPool.activeItems.ToList();
            foreach (var noteB in notesB)
            {
                _beatmapObjectManager.Despawn(noteB);
            }

            var bombs = _bombNotePool.activeItems.ToList();
            foreach (var bomb in bombs)
            {
                _beatmapObjectManager.Despawn(bomb);
            }

            var obstacles = _obstaclePool.activeItems.ToList();
            foreach (var obstacle in obstacles)
            {
                _beatmapObjectManager.Despawn(obstacle);
            }
            */

            this._audioTimeSyncController.SetPrivateField("_prevAudioSamplePos", -1);
            this._audioTimeSyncController.SetPrivateField("_songTime", newSongTime);
            this._noteCutSoundEffectManager.SetPrivateField("_prevNoteATime", -1);
            this._noteCutSoundEffectManager.SetPrivateField("_prevNoteBTime", -1);
        }
    }
}