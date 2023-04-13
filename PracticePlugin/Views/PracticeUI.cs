using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Loader;
using PracticePlugin.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        /// <summary>NJSを編集できるかどうか を取得、設定</summary>
        private bool _njsInterractable = true;
        [UIValue("njs-interactable")]
        /// <summary>NJSを編集できるかどうか を取得、設定</summary>
        public bool NJSInterractable
        {
            get => this._njsInterractable;

            set => this.SetProperty(ref this._njsInterractable, value);
        }

        /// <summary>オフセットを編集できるかどうか を取得、設定</summary>
        private bool _offsetInterractable = true;
        [UIValue("spawn-offset-interactable")]
        /// <summary>オフセットを編集できるかどうか を取得、設定</summary>
        public bool OffsetInterractable
        {
            get => this._offsetInterractable;

            set => this.SetProperty(ref this._offsetInterractable, value);
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
        private SongSeeker _songSeeker;
        private SongSpeedParameeter _beforeDeactiveParam;
        [Inject]
        public void Constractor(GameplayCoreSceneSetupData gameplayCoreSceneSetupData, BeatmapObjectSpawnController.InitData initData, IDifficultyBeatmap level, SongSeeker songSeeker)
        {
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._songSeeker = songSeeker;
            if (this._gameplayCoreSceneSetupData.practiceSettings != null) {
                this.Speed = Mathf.RoundToInt(this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul * 100);
                this._defaultNJS = initData.noteJumpMovementSpeed;
                this.NJS = this._defaultNJS;
                this._defaultOffset = initData.noteJumpValue;
                this.Offset = this._defaultOffset;
            }
            if (PluginManager.EnabledPlugins.Any(x => x.Name == "NoodleExtensions")) {
                var isIsNoodleMap = SongCore.Collections.RetrieveDifficultyData(level)?
                    .additionalDifficultyData?
                    ._requirements?.Any(x => x == "Noodle Extensions") == true;
                this.NJSInterractable = !isIsNoodleMap;
                this.OffsetInterractable = !isIsNoodleMap;
            }
            else {
                this.NJSInterractable = true;
                this.OffsetInterractable = true;
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            this._beforeDeactiveParam = new SongSpeedParameeter
            {
                Speed = this.Speed,
                NJS = this.NJS,
                Offset = this.Offset
            };
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            var afterDeactiveParam = new SongSpeedParameeter
            {
                Speed = this.Speed,
                NJS = this.NJS,
                Offset = this.Offset
            };
            if (this._beforeDeactiveParam != afterDeactiveParam) {
                this._songSeeker.ApplyPlaybackPosition();
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
