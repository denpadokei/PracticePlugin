using IPA.Config.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PracticePlugin.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual bool StartWithFullEnergy { get; set; } = false;
        public virtual bool ShowTimeFailed { get; set; } = true;
        public virtual bool DisablePitchCorrection { get; set; } = false;
        public virtual bool AdjustNJSWithSpeed { get; set; } = false;
    }
}
