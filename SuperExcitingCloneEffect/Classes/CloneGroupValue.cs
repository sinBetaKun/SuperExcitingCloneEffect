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
        private bool _isOpned = true;

        [JsonIgnore]
        public CloneGroupValue? Parent { get => _parent; set => Set(ref _parent, value); }
        private CloneGroupValue? _parent = null;

        [JsonIgnore]
        public int Depth { get => _depth; set => Set(ref _depth, value); }
        private int _depth;

        [Display(GroupName = nameof(TextResource.GroupName_CloneGroupValue), Name = nameof(TextResource.IManagedItem_Hide), ResourceType = typeof(TextResource))]
        [ToggleSlider]
        public bool Hide { get => _hide; set => Set(ref _hide, value); }
        private bool _hide = false;

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

        public CloneGroupValue()
        {
        }

        public CloneGroupValue(CloneGroupValue origin)
        {
            IsOpened = origin.IsOpened;
            Depth = origin.Depth;
            Hide = origin.Hide;
            NameTag = origin.NameTag;
            Comment = origin.Comment;
            Effects = YukkuriMovieMaker.Json.Json.GetClone(origin.Effects)!;

            //List<IManagedItem> children = [];

            //foreach (IManagedItem mi in origin.Chirdren)
            //{
            //    if (mi is CloneValue cv)
            //    {
            //        children.Add(new CloneValue(cv));
            //    }
            //    else
            //    {
            //        CloneGroupValue gv = (CloneGroupValue)mi;
            //        children.Add(new CloneGroupValue(gv));
            //    }
            //}

            //Chirdren = [.. children];
        }

        protected override IEnumerable<IAnimatable> GetAnimatables()
            => [.. Effects];
    }
}
