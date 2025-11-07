using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.Intrinsics.X86;
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
            foreach (var property in otherProperties)
                property.SetValue(DeepCloneManagedItems(ManagedItems));
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
                property.SetValue(DeepCloneManagedItems(ManagedItems) /*ManagedItems*/);
        }

        private static ImmutableList<IManagedItem> DeepCloneManagedItems(ImmutableList<IManagedItem> origin)
        {
            //CloneTreeNode tree = new(origin);

            //return [.. tree.GetCloneTree().ToList()];

            List<IManagedItem> mis = [];

            foreach (IManagedItem mi in origin)
            {
                if (mi is CloneGroupValue gv)
                    mis.Add(new CloneGroupValue(gv));
                else
                    mis.Add(new CloneValue((CloneValue)mi));
            }

            return [.. mis];
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

        public List<IManagedItem> GetVisibleItem(int parentIndex = -1, int depth = 0)
        {
            List<IManagedItem> ret = [];

            for (int i = 0; i < ManagedItems.Count; i++)
            {
                IManagedItem mi = ManagedItems[i];

                if (mi.ParentIndex == parentIndex)
                {
                    ret.Add(mi);
                    mi.Depth = depth;

                    if (mi is CloneGroupValue gv && gv.IsOpened)
                        ret.AddRange(GetVisibleItem(i, depth + 1));
                }
            }

            return ret;

            //return new CloneTreeNode(ManagedItems).GetVisibleNode().ToList();
        }

        private List<IManagedItem> GetDescendants(IManagedItem mi1)
        {
            if (!ManagedItems.Contains(mi1))
                return [];

            int index1 = ManagedItems.IndexOf(mi1);
            int index2 = index1 + 1;

            if (index2 == ManagedItems.Count)
                return [];

            List<IManagedItem> list1 = [];

            while (ManagedItems[index2].ParentIndex == index1)
            {
                IManagedItem mi2 = ManagedItems[index2];
                list1.Add(mi2);
                List<IManagedItem> list2 = GetDescendants(mi2);
                list1.AddRange(list2);
                index2 += list2.Count + 1;
            }

            return list1;
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
            return ManagedItems.Where(mi => mi.ParentIndex == ManagedItems.IndexOf(gv));
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
                foreach (IManagedItem mi in items)
                    if (mi.ParentIndex > -1)
                        mi.ParentIndex += ManagedItems.Count;

                ManagedItems = ManagedItems.AddRange(items);
                flag = true;
            }
            else if (Source[index] is CloneGroupValue gv)
            {
                IManagedItem[] children = [.. FindChildren(gv)];

                if (children.Length == 0)
                {
                    int index2 = ManagedItems.IndexOf(gv);

                    foreach (IManagedItem mi in items)
                    {
                        if (mi.ParentIndex < 0)
                            mi.ParentIndex = index2;
                        else
                            mi.ParentIndex += index2 + 1;
                    }

                    int index3 = ManagedItems.IndexOf(gv) + 1;
                    if (index3 < ManagedItems.Count)
                        ManagedItems = ManagedItems.InsertRange(index3, items);
                    else
                        ManagedItems = ManagedItems.AddRange(items);

                    foreach (IManagedItem mi in ManagedItems)
                        if (mi.ParentIndex > index2)
                            mi.ParentIndex += items.Count;

                    flag = true;
                }
            }
            
            if (!flag)
            {
                IManagedItem target = Source[index];
                int index2 = ManagedItems.IndexOf(target);

                if (target.ParentIndex > -1 && target.ParentIndex < ManagedItems.Count)
                {
                    foreach (IManagedItem mi in items)
                    {
                        if (mi.ParentIndex < 0)
                            mi.ParentIndex = target.ParentIndex;
                        else
                            mi.ParentIndex += index2 + 1;
                    }
                }

                foreach (IManagedItem mi in ManagedItems)
                    if (mi.ParentIndex > index2)
                        mi.ParentIndex += items.Count;

                if (index2 + 1 == ManagedItems.Count)
                    ManagedItems = ManagedItems.AddRange(items);
                else
                    ManagedItems = ManagedItems.InsertRange(index2 + 1, items);
            }

            UpdateSource();
            int index4 = Source.IndexOf(first);
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index4;
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

            foreach (IManagedItem mi1 in hash)
            {
                int index2 = ManagedItems.IndexOf(mi1);

                for (int i = index2; i < ManagedItems.Count; i++)
                {
                    IManagedItem mi2 = ManagedItems[i];

                    if (mi2.ParentIndex > index2)
                        mi2.ParentIndex--;
                }

                foreach (IManagedItem mi2 in ManagedItems)

                ManagedItems = ManagedItems.Remove(mi1);
            }

            UpdateSource();
            int index3 = (index < Source.Count - 1) ? index : -1;
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index3;
        }



        public bool CanMoveUpItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (target.ParentIndex > -1 && target.ParentIndex < ManagedItems.Count)
            {
                if (ManagedItems[target.ParentIndex] is CloneGroupValue gv)
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
            int index1 = SelectedIndex;
            IManagedItem target = Source[index1];

            if (target.ParentIndex > -1 && target.ParentIndex < ManagedItems.Count)
            {
                if (ManagedItems[target.ParentIndex] is CloneGroupValue gv)
                    if (FindChildren(gv).ToList().IndexOf(target) < 1)
                        return;
            }
            else
            {
                if (ManagedItems.IndexOf(target) < 1)
                    return;
            }

            int index2 = ManagedItems.IndexOf(target);
            List<IManagedItem> list1 = [target, .. GetDescendants(target)];
            List<IManagedItem> list2 = [.. ManagedItems.Where(mi => mi.ParentIndex == target.ParentIndex)];
            IManagedItem mi1 = list2[list2.IndexOf(target) - 1];
            List<IManagedItem> list3 = [mi1, .. GetDescendants(mi1)];
            int index3 = ManagedItems.IndexOf(mi1);

            for (int i = 1; i < list1.Count; i++)
                list1[i].ParentIndex -= list3.Count;

            for (int i = 1; i < list3.Count; i++)
                list3[1].ParentIndex += list1.Count;

            BeginEdit?.Invoke(this, EventArgs.Empty);
            ManagedItems = ManagedItems
                .RemoveRange(list3)
                .RemoveRange(list1);

            if (ManagedItems.Last() == list1.Last())
                ManagedItems = ManagedItems.AddRange([.. list1, .. list3]);
            else
                ManagedItems = ManagedItems.InsertRange(index3, [.. list1, .. list3]);

            UpdateSource();
            int index4 = Source.IndexOf(target);
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index4;
            _view.UpdateButtons();
        }

        public bool CanMoveDownItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (target.ParentIndex > -1 && target.ParentIndex < ManagedItems.Count)
            {
                if (ManagedItems[target.ParentIndex] is CloneGroupValue gv)
                {
                    List<IManagedItem> list = [.. FindChildren(gv)];
                    if (list.IndexOf(target) > list.Count - 2)
                        return false;
                }
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
            int index1 = SelectedIndex;
            IManagedItem target = Source[index1];

            if (target.ParentIndex > -1 && target.ParentIndex < ManagedItems.Count)
            {
                if (ManagedItems[target.ParentIndex] is CloneGroupValue gv)
                {
                    List<IManagedItem> list = [.. FindChildren(gv)];
                    if (list.IndexOf(target) > list.Count - 2)
                        return;
                }
            }
            else
            {
                if (ManagedItems.IndexOf(target) > ManagedItems.Count - 2)
                    return;
            }

            int index2 = ManagedItems.IndexOf(target);
            List<IManagedItem> list1 = [target, .. GetDescendants(target)];
            List<IManagedItem> list2 = [.. ManagedItems.Where(mi => mi.ParentIndex == target.ParentIndex)];
            IManagedItem mi1 = list2[list2.IndexOf(target) + 1];
            List<IManagedItem> list3 = [mi1, .. GetDescendants(mi1)];
            int index3 = ManagedItems.IndexOf(mi1);

            for (int i = 1; i < list1.Count; i++)
                list1[i].ParentIndex += list3.Count;

            for (int i = 1; i < list3.Count; i++)
                list3[1].ParentIndex -= list1.Count;

            BeginEdit?.Invoke(this, EventArgs.Empty);
            ManagedItems = ManagedItems
                .RemoveRange(list1)
                .RemoveRange(list3);

            if (ManagedItems.Last() == list3.Last())
                ManagedItems = ManagedItems.AddRange([.. list3, .. list1]);
            else
                ManagedItems = ManagedItems.InsertRange(index3, [.. list3, .. list1]);

            if (index1 < ManagedItems.Count - 2)
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
            int index4 = Source.IndexOf(target);
            SetProperties();
            UpdateSource();
            EndEdit?.Invoke(this, EventArgs.Empty);
            SelectedIndex = index4;
            _view.UpdateButtons();
        }

        public void Dispose()
        {
            _item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
