using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using UnityEngine;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;
using System.Reflection;
using PracticePlugin.Configuration;

namespace PracticePlugin.Views
{
    public class PracticeUI : BSMLResourceViewController, IInitializable
    {
        public override string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name, "bsml");

        private float _speed = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.practiceSettings.songSpeedMul;
        [UIValue("speed")]
        public float speed
        {
            get => _speed;
            set
            {
                _speed = value;
           //     Plugin.TimeScale = PracticeUI.instance.speed;
            }
        }
        [UIAction("setSpeed")]
        void SetSpeed(float value)
        {
            speed = value;
        }
        private float _njs = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed != 0?
            BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.noteJumpMovementSpeed : 
            BeatmapDifficultyMethods.NoteJumpMovementSpeed(BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.difficulty);

        [UIValue("njs")]
        public float njs
        {
            get => _njs;
            set
            {
                _njs = value;
            }
        }
        [UIAction("setnjs")]
        void SetNjs(float value)
        {
            njs = value;
            _uiElementsCreator.NjsController_ValueChangedEvent(value);
        }
        private float _offset = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.noteJumpStartBeatOffset;
        [UIValue("offset")]
        public float offset
        {
            get => _offset;
            set
            {
                _offset = value;
            }
        }

        [UIAction("setoffset")]
        void SetSpawnOffset(float value)
        {
            offset = value;
            _uiElementsCreator.SpawnOffsetController_ValueChangedEvent(value);
        }

        [UIAction("speedFormatter")]
        string speedForValue(float value)
        {
            return $"{(int)(value * 100)}%";
        }
        [UIAction("njsFormatter")]
        string njsForValue(float value)
        {
            return value == UIElementsCreator.defaultNJS ? $"<u>{value}</u>" : $"{value}";
        }
        [UIAction("spawnOffsetFormatter")]
        string offsetForValue(float value)
        {
            return value == UIElementsCreator.defaultOffset ? $"<u>{value.ToString("F2")}</u>" : $"{value.ToString("F2")}";
        }

        [UIAction("#post-parse")]
        void PostParse()
        {
          if(gameObject.GetComponent<Touchable>() == null) 
                gameObject.AddComponent<Touchable>();
        }
        private UIElementsCreator _uiElementsCreator;
        private GameEnergyCounter _gameEnergyCounter;
        private PracticeUI _practiceUI;
        [Inject]
        public void Constractor(GameplayCoreSceneSetupData gameplayCoreSceneSetupData, PauseMenuManager pauseMenuManager, SaberManager saberManager, BeatmapObjectSpawnController beatmapObjectSpawnController, GameEnergyCounter gameEnergyCounter, UIElementsCreator uIElementsCreator, PracticeUI practiceUI)
        {
            this._gameEnergyCounter = gameEnergyCounter;
            this._uiElementsCreator = uIElementsCreator;
            this._practiceUI = practiceUI;
        }

        public void Initialize()
        {
            try {
                
                Logger.Debug("Atemmpting Practice Plugin UI");

                var canvas = GameObject.Find("PauseMenu").transform.Find("Wrapper").transform.Find("MenuWrapper").transform.Find("Canvas");

                if (canvas == null) {
                    Logger.Debug("Canvas Null");
                }


                GameObject uiObj = new GameObject("PracticePlugin Seeker UI", typeof(RectTransform));

                (uiObj.transform as RectTransform).anchorMin = new Vector2(0, 0);
                (uiObj.transform as RectTransform).anchorMax = new Vector2(1, 1);
                (uiObj.transform as RectTransform).sizeDelta = new Vector2(0, 0);

                _uiElementsCreator.gameObject.transform.SetParent(this.transform, false);
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this._practiceUI.ResourceName), canvas.gameObject, this._practiceUI);
                uiObj.transform.SetParent(canvas, false);

                uiObj.transform.localScale = new Vector3(1, 1, 1);
                uiObj.transform.localPosition = new Vector3(0f, -3f, 0f);
                if (PluginConfig.Instance.StartWithFullEnergy) {
                    this._gameEnergyCounter.ProcessEnergyChange(1 - _gameEnergyCounter.energy);
                }
            }
            catch (Exception ex) {
                Logger.Debug(ex.ToString());
            }
        }
    }
}
