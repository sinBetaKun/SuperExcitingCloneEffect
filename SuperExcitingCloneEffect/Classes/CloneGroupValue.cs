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
        public bool IsOpened { get => _isOpned; set => Set(ref _isOpned, value); }
        private bool _isOpned = false;

        [JsonIgnore]
        public int Depth { get => _depth; set => Set(ref _depth, value); }
        private int _depth;

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_Hide), ResourceType = typeof(TextResource))]
        [ToggleSlider]
        public bool Hide { get => _appear; set => Set(ref _appear, value); }
        private bool _appear = true;

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_NameTag), ResourceType = typeof(TextResource))]
        [TextEditor]
        public string NameTag { get => _nameTag; set => Set(ref _nameTag, value); }
        private string _nameTag = string.Empty;

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_Comment), ResourceType = typeof(TextResource))]
        [TextEditor]
        public string Comment { get => _comment; set => Set(ref _comment, value); }
        private string _comment = string.Empty;

        [Display(GroupName = nameof(TextResource.IManagedItem_GroupEffects), ResourceType = typeof(TextResource))]
        [VideoEffectSelector(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ImmutableList<IVideoEffect> Effects { get => effects; set => Set(ref effects, value); }
        ImmutableList<IVideoEffect> effects = [];

        public ImmutableList<IManagedItem> Chirdren { get => _children; set => Set(ref _children, value); }
        private ImmutableList<IManagedItem> _children = [];

        protected override IEnumerable<IAnimatable> GetAnimatables()
            => [.. Effects, .. Chirdren];
    }
}
