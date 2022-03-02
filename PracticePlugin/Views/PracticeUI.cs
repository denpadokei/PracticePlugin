using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using PracticePlugin.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Views
{
    [HotReload]
    public class PracticeUI : BSMLResourceViewController
    {
        public override string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");

        private int _speed;
        [UIValue("speed")]
        public int Speed
        {
            get => this._speed;
            set => this.SetProperty(ref this._speed, value);
        }

        private float _njs;
        [UIValue("njs")]
        public float NJS
        {
            get => this._njs;
            set => this.SetProperty(ref this._njs, value);
        }

        private float _offset;
        [UIValue("offset")]
        public float Offset
        {
            get => this._offset;
            set => this.SetProperty(ref this._offset, value);
        }

        [UIAction("speedFormatter")]
        private string speedForValue(float value)
        {
            return $"{value}%";
        }
        [UIAction("njsFormatter")]
        private string njsForValue(float value)
        {
            return value == UIElementsCreator.defaultNJS ? $"<u>{value}</u>" : $"{value}";
        }
        [UIAction("spawnOffsetFormatter")]
        private string offsetForValue(float value)
        {
            return value == UIElementsCreator.defaultOffset ? $"<u>{value:F2}</u>" : $"{value:F2}";
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            if (this.gameObject.GetComponent<Touchable>() == null) {
                this.gameObject.AddComponent<Touchable>();
            }
        }
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        [Inject]
        public void Constractor(GameplayCoreSceneSetupData gameplayCoreSceneSetupData)
        {
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            if (this._gameplayCoreSceneSetupData.practiceSettings != null) {
                this.Speed = Mathf.RoundToInt(this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul * 100);
                this.NJS = this._gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed != 0 ? this._gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed : BeatmapDifficultyMethods.NoteJumpMovementSpeed(this._gameplayCoreSceneSetupData.difficultyBeatmap.difficulty);
                this.Offset = this._gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpStartBeatOffset;
            }
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChanged(e.PropertyName);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string memberName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            this.OnPropertyChanged(new PropertyChangedEventArgs(memberName));
            return true;
        }
    }
}
