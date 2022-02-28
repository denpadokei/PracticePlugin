using PracticePlugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
