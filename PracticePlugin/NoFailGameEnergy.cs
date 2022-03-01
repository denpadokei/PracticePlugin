﻿using System.Collections;
using UnityEngine;
namespace PracticePlugin
{
    public class NoFailGameEnergy : MonoBehaviour
    {
        private readonly GameEnergyUIPanel _gameEnergyUIPanel;
        private readonly GameEnergyCounter _gameEnergyCounter;
        private readonly Animator _levelFailedAnimator;
        private readonly GameObject _levelFailedGameObject;
        public static bool limitLevelFail = false;
        public static bool hasFailed;
        private bool _isFailedVisible;

        private void Awake()
        {/*
            hasFailed = false;
            limitLevelFail = Config.GetBool("PracticePlugin", "limitLevelFailDisplay", false, true);

			_gameEnergyUIPanel = Resources.FindObjectsOfTypeAll<GameEnergyUIPanel>().FirstOrDefault();
			if (_gameEnergyUIPanel == null) return;
			_gameEnergyUIPanel.gameObject.SetActive(true);

			_gameEnergyCounter = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().FirstOrDefault();
            _gameEnergyCounter.gameEnergyDidChangeEvent += _gameEnergyUIPanel.HandleGameEnergyDidChange;
			var levelFailedController = Resources.FindObjectsOfTypeAll<StandardLevelFailedController>().FirstOrDefault();
			if (levelFailedController == null) return;
			var textEffect = levelFailedController.GetPrivateField<LevelFailedTextEffect>("_levelFailedTextEffect");
			_levelFailedAnimator = textEffect.GetPrivateField<Animator>("_animator");
			_levelFailedGameObject = GameObject.Find("LevelFailedTextEffect");
            */
        }

        private void LateUpdate()
        {/*
			if (_isFailedVisible) return;
			if (_gameEnergyCounter.energy > 1E-05f) return;

            if(limitLevelFail == true)
            {
                if (hasFailed == false)
                    StartCoroutine(LevelFailedRoutine());
                if (hasFailed == true)
                    _gameEnergyCounter.AddEnergy(0.5f);
            }
          else
            {
			StartCoroutine(LevelFailedRoutine());
                _gameEnergyCounter.AddEnergy(0.5f);
            }
            */
        }

        private IEnumerator LevelFailedRoutine()
        {

            if (!(limitLevelFail == true && hasFailed == true)) {
                this._isFailedVisible = true;

                this._levelFailedGameObject.SetActive(false);
                this._levelFailedAnimator.enabled = true;
                yield return new WaitForSeconds(0.1f);
                this._levelFailedGameObject.SetActive(true);
                var waitTime = Time.realtimeSinceStartup + 3;
                while (Time.realtimeSinceStartup < waitTime) {
                    this._gameEnergyCounter.ProcessEnergyChange(-1);
                    yield return null;
                }
                this._levelFailedGameObject.SetActive(false);

                this._gameEnergyCounter.ProcessEnergyChange(0.5f);
                this._isFailedVisible = false;
                if (limitLevelFail == true) {
                    hasFailed = true;
                }
            }

        }
    }
}