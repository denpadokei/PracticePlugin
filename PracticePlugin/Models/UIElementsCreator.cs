﻿using BeatSaberMarkupLanguage;
using PracticePlugin.Configuration;
using PracticePlugin.Views;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Models
{
    public class UIElementsCreator : MonoBehaviour, IInitializable
    {
        private SongSeeker _songSeeker;
        internal static float s_defaultNJS;
        internal static float s_defaultOffset;
        private GameEnergyCounter _gameEnergyCounter;
        private SongTimeInfoEntity _songTimeInfoEntity;
        public BeatmapObjectSpawnController _spawnController;
        private PracticeUI _practiceUI;
        [Inject]
        public void Constractor(
            GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            BeatmapObjectSpawnController beatmapObjectSpawnController,
            SongTimeInfoEntity songTimeInfoEntity,
            SongSeeker songSeeker,
            PracticeUI practiceUI,
            GameEnergyCounter gameEnergyCounter)
        {
            this._spawnController = beatmapObjectSpawnController;
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._songSeeker = songSeeker;
            this._practiceUI = practiceUI;
            this._gameEnergyCounter = gameEnergyCounter;
            songTimeInfoEntity.PracticeMode = gameplayCoreSceneSetupData.practiceSettings != null;
        }
        public void Initialize()
        {
            this.gameObject.AddComponent<RectTransform>();
            if (!this._songTimeInfoEntity.PracticeMode) {
                return;
            }
            if (PluginConfig.Instance.StartWithFullEnergy) {
                this._gameEnergyCounter.ProcessEnergyChange(1 - this._gameEnergyCounter.energy);
            }
            var canvas = GameObject.Find("PauseMenu").transform.Find("Wrapper").transform.Find("MenuWrapper").transform.Find("Canvas");

            if (canvas == null) {
                Logger.Debug("Canvas Null");
                return;
            }
            var uiObj = new GameObject("PracticePlugin Seeker UI", typeof(RectTransform));

            (uiObj.transform as RectTransform).anchorMin = new Vector2(0, 0);
            (uiObj.transform as RectTransform).anchorMax = new Vector2(1, 1);
            (uiObj.transform as RectTransform).sizeDelta = new Vector2(0, 0);
            this.transform.SetParent(uiObj.transform as RectTransform, false);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this._practiceUI.ResourceName), canvas.gameObject, this._practiceUI);
            uiObj.transform.SetParent(canvas, false);
            uiObj.transform.localScale = new Vector3(1, 1, 1);
            uiObj.transform.localPosition = new Vector3(0f, -3f, 0f);
            this._songSeeker.gameObject.transform.SetParent(canvas, false);
        }
    }
}