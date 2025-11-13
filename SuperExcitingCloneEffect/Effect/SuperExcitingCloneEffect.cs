using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Controllers.CloneValueList;
using SuperExcitingCloneEffect.Interfaces;
using SuperExcitingCloneEffect.Properties;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.Effect
{
    [VideoEffect(nameof(TextResource.Plugin_Name), [nameof(TextResource.Plugin_Category)],
        [nameof(TextResource.Plugin_Tag_1)],
        isAviUtlSupported: false, ResourceType = typeof(TextResource))]
    public class SuperExcitingCloneEffect : VideoEffectBase
    {
        public override string Label => TextResource.Plugin_Name;

        [Display]
        [CloneValueList(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<IManagedItem> ManagedItems { get => _managedItems; set => Set(ref _managedItems, value); }
        private ImmutableList<IManagedItem> _managedItems = [new CloneValue()];

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new SuperExcitingCloneEffectProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [.. ManagedItems];
    }
}
