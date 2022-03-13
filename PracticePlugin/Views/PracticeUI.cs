using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Views
{
    [HotReload]
    public class PracticeUI : BSMLAutomaticViewController
    {
        public string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");

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
        public string SpeedForValue(float value)
        {
            return $"{value}%";
        }
        [UIAction("njsFormatter")]
        public string NjsForValue(float value)
        {
            return Mathf.Approximately(value, this._defaultNJS) ? $"<u>{value}</u>" : $"{value}";
        }
        [UIAction("spawnOffsetFormatter")]
        public string OffsetForValue(float value)
        {
            return Mathf.Approximately(value, this._defaultOffset) ? $"<u>{value:F2}</u>" : $"{value:F2}";
        }

        [UIAction("#post-parse")]
        public void PostParse()
        {
            if (this.gameObject.GetComponent<Touchable>() == null) {
                this.gameObject.AddComponent<Touchable>();
            }
        }
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private float _defaultNJS;
        private float _defaultOffset;
        [Inject]
        public void Constractor(GameplayCoreSceneSetupData gameplayCoreSceneSetupData, BeatmapObjectSpawnController.InitData initData)
        {
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            if (this._gameplayCoreSceneSetupData.practiceSettings != null) {
                this.Speed = Mathf.RoundToInt(this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul * 100);
                this._defaultNJS = initData.noteJumpMovementSpeed;
                this.NJS = this._defaultNJS;
                this._defaultOffset = initData.noteJumpValue;
                this.Offset = this._defaultOffset;
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
