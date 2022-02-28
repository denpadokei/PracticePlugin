using IPA;
using IPA.Config;
using IPA.Config.Stores;
using ModestTree;
using PracticePlugin.Installers;
using SiraUtil.Zenject;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace PracticePlugin
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }

        public const float SpeedMaxSize = 5.05f;
        public const float SpeedStepSize = 0.05f;

        public const int NjsMaxSize = 100;
        public const int NjstepSize = 1;

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Config conf, Zenjector zenjector)
        {
            
            Log = logger;
            Log.Info("PracticePlugin initialized.");
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
            zenjector.Install<PlayerInstaller>(Location.StandardPlayer);
            zenjector.Install<PracticeAppInstaller>(Location.App);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
        }
        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
        }
        //public static void AdjustNJS(float njs)
        //{
        //    float halfJumpDur = 4f;
        //    float maxHalfJump = _spawnController.GetPrivateField<float>("_maxHalfJumpDistance");
        //    float noteJumpStartBeatOffset = _levelData.GameplayCoreSceneSetupData.difficultyBeatmap.noteJumpStartBeatOffset;
        //    float moveSpeed = _spawnController.GetPrivateField<float>("_moveSpeed");
        //    float moveDir = _spawnController.GetPrivateField<float>("_moveDurationInBeats");
        //    float jumpDis;
        //    float spawnAheadTime;
        //    float moveDis;
        //    float bpm = _spawnController.GetPrivateField<float>("_beatsPerMinute");
        //    float num = 60f / bpm;
        //    moveDis = moveSpeed * num * moveDir;
        //    while (njs * num * halfJumpDur > maxHalfJump) {
        //        halfJumpDur /= 2f;
        //    }
        //    halfJumpDur += noteJumpStartBeatOffset;
        //    if (halfJumpDur < 1f) halfJumpDur = 1f;
        //    //        halfJumpDur = spawnController.GetPrivateField<float>("_halfJumpDurationInBeats");
        //    jumpDis = njs * num * halfJumpDur * 2f;
        //    spawnAheadTime = moveDis / moveSpeed + jumpDis * 0.5f / njs;
        //    _spawnController.SetPrivateField("_halfJumpDurationInBeats", halfJumpDur);
        //    _spawnController.SetPrivateField("_spawnAheadTime", spawnAheadTime);
        //    _spawnController.SetPrivateField("_jumpDistance", jumpDis);
        //    _spawnController.SetPrivateField("_noteJumpMovementSpeed", njs);
        //    _spawnController.SetPrivateField("_moveDistance", moveDis);


        //}
        //public static void AdjustSpawnOffset(float offset)
        //{
        //    float njs = _spawnController.GetPrivateField<BeatmapObjectSpawnController.InitData>("_initData").noteJumpMovementSpeed;
        //    float halfJumpDur = 4f;
        //    float maxHalfJump = _spawnController.GetPrivateField<float>("_maxHalfJumpDistance");
        //    float noteJumpStartBeatOffset = offset;
        //    float moveSpeed = _spawnController.GetPrivateField<float>("_moveSpeed");
        //    float moveDir = _spawnController.GetPrivateField<float>("_moveDurationInBeats");
        //    float jumpDis;
        //    float spawnAheadTime;
        //    float moveDis;
        //    float bpm = _spawnController.GetPrivateField<float>("_beatsPerMinute");
        //    float num = 60f / bpm;
        //    moveDis = moveSpeed * num * moveDir;
        //    while (njs * num * halfJumpDur > maxHalfJump) {
        //        halfJumpDur /= 2f;
        //    }
        //    halfJumpDur += noteJumpStartBeatOffset;
        //    if (halfJumpDur < 1f) halfJumpDur = 1f;
        //    //        halfJumpDur = spawnController.GetPrivateField<float>("_halfJumpDurationInBeats");
        //    jumpDis = njs * num * halfJumpDur * 2f;
        //    spawnAheadTime = moveDis / moveSpeed + jumpDis * 0.5f / njs;
        //    _spawnController.SetPrivateField("_halfJumpDurationInBeats", halfJumpDur);
        //    _spawnController.SetPrivateField("_spawnAheadTime", spawnAheadTime);
        //    _spawnController.SetPrivateField("_jumpDistance", jumpDis);
        //    _spawnController.SetPrivateField("_noteJumpMovementSpeed", njs);
        //    _spawnController.SetPrivateField("_moveDistance", moveDis);
        //}

        
        //public void OnSceneUnloaded(Scene scene)
        //{
        //}

        //public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition)
        //{
        //    return CreateText(parent, text, anchoredPosition, new Vector2(60f, 10f));
        //}

        //public static TextMeshProUGUI CreateText(RectTransform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        //{
        //    GameObject gameObj = new GameObject("CustomUIText");
        //    gameObj.SetActive(false);

        //    TextMeshProUGUI textMesh = gameObj.AddComponent<TextMeshProUGUI>();
        //    textMesh.font = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(t => t.name == "Teko-Medium SDF No Glow"));
        //    textMesh.rectTransform.SetParent(parent, false);
        //    textMesh.text = text;
        //    textMesh.fontSize = 4;
        //    textMesh.color = Color.white;

        //    textMesh.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        //    textMesh.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        //    textMesh.rectTransform.sizeDelta = sizeDelta;
        //    textMesh.rectTransform.anchoredPosition = anchoredPosition;

        //    gameObj.SetActive(true);
        //    return textMesh;
        //}
    }
}
