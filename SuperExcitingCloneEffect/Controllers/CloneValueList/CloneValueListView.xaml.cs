using Newtonsoft.Json;
using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using SuperExcitingCloneEffect.Properties;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    /// <summary>
    /// CloneValueListView.xaml の相互作用ロジック
    /// </summary>
    public partial class CloneValueListView : UserControl, IPropertyEditorControl2
    {
        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        private static string _clipboad = "[]";

        private static JsonSerializerSettings GetJsonSetting =>
            new() { TypeNameHandling = TypeNameHandling.Auto };

        private static List<IManagedItem> GetClipped()
        {
            if (JsonConvert.DeserializeObject<IEnumerable<IManagedItem>>(_clipboad, GetJsonSetting) is List<IManagedItem> pv_e0)
                return pv_e0;

            SuperExcitingCloneDialog.ShowWarning(TextResource.ErrorMessage_FailToGetClippedFromJson);
            return [];
        }

        private static void SetClipped(IEnumerable<IManagedItem> pvs)
        {
            _clipboad = JsonConvert.SerializeObject(pvs, Newtonsoft.Json.Formatting.Indented, GetJsonSetting);
        }

        public CloneValueListView()
        {
            InitializeComponent();
            DataContextChanged += CloneValueListView_DataContextChanged;
        }

        private void CloneValueListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CloneValueListViewModel oldVm)
            {
                oldVm.BeginEdit -= PropertiesEditor_BeginEdit;
                oldVm.EndEdit -= PropertiesEditor_EndEdit;
            }
            if (e.NewValue is CloneValueListViewModel newVm)
            {
                newVm.BeginEdit += PropertiesEditor_BeginEdit;
                newVm.EndEdit += PropertiesEditor_EndEdit;
            }

            UpdateButtons();
        }

        public void UpdateButtons()
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            List<IManagedItem> selecteds = GetSelecteds();

            ItemMovable cmv = vm.CanMoveItem(selecteds);
            UpButton.IsEnabled = cmv == ItemMovable.OnlyUp || cmv == ItemMovable.Both;
            DownButton.IsEnabled = cmv == ItemMovable.OnlyDown || cmv == ItemMovable.Both;

            if (cmv == ItemMovable.Hazard)
            {
                HazardIcon.ToolTip = TextResource.PartValueListView_WarningA;
                HazardIcon.Visibility = Visibility.Visible;
                DuplicateButton.IsEnabled = false;
            }
            else
            {
                HazardIcon.ToolTip = string.Empty;
                HazardIcon.Visibility = Visibility.Hidden;
                DuplicateButton.IsEnabled = true;
            }

            if (selecteds.Count > 1)
            {
                AddButton.IsEnabled = false;
                RemoveButton.IsEnabled = true;
                CutButton.IsEnabled = true;
                CopyButton.IsEnabled = true;
                PasteButton.IsEnabled = false;
            }
            else
            {
                AddButton.IsEnabled = true;
                IManagedItem[] pv_a0 = [.. GetClipped()];
                PasteButton.IsEnabled = pv_a0.Length > 0;

                if (selecteds.Count < 1)
                {
                    RemoveButton.IsEnabled = false;
                    CutButton.IsEnabled = false;
                    CopyButton.IsEnabled = false;
                }
                else
                {
                    RemoveButton.IsEnabled = true;
                    CutButton.IsEnabled = true;
                    CopyButton.IsEnabled = true;
                }
            }
        }

        public void SetEditorInfo(IEditorInfo? info)
        {
            propertiesEditor.SetEditorInfo(info);

            if (DataContext is CloneValueListViewModel vm)
                vm.UpdateSource();
        }

        private void PropertiesEditor_BeginEdit(object? sender, EventArgs e)
        {
            BeginEdit?.Invoke(this, e);
        }

        private void PropertiesEditor_EndEdit(object? sender, EventArgs e)
        {
            //Part内のAnimationを変更した際にPartsを更新する
            //複数のアイテムを選択している場合にすべてのアイテムを更新するために必要
            if (DataContext is CloneValueListViewModel vm)
                vm.CopyToOtherItems();

            EndEdit?.Invoke(this, e);
        }

        private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void SetSelecteds(List<IManagedItem> items)
        {
            ItemList.SelectedItems.Clear();
            items.ForEach(x => ItemList.SelectedItems.Add(x));

        }

        private void PartValueView_GroupPullDownChanged(object sender, EventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            if (sender is not CloneGroupValue)
                return;

            vm.UpdateSource();
        }

        public List<IManagedItem> GetSelecteds()
        {
            List<IManagedItem> selecteds = [];
            foreach (var selected in ItemList.SelectedItems)
                if (selected is IManagedItem item)
                    selecteds.Add(item);
            return selecteds;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            CloneValue cv_0 = new();
            vm.InsertItems([cv_0], ItemList.SelectedIndex);
            ItemList.SelectedItems.Clear();
            ItemList.SelectedItems.Add(cv_0);
        }

        private void GroupAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            CloneGroupValue gv_0 = new();
            vm.InsertItems([gv_0], ItemList.SelectedIndex);
            SetSelecteds([gv_0]);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.RemoveItems(GetSelecteds());
            ItemList.SelectedItems.Clear();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.MoveUpItems(GetSelecteds());
            UpdateButtons();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.MoveDownItems(GetSelecteds());
            UpdateButtons();
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            List<IManagedItem> mi_l0 = GetCloneOfSelected();
            SetClipped(mi_l0);
            vm.RemoveItems(GetSelecteds());
            ItemList.SelectedItems.Clear();
            UpdateButtons();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            List<IManagedItem> mi_l0 = GetCloneOfSelected();
            SetClipped(mi_l0);
            UpdateButtons();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            List<IManagedItem> mi_l0 = GetClipped();
            vm.InsertItems(mi_l0, ItemList.SelectedIndex);
            SetSelecteds(mi_l0);
            UpdateButtons();
        }

        private void DuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            List<IManagedItem> mi_l0 = GetCloneOfSelected();
            vm.InsertItems(mi_l0, GetSelecteds().Max(vm.Source.IndexOf));
            SetSelecteds(mi_l0);
            UpdateButtons();
        }

        private List<IManagedItem> GetCloneOfSelected()
        {
            if (DataContext is not CloneValueListViewModel vm)
                return [];

            List<IManagedItem> mi_l0 = [];
            IManagedItem? firstSelected = (IManagedItem)ItemList.SelectedItem;

            foreach (IManagedItem mi_0 in GetSelecteds())
            {
                List<IManagedItem> mi_l1 = [mi_0];
                List<IManagedItem> mi_l2 = [];

                while (mi_l1.Count > 0)
                {
                    foreach (IManagedItem mi_1 in mi_l1)
                    {
                        if (mi_1 is CloneGroupValue gv)
                            mi_l2.AddRange(vm.GetDescendants(gv));

                        mi_l0.Add(mi_1);
                    }

                    mi_l1 = mi_l2;
                    mi_l2 = [];
                }
            }

            List<IManagedItem> mi_l3 = [.. mi_l0.Distinct().OrderBy(vm.ManagedItems.IndexOf)];
            List<(IManagedItem, IManagedItem)> tp_l0 = [];

            foreach (IManagedItem mi_0 in mi_l3)
            {
                if (mi_0.ParentIndex < 0 || mi_0.ParentIndex >= vm.ManagedItems.Count)
                {
                    if (mi_0 is CloneGroupValue gv_0)
                    {
                        CloneGroupValue gv_1 = new(gv_0) { ParentIndex = -1 };
                        tp_l0.Add((mi_0, gv_1));
                    }
                    else
                    {
                        CloneValue cv = new((CloneValue)mi_0) { ParentIndex = -1 };
                        tp_l0.Add((mi_0, cv));
                    }
                }
                else
                {
                    if (tp_l0.Select(item => item.Item1).FirstOrDefault(x => x == vm.ManagedItems[mi_0.ParentIndex]) is IManagedItem mi_1)
                    {
                        int index = tp_l0.Select(item => item.Item1).ToList().IndexOf(mi_1);

                        if (mi_0 is CloneGroupValue gv_0)
                        {
                            CloneGroupValue gv_1 = new(gv_0) { ParentIndex = index };
                            tp_l0.Add((mi_0, gv_1));
                        }
                        else
                        {
                            CloneValue cv = new((CloneValue)mi_0) { ParentIndex = index };
                            tp_l0.Add((mi_0, cv));
                        }
                    }
                    else
                    {
                        if (mi_0 is CloneGroupValue gv_0)
                        {
                            CloneGroupValue gv2 = new(gv_0) { ParentIndex = -1 };
                            tp_l0.Add((mi_0, gv2));
                        }
                        else
                        {
                            CloneValue cv = new((CloneValue)mi_0) { ParentIndex = -1 };
                            tp_l0.Add((mi_0, cv));
                        }
                    }
                }
            }

            return [.. tp_l0.Select(item => item.Item2)];
        }
    }
}
