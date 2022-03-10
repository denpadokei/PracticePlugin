using PracticePlugin.Views;
using Zenject;

namespace PracticePlugin.Installers
{
    internal class PracticeMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<ResultViewTextController>().FromNewComponentAsViewController().AsSingle().NonLazy();
        }
    }
}
