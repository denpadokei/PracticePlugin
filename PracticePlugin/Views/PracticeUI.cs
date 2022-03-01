using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using PracticePlugin.Configuration;
using PracticePlugin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Views
{
    public class PracticeUI : BSMLResourceViewController, IInitializable
    {
        public override string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");

        private float _speed;
        [UIValue("speed")]
        public float speed
        {
            get => this._speed;
            set => this.SetProperty(ref this._speed, value);
        }
        [UIAction("setSpeed")]
        private void SetSpeed(float value)
        {
            this.speed = value;
        }
        private float _njs;

        [UIValue("njs")]
        public float njs
        {
            get => this._njs;
            set => this.SetProperty(ref this._njs, value);
        }
        [UIAction("setnjs")]
        private void SetNjs(float value)
        {
            this.njs = value;
        }
        private float _offset;
        [UIValue("offset")]
        public float offset
        {
            get => this._offset;
            set => this.SetProperty(ref this._offset, value);
        }

        [UIAction("setoffset")]
        private void SetSpawnOffset(float value)
        {
            this.offset = value;
        }

        [UIAction("speedFormatter")]
        private string speedForValue(float value)
        {
            return $"{(int)(value * 100)}%";
        }
        [UIAction("njsFormatter")]
        private string njsForValue(float value)
        {
            return value == UIElementsCreator.defaultNJS ? $"<u>{value}</u>" : $"{value}";
        }
        [UIAction("spawnOffsetFormatter")]
        private string offsetForValue(float value)
        {
            return value == UIElementsCreator.defaultOffset ? $"<u>{value.ToString("F2")}</u>" : $"{value.ToString("F2")}";
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            if (this.gameObject.GetComponent<Touchable>() == null) {
                this.gameObject.AddComponent<Touchable>();
            }
        }
        private UIElementsCreator _uiElementsCreator;
        private GameEnergyCounter _gameEnergyCounter;
        private LooperUI _looperUI;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private SongTimeInfoEntity _songTimeInfoEntity;
        private AudioSpeedController _audioSpeedController;
        [Inject]
        public void Constractor(GameplayCoreSceneSetupData gameplayCoreSceneSetupData, GameEnergyCounter gameEnergyCounter, UIElementsCreator uIElementsCreator, LooperUI looperUI, SongTimeInfoEntity songTimeInfoEntity, AudioSpeedController audioSpeedController)
        {
            this._gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this._gameEnergyCounter = gameEnergyCounter;
            this._uiElementsCreator = uIElementsCreator;
            this._looperUI = looperUI;
            this._audioSpeedController = audioSpeedController;
            songTimeInfoEntity.PracticeMode = gameplayCoreSceneSetupData.practiceSettings != null;
            this._songTimeInfoEntity = songTimeInfoEntity;
            if (this._gameplayCoreSceneSetupData.practiceSettings != null) {
                this.speed = this._gameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
                this.njs = this._gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed != 0 ? this._gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed : BeatmapDifficultyMethods.NoteJumpMovementSpeed(this._gameplayCoreSceneSetupData.difficultyBeatmap.difficulty);
                this.offset = this._gameplayCoreSceneSetupData.difficultyBeatmap.noteJumpStartBeatOffset;
            }
        }

        public void OnDisable()
        {
            this._songTimeInfoEntity.TimeScale = this.speed;
        }

        public void Initialize()
        {
            if (!this._songTimeInfoEntity.PracticeMode) {
                return;
            }

            try {

                Logger.Debug("Atemmpting Practice Plugin UI");

                var canvas = GameObject.Find("PauseMenu").transform.Find("Wrapper").transform.Find("MenuWrapper").transform.Find("Canvas");

                if (canvas == null) {
                    Logger.Debug("Canvas Null");
                }


                var uiObj = new GameObject("PracticePlugin Seeker UI", typeof(RectTransform));

                (uiObj.transform as RectTransform).anchorMin = new Vector2(0, 0);
                (uiObj.transform as RectTransform).anchorMax = new Vector2(1, 1);
                (uiObj.transform as RectTransform).sizeDelta = new Vector2(0, 0);

                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this.ResourceName), canvas.gameObject, this);
                uiObj.transform.SetParent(canvas, false);
                uiObj.transform.localScale = new Vector3(1, 1, 1);
                uiObj.transform.localPosition = new Vector3(0f, -3f, 0f);
                this._uiElementsCreator.transform.SetParent(uiObj.transform, false);
                //this._looperUI.transform.SetParent(uiObj.transform, false);
                if (PluginConfig.Instance.StartWithFullEnergy) {
                    this._gameEnergyCounter.ProcessEnergyChange(1 - this._gameEnergyCounter.energy);
                }
            }
            catch (Exception ex) {
                Logger.Debug(ex.ToString());
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.offset) || e.PropertyName == nameof(this.njs)) {
                this._uiElementsCreator.UpdateSpawnMovementData(this.njs, this.offset);
            }
            else if (e.PropertyName == nameof(this.speed)) {
                this._audioSpeedController.TimeScale = this.speed;
            }
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
