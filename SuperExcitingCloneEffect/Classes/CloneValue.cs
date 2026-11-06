using SuperExcitingCloneEffect.Interfaces;
using SuperExcitingCloneEffect.Properties;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project;

namespace SuperExcitingCloneEffect.Classes
{
    public class CloneValue : Animatable, IManagedItem
    {
        [JsonIgnore]
        public int Depth { get; set => Set(ref field, value); }

        public int ParentIndex { get; set => Set(ref field, value); } = -1;

        [Display(GroupName = nameof(TextResource.GroupName_CloneValue), Name = nameof(TextResource.IManagedItem_Hide), ResourceType = typeof(TextResource))]
        [ToggleSlider]
        public bool Hide { get; set => Set(ref field, value); } = false;

        [Display(GroupName = nameof(TextResource.GroupName_CloneValue), Name = nameof(TextResource.IManagedItem_NameTag), ResourceType = typeof(TextResource))]
        [TextEditor]
        public string NameTag { get; set => Set(ref field, value); } = string.Empty;

        [Display(GroupName = nameof(TextResource.GroupName_CloneValue), Name = nameof(TextResource.IManagedItem_Blend), ResourceType = typeof(TextResource))]
        [EnumComboBox]
        public Blend Blend { get; set => Set(ref field, value); } = Blend.Normal;

        [Display(GroupName = nameof(TextResource.GroupName_CloneValue), Name = nameof(TextResource.IManagedItem_Comment), ResourceType = typeof(TextResource))]
        [TextEditor]
        public string Comment { get; set => Set(ref field, value); } = string.Empty;

        [Display(GroupName = nameof(TextResource.IManagedItem_CloneEffects), ResourceType = typeof(TextResource))]
        [VideoEffectSelector(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<IVideoEffect> Effects { get; set => Set(ref field, value); } = [];

        public CloneValue()
        {
        }

        public CloneValue(CloneValue origin)
        {
            Depth = origin.Depth;
            ParentIndex = origin.ParentIndex;
            Hide = origin.Hide;
            NameTag = origin.NameTag;
            Blend = origin.Blend;
            Comment = origin.Comment;
            Effects = YukkuriMovieMaker.Json.Json.GetClone(origin.Effects)!;
        }

        protected override IEnumerable<IAnimatable> GetAnimatables()
            => [.. Effects];
    }
}
