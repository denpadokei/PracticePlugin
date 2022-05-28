using BeatSaberMarkupLanguage;
using PracticePlugin.Configuration;
using PracticePlugin.Views;
using SiraUtil.Affinity;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace PracticePlugin.Models
{
    public class UIElementsCreator : IInitializable, IAffinity
    {
        private readonly SongSeeker _songSeeker;
        private readonly SongTimeInfoEntity _songTimeInfoEntity;
        private readonly PracticeUI _practiceUI;
        [Inject]
        public UIElementsCreator(
            SongTimeInfoEntity songTimeInfoEntity,
            SongSeeker songSeeker,
            PracticeUI practiceUI)
        {
            this._songTimeInfoEntity = songTimeInfoEntity;
            this._songSeeker = songSeeker;
            this._practiceUI = practiceUI;
        }
        public void Initialize()
        {
            if (!this._songTimeInfoEntity.PracticeMode) {
                return;
            }
            var canvas = GameObject.Find("PauseMenu").transform.Find("Wrapper").transform.Find("MenuWrapper").transform.Find("Canvas");
            if (canvas == null) {
                return;
            }
            var uiObj = new GameObject("PracticePlugin Seeker UI", typeof(RectTransform));
            (uiObj.transform as RectTransform).anchorMin = new Vector2(0, 0);
            (uiObj.transform as RectTransform).anchorMax = new Vector2(1, 1);
            (uiObj.transform as RectTransform).sizeDelta = new Vector2(0, 0);
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), this._practiceUI.ResourceName), canvas.gameObject, this._practiceUI);
            uiObj.transform.SetParent(canvas, false);
            uiObj.transform.localScale = new Vector3(1, 1, 1);
            uiObj.transform.localPosition = new Vector3(0f, -3f, 0f);
            this._songSeeker.gameObject.transform.SetParent(canvas, false);
        }

        [AffinityPatch(typeof(GameEnergyCounter), nameof(GameEnergyCounter.Start))]
        [AffinityPostfix]
        protected void EnergyCounterStartPostfix(GameEnergyCounter __instance)
        {
            if (!this._songTimeInfoEntity.PracticeMode) {
                return;
            }
            if (__instance.energyType != GameplayModifiers.EnergyType.Battery && PluginConfig.Instance.StartWithFullEnergy) {
                __instance.ProcessEnergyChange(1 - __instance.energy);
            }
        }
    }
}