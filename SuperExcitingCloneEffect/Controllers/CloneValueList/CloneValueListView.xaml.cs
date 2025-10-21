using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using System.Collections.ObjectModel;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private List<IManagedItem> _clipboad = [];

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

            if (selecteds.Count > 1)
            {
                AddButton.IsEnabled = false;
                GroupAddButton.IsEnabled = false;
                RemoveButton.IsEnabled = true;
                UpButton.IsEnabled = false;
                DownButton.IsEnabled = false;
                CutButton.IsEnabled = true;
                CopyButton.IsEnabled = true;
                PasteButton.IsEnabled = false;
                DuplicateButton.IsEnabled = false;
            }
            else
            {
                AddButton.IsEnabled = true;
                GroupAddButton.IsEnabled = true;
                PasteButton.IsEnabled = _clipboad.Count > 0;

                if (selecteds.Count < 1)
                {
                    RemoveButton.IsEnabled = false;
                    UpButton.IsEnabled = false;
                    DownButton.IsEnabled = false;
                    CutButton.IsEnabled = false;
                    CopyButton.IsEnabled = false;
                    DuplicateButton.IsEnabled = false;
                }
                else
                {
                    RemoveButton.IsEnabled = true;
                    CutButton.IsEnabled = true;
                    CopyButton.IsEnabled = true;
                    DuplicateButton.IsEnabled = true;

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

        /// <summary>
        /// このプラグインでは使われる場面がない。
        /// STP に移植するときのためのもの。
        /// </summary>
        private void List_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = FindVisualChild<ScrollViewer>(ItemList);
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

        /// <summary>
        /// このプラグインでは使われる場面がない。
        /// STP に移植するときのためのもの。
        /// </summary>
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t)
                    return t;
                else
                {
                    T? result = FindVisualChild<T>(child);

                    if (result != null)
                        return result;
                }
            }
            return null;
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

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            _clipboad = GetCloneOfSelected();
            vm.RemoveItems(GetSelecteds());
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            _clipboad = GetCloneOfSelected();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.InsertItems(GetCloneOfClipboad());
        }

        private void DuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneValueListViewModel vm)
                return;

            vm.InsertItems(GetCloneOfSelected());
        }

        private List<IManagedItem> GetCloneOfSelected()
        {
            if (DataContext is not CloneValueListViewModel vm)
                return [];

            HashSet<IManagedItem> hash = [];

            foreach (IManagedItem mi1 in GetSelecteds())
            {
                List<IManagedItem> list1 = [mi1];
                List<IManagedItem> list2 = [];

                while (list1.Count > 0)
                {
                    foreach (IManagedItem mi2 in list1)
                    {
                        if (mi2 is CloneGroupValue gv)
                            list2.AddRange(vm.FindChildren(gv));

                        hash.Add(mi2);
                    }

                    list1 = list2;
                    list2 = [];
                }
            }

            CloneTreeNode tn = new(hash);

            return tn.GetCloneTree().ToList();
        }

        private List<IManagedItem> GetCloneOfClipboad()
        {
            CloneTreeNode tn = new(_clipboad);

            return tn.GetCloneTree().ToList();
        }
    }
}
