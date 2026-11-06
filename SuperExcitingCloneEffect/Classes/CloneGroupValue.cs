using SuperExcitingCloneEffect.Interfaces;
using SuperExcitingCloneEffect.Properties;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace SuperExcitingCloneEffect.Classes
{
    public class CloneGroupValue : Animatable, IManagedItem
    {
        [JsonIgnore]
        public bool IsOpened { get; set => Set(ref field, value); } = true;

        public int ParentIndex { get; set => Set(ref field, value); } = -1;

        [JsonIgnore]
        public int Depth { get; set => Set(ref field, value); }

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_Hide), ResourceType = typeof(TextResource))]
        [ToggleSlider]
        public bool Hide { get; set => Set(ref field, value); } = false;

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_NameTag), ResourceType = typeof(TextResource))]
        [TextEditor]
        public string NameTag { get; set => Set(ref field, value); } = string.Empty;

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_Comment), ResourceType = typeof(TextResource))]
        [TextEditor]
        public string Comment { get; set => Set(ref field, value); } = string.Empty;

        [Display(GroupName = nameof(TextResource.IManagedItem_GroupEffects), ResourceType = typeof(TextResource))]
        [VideoEffectSelector(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<IVideoEffect> Effects { get; set => Set(ref field, value); } = [];

        public CloneGroupValue()
        {
        }

        public CloneGroupValue(CloneGroupValue origin)
        {
            IsOpened = origin.IsOpened;
            ParentIndex = origin.ParentIndex;
            Depth = origin.Depth;
            Hide = origin.Hide;
            NameTag = origin.NameTag;
            Comment = origin.Comment;
            Effects = YukkuriMovieMaker.Json.Json.GetClone(origin.Effects)!;
        }

        protected override IEnumerable<IAnimatable> GetAnimatables()
            => [.. Effects];
    }
}
