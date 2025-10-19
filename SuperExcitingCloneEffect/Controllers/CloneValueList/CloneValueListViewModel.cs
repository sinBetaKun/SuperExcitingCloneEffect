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

        public List<IManagedItem> Selecteds { get; private set; } = [];

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
            CloneValueListManager manager = _properties[0].GetValue<CloneValueListManager>() ?? new();

            if (!ManagedItemsEqual(manager.ManagedItems))
            {
                ManagedItems = DeepCloneManagedItems(manager.ManagedItems);
            }

            FindOpendGroup();
            UpdateSource();
        }

        private bool ManagedItemsEqual(ImmutableList<IManagedItem> other)
        {
            List<(ImmutableList<IManagedItem>, ImmutableList<IManagedItem>)> groupts1
                = [(ManagedItems, other)];
            List<(ImmutableList<IManagedItem>, ImmutableList<IManagedItem>)> groupts2
                = [];

            while (groupts1.Count > 0)
            {
                foreach ((ImmutableList<IManagedItem> a, ImmutableList<IManagedItem> b) in groupts1)
                {
                    if (a.Count != b.Count)
                        return false;

                    for (int i = 0; i < a.Count; i++)
                    {
                        if (a[i] != b[i])
                            return false;

                        if (a[i] is CloneGroupValue ga)
                        {
                            CloneGroupValue gb = (CloneGroupValue)b[i];
                            groupts2.Add((ga.Chirdren, gb.Chirdren));
                        }
                    }
                }

                groupts1 = groupts2;
                groupts2 = [];
            }

            return true;
        }

        public void SetProperties()
        {
            foreach (var property in _properties)
                property.SetValue(MakeManager());
        }

        private CloneValueListManager MakeManager()
        {
            //List<IManagedItem> list = [];

            //foreach (IManagedItem mi in ManagedItems)
            //{
            //    if (mi is CloneValue cv)
            //    {
            //        list.Add(new CloneValue(cv));
            //    }
            //    else
            //    {
            //        CloneGroupValue gv = (CloneGroupValue)mi;
            //        list.Add(new CloneGroupValue(gv));
            //    }
            //}

            return
                    new CloneValueListManager()
                    {
                        ManagedItems = ManagedItems,
                        SelectedManagedItems = [.. Selecteds]
                    };
        }

        private static ImmutableList<IManagedItem> DeepCloneManagedItems(ImmutableList<IManagedItem> origin)
        {
            ImmutableList<IManagedItem> ret = [.. origin];

            foreach (IManagedItem mi in ret)
                if (mi is CloneGroupValue gv)
                    gv.Chirdren = DeepCloneManagedItems(gv.Chirdren);

            return ret;
        }

        private void FindOpendGroup()
        {
            _openeds.Clear();
            List<CloneGroupValue> gs1 = [];
            List<CloneGroupValue> gs2 = [];

            foreach (IManagedItem mi in ManagedItems)
                if (mi is CloneGroupValue gv)
                    gs1.Add(gv);

            while (gs1.Count > 0)
            {
                foreach (CloneGroupValue gs in gs1)
                {
                    if (gs.IsOpened)
                    {
                        _openeds.Add(gs);

                        foreach (IManagedItem mi in gs.Chirdren)
                            if (mi is CloneGroupValue gv)
                                gs2.Add(gv);
                    }
                }

                gs1 = gs2;
                gs2 = [];
            }
        }

        public void UpdateSource()
        {
            Source = [.. GetVisibleItem(ManagedItems, 0)];
        }

        public static List<IManagedItem> GetVisibleItem(ImmutableList<IManagedItem> items, int depth)
        {
            List<IManagedItem> visibles = [];

            foreach (IManagedItem item in items)
            {
                item.Depth = depth;

                if (item is CloneValue cv)
                {
                    visibles.Add(cv);
                }
                else
                {
                    CloneGroupValue gv = (CloneGroupValue)item;
                    visibles.Add(gv);
                    if (gv.IsOpened)
                        visibles.AddRange(GetVisibleItem(gv.Chirdren, depth + 1));
                }
            }

            return visibles;
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

        public void UpdateSelected(IEnumerable<IManagedItem> items)
        {
            Selecteds = [.. items];
        }

        public CloneGroupValue? FindParent(IManagedItem target)
        {
            if (ManagedItems.Contains(target))
                return null;

            List<CloneGroupValue> gs1 = [];
            List<CloneGroupValue> gs2 = [];

            foreach (IManagedItem mi in ManagedItems)
                if (mi is CloneGroupValue gv)
                    gs1.Add(gv);

            while (gs1.Count > 0)
            {
                foreach (CloneGroupValue gv1 in gs1)
                {
                    if (gv1.Chirdren.Contains(target))
                        return gv1;

                    foreach (IManagedItem mi in gv1.Chirdren)
                        if (mi is CloneGroupValue gv2)
                            gs2.Add(gv2);
                }

                gs1 = gs2;
                gs2 = [];
            }

            return null;
        }

        public void InsertItems(IEnumerable<IManagedItem> items)
        {
            if (SelectedIndex < -1 || SelectedIndex >= Source.Count)
                return;

            IManagedItem first = items.First();
            int index = SelectedIndex;

            BeginEdit?.Invoke(this, EventArgs.Empty);

            if (index < 0)
            {
                ManagedItems = ManagedItems.AddRange(items);
            }
            else if (Source[index] is CloneGroupValue gv2 && gv2.Chirdren.Count == 0)
            {
                gv2.Chirdren = gv2.Chirdren.AddRange(items);
            }
            else
            {
                IManagedItem target = Source[index];

                if (FindParent(target) is CloneGroupValue gv)
                {
                    int index2 = gv.Chirdren.IndexOf(target);

                    if (index2 + 1 == gv.Chirdren.Count)
                        gv.Chirdren = gv.Chirdren.AddRange(items);
                    else
                        gv.Chirdren = gv.Chirdren.InsertRange(index2 + 1, items);
                }
                else
                {
                    int index2 = ManagedItems.IndexOf(target);
                    if (index2 + 1 == ManagedItems.Count)
                        ManagedItems = ManagedItems.AddRange(items);
                    else
                        ManagedItems = ManagedItems.InsertRange(index2 + 1, items);
                }
            }

            SetProperties();
            UpdateSource();
            SelectedIndex = Source.IndexOf(first);
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveItems(IEnumerable<IManagedItem> items)
        {
            int index = SelectedIndex;

            BeginEdit?.Invoke(this, EventArgs.Empty);

            foreach (IManagedItem item in items)
            {
                if (FindParent(item) is CloneGroupValue gv)
                    gv.Chirdren = gv.Chirdren.Remove(item);
                else
                    ManagedItems = ManagedItems.Remove(item);
            }

            SetProperties();
            UpdateSource();
            SelectedIndex = (index < Source.Count - 1) ? index : -1;
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public bool CanMoveUpItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (FindParent(target) is CloneGroupValue gv)
            {
                int index2 = gv.Chirdren.IndexOf(target);

                if (index2 < 1)
                    return false;
            }
            else
            {
                int index2 = ManagedItems.IndexOf(target);

                if (index2 < 1)
                    return false;
            }

            return true;
        }

        public void MoveUpItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (FindParent(target) is CloneGroupValue gv)
            {
                int index2 = gv.Chirdren.IndexOf(target);

                if (index2 < 1)
                    return;

                BeginEdit?.Invoke(this, EventArgs.Empty);
                gv.Chirdren = gv.Chirdren
                    .Remove(target)
                    .Insert(index2 - 1, target);
            }
            else
            {
                int index2 = ManagedItems.IndexOf(target);

                if (index2 < 1)
                    return;

                BeginEdit?.Invoke(this, EventArgs.Empty);
                ManagedItems = ManagedItems
                    .Remove(target)
                    .Insert(index2 - 1, target);
            }

            SetProperties();
            UpdateSource();
            SelectedIndex = Source.IndexOf(target);
            _view.UpdateButtons();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public bool CanMoveDownItem()
        {

            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (FindParent(target) is CloneGroupValue gv)
            {
                int index2 = gv.Chirdren.IndexOf(target);

                if (index2 > gv.Chirdren.Count - 2)
                    return false;
            }
            else
            {
                int index2 = ManagedItems.IndexOf(target);

                if (index2 > ManagedItems.Count - 2)
                    return false;
            }

            return true;
        }

        public void MoveDownItem()
        {
            int index = SelectedIndex;
            IManagedItem target = Source[index];

            if (FindParent(target) is CloneGroupValue gv)
            {
                int index2 = gv.Chirdren.IndexOf(target);

                if (index2 > gv.Chirdren.Count - 2)
                    return;

                BeginEdit?.Invoke(this, EventArgs.Empty);
                if (index2 < gv.Chirdren.Count - 2)
                {
                    gv.Chirdren = gv.Chirdren
                        .Remove(target)
                        .Insert(index2 + 1, target);
                }
                else
                {
                    gv.Chirdren = gv.Chirdren
                        .Remove(target)
                        .Add(target);
                }
            }
            else
            {
                int index2 = ManagedItems.IndexOf(target);

                if (index2 > ManagedItems.Count - 2)
                    return;

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
            }

            SetProperties();
            UpdateSource();
            SelectedIndex = Source.IndexOf(target);
            _view.UpdateButtons();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
