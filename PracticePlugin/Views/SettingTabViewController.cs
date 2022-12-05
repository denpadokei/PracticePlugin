using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using PracticePlugin.Configuration;
using System;
using Zenject;

namespace PracticePlugin.Views
{
    [HotReload]
    internal class SettingTabViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        [UIValue("start-with-full-energy")]
        public bool StartWithFullEnergy
        {
            get => PluginConfig.Instance.StartWithFullEnergy;
            set => PluginConfig.Instance.StartWithFullEnergy = value;
        }

        [UIValue("show-time-failed")]
        public bool ShowTimeFailed
        {
            get => PluginConfig.Instance.ShowTimeFailed;
            set => PluginConfig.Instance.ShowTimeFailed = value;
        }

        [UIValue("adjust-njs-with-speed")]
        public bool AdjustNJSWithSpeed
        {
            get => PluginConfig.Instance.AdjustNJSWithSpeed;
            set => PluginConfig.Instance.AdjustNJSWithSpeed = value;
        }

        [UIValue("disable-pitch-correction")]
        public bool DisablePithCorrection
        {
            get => PluginConfig.Instance.DisablePitchCorrection;
            set => PluginConfig.Instance.DisablePitchCorrection= value;
        }

        private bool _disposedValue;
        public void Initialize()
        {
            GameplaySetup.instance.RemoveTab("PracticePlugin");
            GameplaySetup.instance.AddTab("PracticePlugin", "PracticePlugin.Views.SettingTabViewController", this);
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue) {
                if (disposing) {
                    GameplaySetup.instance.RemoveTab("PracticePlugin");
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
