using SuperExcitingCloneEffect.Classes;
using System.Collections.Immutable;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.Interfaces
{
    public interface IManagedItem : IAnimatable
    {
        public int Depth { get; set; }

        public CloneGroupValue? Parent { get; set; }

        public bool Hide { get; set; }

        public string NameTag { get; set; }

        public string Comment { get; set; }

        public ImmutableList<IVideoEffect> Effects { get; set; }
    }
}
