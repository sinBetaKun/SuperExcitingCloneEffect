using SuperExcitingCloneEffect.Interfaces;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.Classes
{
    public class CloneValueListManager : Animatable
    {
        public ImmutableList<IManagedItem> ManagedItems
        {
            get => _managedItems;
            set => Set(ref _managedItems, value);
        }
        private ImmutableList<IManagedItem> _managedItems = [new CloneValue()];

        [JsonIgnore]
        public ImmutableList<IManagedItem> SelectedManagedItems { get => _selectedManagedItems; set => Set(ref _selectedManagedItems, value); }
        private ImmutableList<IManagedItem> _selectedManagedItems = [];

        public int ItemCount()
        {
            int count = 0;

            List<IManagedItem> mis1 = [.. ManagedItems];
            List<IManagedItem> mis2 = [];

            while (mis1.Count > 0)
            {
                foreach (IManagedItem m in mis1)
                    if (m is CloneGroupValue gv)
                        mis2.AddRange(gv.Chirdren);

                count += mis1.Count;
                mis1 = mis2;
                mis2 = [];
            }

            return count;
        }

        protected override IEnumerable<IAnimatable> GetAnimatables()
            => [.. ManagedItems];
    }
}
