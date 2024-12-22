using SuperExcitingCloneEffect.CloneController;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect
{
    [PluginDetails(AuthorName = "sinβ")]
    [VideoEffect("超☆エキサイティン複製エフェクト", ["配置"], [], isAviUtlSupported: false)]
    public class SuperExcitingCloneEffect : VideoEffectBase
    {
        public override string Label => "超☆エキサイティン複製エフェクト";

        [Display(GroupName = "複製内容", Name = "描画順序")]
        [CloneOrderChanger(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<CloneBlock> Clones { get => clones; set => Set(ref clones, value); }
        ImmutableList<CloneBlock> clones = [new()];


        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new SuperExcitingCloneEffectProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [.. Clones];
    }
}
