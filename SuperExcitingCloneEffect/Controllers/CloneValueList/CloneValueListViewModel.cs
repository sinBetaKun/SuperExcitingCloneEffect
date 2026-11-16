using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using SuperExcitingCloneEffect.Properties;
using System.Collections.Immutable;
using System.ComponentModel;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    internal class CloneValueListViewModel : Bindable, IPropertyEditorControl, IDisposable
    {
        private readonly INotifyPropertyChanged _item;
        private readonly ItemProperty[] _properties;

        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public List<IManagedItem> ManagedItems { get; set => Set(ref field, value); } = [];

        public List<IManagedItem> Source { get;  set => Set(ref field, value); } = [];

        public CloneValueListViewModel(ItemProperty[] properties)
        {
            _properties = properties;

            _item = (INotifyPropertyChanged)properties[0].PropertyOwner;
            _item.PropertyChanged += Item_PropertyChanged;

            UpdateClones();
        }

        public void CopyToOtherItems()
        {
            var otherProperties = _properties.Skip(1);
            foreach (var property in otherProperties)
                property.SetValue(DeepCloneManagedItems(ManagedItems).ToImmutableList());
        }

        private static List<IManagedItem> DeepCloneManagedItems(IEnumerable<IManagedItem> origin)
        {
            List<IManagedItem> mi_l0 = [];

            foreach (IManagedItem mi in origin)
            {
                if (mi is CloneGroupValue gv)
                    mi_l0.Add(new CloneGroupValue(gv));
                else
                    mi_l0.Add(new CloneValue((CloneValue)mi));
            }

            return mi_l0;
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
                ManagedItems = [.. list];
            }

            UpdateSource();
        }

        public void SetProperties()
        {
            foreach (var property in _properties)
                property.SetValue(ManagedItems.ToImmutableList());
        }

        public void UpdateSource()
        {
            Source = [.. ItemTreeNode.GetVisibleList(ItemTreeNode.GetTree(ManagedItems))];
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
        }

        public List<IManagedItem> GetDescendants(IManagedItem mi1)
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

                if (index2 == ManagedItems.Count)
                    break;
            }

            return list1;
        }

        private (List<ItemTreeNode>, List<ItemTreeNode>) FindSelectedNodes(List<IManagedItem> items)
        {
            List<ItemTreeNode> n_l0 = ItemTreeNode.GetTree(ManagedItems);
            List<ItemTreeNode> n_l1 = [];

            foreach (IManagedItem mi_0 in items)
            {
                if (n_l0.FirstOrDefault(x => x.Item == mi_0) is ItemTreeNode n_0)
                    n_l1.Add(n_0);
                else
                    foreach (ItemTreeNode n_1 in n_l0)
                        if (n_1.Find(mi_0) is ItemTreeNode n_2)
                            n_l1.Add(n_2);
            }

            return (n_l0, [.. n_l1.OrderBy(x => ManagedItems.IndexOf(x.Item))]);
        }

        public void InsertItems(List<IManagedItem> items, int selectedIndex)
        {
            if (selectedIndex < -1 || selectedIndex >= Source.Count)
                return;

            List<ItemTreeNode> n_l0 = ItemTreeNode.GetTree(ManagedItems);
            List<ItemTreeNode> n_l1 = ItemTreeNode.GetTree(items);

            if (selectedIndex < 0)
            {
                n_l0.AddRange(n_l1);
            }
            else
            {
                IManagedItem mi_0 = Source[selectedIndex];

                if (n_l0.FirstOrDefault(x => x.Item == mi_0) is ItemTreeNode n_0)
                {
                    if (n_0.InsertToHere(n_l1))
                    {
                        if (n_l0.Last() == n_0)
                        {
                            n_l0.AddRange(n_l1);
                        }
                        else
                        {
                            int i_0 = n_l0.IndexOf(n_0);
                            n_l0.InsertRange(i_0 + 1, n_l1);
                        }
                    }
                }
                else
                {
                    ItemTreeNode? n_1 = null;

                    foreach (ItemTreeNode n_2 in n_l0)
                    {
                        if (n_2.Find(mi_0) is ItemTreeNode n_3)
                        {
                            n_1 = n_3;
                            break;
                        }
                    }

                    if (n_1 is null)
                    {
                        SuperExcitingCloneDialog.ShowError(new(TextResource.CloneValueListViewModel_Error_FailToGenerateValueTree_0));
                        throw new Exception(TextResource.CloneValueListViewModel_Error_FailToGenerateValueTree_0);
                    }

                    if (n_1.InsertToHere(n_l1))
                    {
                        SuperExcitingCloneDialog.ShowError(new(TextResource.CloneValueListViewModel_Error_FailToGenerateValueTree_1));
                        throw new Exception(TextResource.CloneValueListViewModel_Error_FailToGenerateValueTree_1);
                    }
                }
            }

            BeginEdit?.Invoke(this, EventArgs.Empty);
            List<IManagedItem> mi_l0 = ItemTreeNode.GetList(n_l0);
            ManagedItems = mi_l0;
            UpdateSource();
            SetProperties();
            EndEdit?.Invoke(this, EventArgs.Empty);
            return;
        }

        public void RemoveItems(IEnumerable<IManagedItem> items)
        {
            (List<ItemTreeNode> n_l0, List<ItemTreeNode> n_l1) = FindSelectedNodes([.. items]);

            foreach (ItemTreeNode n_0 in n_l1)
            {
                if (n_0.Parent is ItemTreeNode n_1)
                {
                    n_1.Children.Remove(n_0);
                }
                else
                {
                    n_l0.Remove(n_0);
                }
            }

            BeginEdit?.Invoke(this, EventArgs.Empty);
            ManagedItems = ItemTreeNode.GetList(n_l0);
            UpdateSource();
            SetProperties();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }


        public ItemMovable CanMoveItem(IEnumerable<IManagedItem> items)
        {
            (List<ItemTreeNode> n_l0, List<ItemTreeNode> n_l1) = FindSelectedNodes([.. items]);

            if (n_l1.Count == 0)
                return ItemMovable.Neither;

            if (n_l1.Select(x => x.Parent).Distinct().Count() > 1)
                return ItemMovable.Hazard;

            List<ItemTreeNode> n_l2 = n_l1.First().Parent?.Children ?? n_l0;

            if (n_l1.Contains(n_l2.First()))
            {
                if (n_l1.Contains(n_l2.Last()))
                {
                    return ItemMovable.Neither;
                }
                else
                {
                    return ItemMovable.OnlyDown;
                }
            }
            else
            {
                if (n_l1.Contains(n_l2.Last()))
                {
                    return ItemMovable.OnlyUp;
                }
                else
                {
                    return ItemMovable.Both;
                }
            }
        }

        public void MoveUpItems(List<IManagedItem> items)
        {
            (List<ItemTreeNode> n_l0, List<ItemTreeNode> n_l1) = FindSelectedNodes([.. items]);
            List<ItemTreeNode> n_l2 = n_l1.First().Parent?.Children ?? n_l0;

            for (int i = 1; i < n_l2.Count && 0 < n_l1.Count; i++)
            {
                if (n_l2[i] == n_l1.First())
                {
                    (n_l2[i - 1], n_l2[i]) = (n_l2[i], n_l2[i - 1]);
                    n_l1.RemoveAt(0);
                }
            }

            BeginEdit?.Invoke(this, EventArgs.Empty);
            List<IManagedItem> mi_l0 = ItemTreeNode.GetList(n_l0);
            ManagedItems = mi_l0;
            UpdateSource();
            SetProperties();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void MoveDownItems(List<IManagedItem> items)
        {
            (List<ItemTreeNode> n_l0, List<ItemTreeNode> n_l1) = FindSelectedNodes([.. items]);
            List<ItemTreeNode> n_l2 = n_l1.First().Parent?.Children ?? n_l0;

            for (int i = n_l2.Count - 2; i >= 0 && 0 < n_l1.Count; i--)
            {
                if (n_l2[i] == n_l1.Last())
                {
                    (n_l2[i + 1], n_l2[i]) = (n_l2[i], n_l2[i + 1]);
                    n_l1.RemoveAt(n_l1.Count - 1);
                }
            }

            BeginEdit?.Invoke(this, EventArgs.Empty);
            List<IManagedItem> mi_l0 = ItemTreeNode.GetList(n_l0);
            ManagedItems = mi_l0;
            UpdateSource();
            SetProperties();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
