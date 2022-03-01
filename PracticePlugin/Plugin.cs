using IPA;
using IPA.Config;
using IPA.Config.Stores;
using PracticePlugin.Installers;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace PracticePlugin
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static IPALogger Log { get; private set; }
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
            zenjector.Install<PracticeMenuInstaller>(Location.Menu);
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
    }
}
