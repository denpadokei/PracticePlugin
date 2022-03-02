using PracticePlugin.Models;
using Zenject;

namespace PracticePlugin.Installers
{
    internal class PracticeAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<SongTimeInfoEntity>().AsCached().NonLazy();
        }
    }
}
