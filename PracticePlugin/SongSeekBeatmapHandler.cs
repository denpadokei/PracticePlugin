using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace PracticePlugin
{
    public class SongSeekBeatmapHandler
    {
        
        [Inject]
        public SongSeekBeatmapHandler(
            AudioTimeSyncController audioTimeSyncController,
            BeatmapObjectCallbackController beatmapObjectCallbackController,
            BeatmapObjectSpawnController beatmapObjectSpawnController,
            BeatmapObjectExecutionRatingsRecorder beatmapObjectExecutionRatingsRecorder,
            NoteCutSoundEffectManager noteCutSoundEffectManager,
            BasicBeatmapObjectManager beatmapObjectManager)
        {
            this._audioTimeSyncController = audioTimeSyncController;
            this._beatmapObjectCallbackController = beatmapObjectCallbackController;
            this._beatmapObjectSpawnController = beatmapObjectSpawnController;
            this._beatmapObjectExecutionRatingsRecorder = beatmapObjectExecutionRatingsRecorder;
            this._noteCutSoundEffectManager = noteCutSoundEffectManager;
            this._beatmapObjectManager = beatmapObjectManager;
            _callbackList = _beatmapObjectCallbackController
                        .GetPrivateField<List<BeatmapObjectCallbackData>>(
                        "_beatmapObjectCallbackData");
            _beatmapData = _beatmapObjectCallbackController
                .GetPrivateField<BeatmapData>("_beatmapData");
            _notePool = _beatmapObjectManager.GetPrivateField<MemoryPoolContainer<GameNoteController>>("_gameNotePoolContainer");
            _bombNotePool = _beatmapObjectManager.GetPrivateField<MemoryPoolContainer<BombNoteController>>("_bombNotePoolContainer");
            _obstaclePool = _beatmapObjectManager.GetPrivateField<MemoryPoolContainer<ObstacleController>>("_obstaclePoolContainer");
        }

        private List<BeatmapObjectCallbackData> CallbackList
        {
            get
            {
                return _callbackList;
            }
        }

        private List<BeatmapObjectCallbackData> _callbackList;
        private BeatmapObjectCallbackController _beatmapObjectCallbackController;
        private BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private BasicBeatmapObjectManager _beatmapObjectManager;
        private NoteCutSoundEffectManager _noteCutSoundEffectManager;
        AudioTimeSyncController _audioTimeSyncController;
        BeatmapObjectExecutionRatingsRecorder _beatmapObjectExecutionRatingsRecorder;

        private MemoryPoolContainer<GameNoteController> _notePool;
        private MemoryPoolContainer<BombNoteController> _bombNotePool;
        private MemoryPoolContainer<ObstacleController> _obstaclePool;

        private BeatmapData _beatmapData;

        public void OnSongTimeChanged(float newSongTime, float aheadTime)
        {
            if (_beatmapObjectCallbackController)
                _beatmapData = _beatmapObjectCallbackController.GetPrivateField<BeatmapData>("_beatmapData");
            foreach (var callbackData in CallbackList)
            {
                for (var i = 0; i < _beatmapData.beatmapLinesData.Count; i++)
                {
                    callbackData.nextObjectIndexInLine[i] = 0;
                    while (callbackData.nextObjectIndexInLine[i] < _beatmapData.beatmapLinesData[i].beatmapObjectsData.Count)
                    {
                        var beatmapObjectData = _beatmapData.beatmapLinesData[i].beatmapObjectsData[callbackData.nextObjectIndexInLine[i]];
                        if (beatmapObjectData.time - aheadTime >= newSongTime)
                        {
                            break;
                        }

                        callbackData.nextObjectIndexInLine[i]++;
                    }
                }
            }

            var newNextEventIndex = 0;

            while (newNextEventIndex < _beatmapData.beatmapEventsData.Count)
            {
                var beatmapEventData = _beatmapData.beatmapEventsData[newNextEventIndex];
                if (beatmapEventData.time >= newSongTime)
                {
                    break;
                }

                newNextEventIndex++;
            }

            _beatmapObjectCallbackController.SetPrivateField("_nextEventIndex", newNextEventIndex);
            //  _beatmapObjectManager.DissolveAllObjects();
            var notes = _beatmapObjectManager.GetField<MemoryPoolContainer<GameNoteController>>("_gameNotePoolContainer");
            var bombs = _beatmapObjectManager.GetField<MemoryPoolContainer<BombNoteController>>("_bombNotePoolContainer");
            var walls = _beatmapObjectManager.GetField<MemoryPoolContainer<ObstacleController>>("_obstaclePoolContainer");
            foreach (var note in notes.activeItems)
            {
                if (note == null) continue;
                note.hide = false;
                note.pause = false;
                note.enabled = true;
                note.gameObject.SetActive(true);
                note.Dissolve(0f);
            //    _beatmapObjectManager.InvokeMethod<BeatmapObjectManager>("Despawn", note as NoteController);
            }
            foreach (var bomb in bombs.activeItems)
            {
                if (bomb == null) continue;
                bomb.hide = false;
                bomb.pause = false;
                bomb.enabled = true;
                bomb.gameObject.SetActive(true);
                bomb.Dissolve(0f);
                //    _beatmapObjectManager.InvokeMethod<BeatmapObjectManager>("Despawn", bomb as NoteController);
            }
            foreach (var wall in walls.activeItems)
            {
                if (wall == null) continue;
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
                //               Console.WriteLine("Despawning, Length: " + notesA.Count);
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

            _audioTimeSyncController.SetPrivateField("_prevAudioSamplePos", -1);
            _audioTimeSyncController.SetPrivateField("_songTime", newSongTime);
            _noteCutSoundEffectManager.SetPrivateField("_prevNoteATime", -1);
            _noteCutSoundEffectManager.SetPrivateField("_prevNoteBTime", -1);
        }
    }
}