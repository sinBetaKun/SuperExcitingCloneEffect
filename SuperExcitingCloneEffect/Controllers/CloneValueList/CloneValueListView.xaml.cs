using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public CloneValueListView()
        {
            InitializeComponent();
            DataContextChanged += PointsEditor_DataContextChanged;
        }

        private void PointsEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateButtons();
        }

        public void UpdateButtons()
        {

            if (DataContext is not CloneValueListViewModel vm)
                return;

            List<IManagedItem> selecteds = GetSelecteds();
            vm.UpdateSelected(selecteds);

            if (selecteds.Count > 1)
            {
                AddButton.IsEnabled = false;
                GroupAddButton.IsEnabled = false;
                RemoveButton.IsEnabled = true;
                UpButton.IsEnabled = false;
                DownButton.IsEnabled = false;
            }
            else
            {
                AddButton.IsEnabled = true;
                GroupAddButton.IsEnabled = true;

                if (selecteds.Count < 1)
                {
                    RemoveButton.IsEnabled = false;
                    UpButton.IsEnabled = false;
                    DownButton.IsEnabled = false;
                }
                else
                {
                    RemoveButton.IsEnabled = true;

                    if (vm.CanMoveUpItem())
                        UpButton.IsEnabled = true;
                    else
                        UpButton.IsEnabled = false;

                    if (vm.CanMoveDownItem())
                        DownButton.IsEnabled = true;
                    else
                        DownButton.IsEnabled = false;
                }
            }
        }

        public void SetEditorInfo(IEditorInfo info)
        {
            propertiesEditor.SetEditorInfo(info);
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

        private void List_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = CloneValueListViewModel.FindVisualChild<ScrollViewer>(ItemList);
            if (scrollViewer == null) return;

            e.Handled = true;
            bool scrollingUp = e.Delta > 0;

            if ((scrollingUp && scrollViewer.VerticalOffset == 0) ||
                (!scrollingUp && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight))
            {
                // 端に到達 → スクロールイベントを親に渡す

                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };

                // 親要素を取得してイベント再発火
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
            else
            {
                // まだスクロール可能 → 自分で処理
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            }
        }

        private void ManagedItemView_GroupOpened(object sender, EventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            if (sender is not ManagedItemView miv)
                return;

            if (miv.DataContext is not CloneGroupValue gv)
                return;

            vm.AddOpenedGroup(gv);
        }

        private void ManagedItemView_GroupClosed(object sender, EventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            if (sender is not ManagedItemView miv)
                return;

            if (miv.DataContext is not CloneGroupValue gv)
                return;

            vm.RemoveOpenedGroup(gv);
        }

        private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
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

            vm.InsertItems([new CloneValue()]);
        }

        private void GroupAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.InsertItems([new CloneGroupValue()]);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.RemoveItems(GetSelecteds());
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.MoveUpItem();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.MoveDownItem();
        }
    }
}
