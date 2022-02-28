using PracticePlugin.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace PracticePlugin.Installers
{
    internal class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<PracticeUI>().FromNewComponentAsViewController().AsCached().NonLazy();
            this.Container.BindInterfacesAndSelfTo<LooperUI>().FromNewComponentAsViewController().AsCached();
            this.Container.BindInterfacesAndSelfTo<SongSeeker>().FromNewComponentAsViewController().AsCached();
            this.Container.BindInterfacesAndSelfTo<UIElementsCreator>().FromNewComponentOnNewGameObject().AsCached();
            this.Container.BindInterfacesAndSelfTo<SongSeekBeatmapHandler>().AsCached();
        }
    }
}
