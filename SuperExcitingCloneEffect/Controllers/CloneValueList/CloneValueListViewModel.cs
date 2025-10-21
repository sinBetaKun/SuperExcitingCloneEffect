using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using System.Collections.Immutable;
using System.ComponentModel;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    internal class CloneValueListViewModel : Bindable, IPropertyEditorControl, IDisposable
    {
        private readonly CloneValueListView _view;
        private readonly INotifyPropertyChanged _item;
        private readonly ItemProperty[] _properties;

        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public ImmutableList<IManagedItem> ManagedItems { get => _managedItems; set => Set(ref _managedItems, value); }
        private ImmutableList<IManagedItem> _managedItems = [];

        public ImmutableList<IManagedItem> Source { get => _source; set => Set(ref _source, value); }
        private ImmutableList<IManagedItem> _source = [];

        public int SelectedIndex { get => selectedIndex; set => Set(ref selectedIndex, value); }
        int selectedIndex = -1;

        private readonly List<CloneGroupValue> _openeds = [];

        public CloneValueListViewModel(ItemProperty[] properties, CloneValueListView view)
        {
            _properties = properties;
            _view = view;

            _item = (INotifyPropertyChanged)properties[0].PropertyOwner;
            _item.PropertyChanged += Item_PropertyChanged;

            UpdateClones();
        }

        public void CopyToOtherItems()
        {
            var otherProperties = _properties.Skip(1);
            SetProperties();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _properties[0].PropertyInfo.Name)
                UpdateClones();
        }

        private void UpdateClones()
        {
            ImmutableList<IManagedItem> list = _properties[0].GetValue<ImmutableList<IManagedItem>>() ?? [];

            if (!ManagedItems.SequenceEqual(list))
            {
                ManagedItems = DeepCloneManagedItems(list);
            }

            int index = SelectedIndex;
            FindOpendGroup();
            UpdateSource();
            SelectedIndex = index;
        }

        public void SetProperties()
        {
            foreach (var property in _properties)
                property.SetValue(/*DeepCloneManagedItems(ManagedItems)*/ ManagedItems);
        }

        private static ImmutableList<IManagedItem> DeepCloneManagedItems(ImmutableList<IManagedItem> origin)
        {
            CloneTreeNode tree = new(origin);

            return [.. tree.GetCloneTree().ToList()];
        }

        private void FindOpendGroup()
        {
            _openeds.Clear();

            foreach (IManagedItem mi in ManagedItems)
                if (mi is CloneGroupValue gv && gv.IsOpened)
                    _openeds.Add(gv);
        }

        public void UpdateSource()
        {
            Source = [.. GetVisibleItem()];
        }

        public List<IManagedItem> GetVisibleItem()
        {
            return new CloneTreeNode(ManagedItems).GetVisibleNode().ToList();
        }

        public void AddOpenedGroup(CloneGroupValue gv)
        {
            gv.IsOpened = true;
            _openeds.Add(gv);
            UpdateSource();
        }

        public void RemoveOpenedGroup(CloneGroupValue gv)
        {
            gv.IsOpened = false;
            _openeds.Remove(gv);
            UpdateSource();
        }
        public IEnumerable<IManagedItem> FindChildren(CloneGroupValue gv)
        {
            return ManagedItems.Where(mi => mi.Parent == gv);
        }

        public void InsertItems(List<IManagedItem> items)
        {
            if (SelectedIndex < -1 || SelectedIndex >= Source.Count)
                return;

            IManagedItem first = items.First();
            int index = SelectedIndex;
            bool flag = false;

            BeginEdit?.Invoke(this, EventArgs.Empty);

            if (index < 0)
            {
                ManagedItems = ManagedItems.AddRange(items);
                flag = true;
            }
            else if (Source[index] is CloneGroupValue gv)
            {
                IManagedItem[] array = [.. FindChildren(gv)];

                if (array.Length == 0)
                {
                    foreach (IManagedItem mi in items)
                        mi.Parent ??= gv;

                    int index2 = ManagedItems.IndexOf(gv) + array.Length + 1;
                    if (index2 < ManagedItems.Count)
                        ManagedItems = ManagedItems.InsertRange(index2, items);
                    else
                        ManagedItems = ManagedItems.AddRange(items);

                    flag = true;
                }
            }
            
            if (!flag)
            {
                IManagedItem target = Source[index];

                if (target.Parent is CloneGroupValue gv)
                    foreach (IManagedItem mi in items)
                        mi.Parent ??= gv;

                int index2 = ManagedItems.IndexOf(target);
                if (index2 + 1 == ManagedItems.Count)
                    ManagedItems = ManagedItems.AddRange(items);
                else
                    ManagedItems = ManagedItems.InsertRange(index2 + 1, items);
            }

            UpdateSource();
            int index3 = Source.IndexOf(first);
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index3;
        }

        public void RemoveItems(IEnumerable<IManagedItem> items)
        {
            int index = SelectedIndex;

            BeginEdit?.Invoke(this, EventArgs.Empty);

            HashSet<IManagedItem> hash = [];

            foreach (IManagedItem mi1 in items)
            {
                List<IManagedItem> list1 = [mi1];
                List<IManagedItem> list2 = [];

                while (list1.Count > 0)
                {
                    foreach (IManagedItem mi2 in list1)
                    {
                        if (mi2 is CloneGroupValue gv)
                            list2.AddRange(FindChildren(gv));

                        hash.Add(mi2);
                    }

                    list1 = list2;
                    list2 = [];
                }
            }

            foreach (IManagedItem mi in hash)
                ManagedItems = ManagedItems.Remove(mi);

            UpdateSource();
            int index2 = (index < Source.Count - 1) ? index : -1;
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index2;
        }

        public bool CanMoveUpItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (target.Parent is CloneGroupValue gv)
            {
                if (FindChildren(gv).ToList().IndexOf(target) < 1)
                    return false;
            }
            else
            {
                if (ManagedItems.IndexOf(target) < 1)
                    return false;
            }

            return true;
        }

        public void MoveUpItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (target.Parent is CloneGroupValue gv)
            {
                if (FindChildren(gv).ToList().IndexOf(target) < 1)
                    return;
            }
            else
            {
                if (ManagedItems.IndexOf(target) < 1)
                    return;
            }

            int index2 = ManagedItems.IndexOf(target);

            BeginEdit?.Invoke(this, EventArgs.Empty);
            ManagedItems = ManagedItems
                .Remove(target)
                .Insert(index2 - 1, target);

            UpdateSource();
            int index3 = Source.IndexOf(target);
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index3;
            _view.UpdateButtons();
        }

        public bool CanMoveDownItem()
        {

            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (target.Parent is CloneGroupValue gv)
            {
                List<IManagedItem> list = FindChildren(gv).ToList();
                if (list.IndexOf(target) > list.Count - 2)
                    return false;
            }
            else
            {
                if (ManagedItems.IndexOf(target) > ManagedItems.Count - 2)
                    return false;
            }

            return true;
        }

        public void MoveDownItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (target.Parent is CloneGroupValue gv)
            {
                List<IManagedItem> list = FindChildren(gv).ToList();
                if (list.IndexOf(target) > list.Count - 2)
                    return;
            }
            else
            {
                if (ManagedItems.IndexOf(target) > ManagedItems.Count - 2)
                    return;
            }

            int index2 = ManagedItems.IndexOf(target);
            BeginEdit?.Invoke(this, EventArgs.Empty);

            if (index < ManagedItems.Count - 2)
            {
                ManagedItems = ManagedItems
                    .Remove(target)
                    .Insert(index2 + 1, target);
            }
            else
            {
                ManagedItems = ManagedItems
                    .Remove(target)
                    .Add(target);
            }

            UpdateSource();
            int index3 = Source.IndexOf(target);
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index3;
            _view.UpdateButtons();
        }

        public void Dispose()
        {
            _item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
