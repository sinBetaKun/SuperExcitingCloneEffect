using SuperExcitingCloneEffect.Classes;
using SuperExcitingCloneEffect.Interfaces;
using System.Windows;
using System.Windows.Controls;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    /// <summary>
    /// ManagedItemView.xaml の相互作用ロジック
    /// </summary>
    public partial class ManagedItemView : UserControl, IPropertyEditorControl
    {
        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public ManagedItemView()
        {
            InitializeComponent();
        }

        private void Switch_Hide(object sender, RoutedEventArgs e)
        {
            if (DataContext is IManagedItem mi)
            {
                BeginEdit?.Invoke(this, e);
                mi.Hide = !mi.Hide;
                EndEdit?.Invoke(this, e);
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(ManagedItemView),
                new PropertyMetadata(false, OnStatusChanged));


        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ManagedItemView control)
            {
                control.UpdateUIStyle();
            }
        }

        private void UpdateUIStyle()
        {
            if (IsSelected)
                SelectedSign.Visibility = Visibility.Visible;
            else
                SelectedSign.Visibility = Visibility.Hidden;
        }

        public event EventHandler? GroupOpened;
        public event EventHandler? GroupClosed;

        private void GroupViewChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CloneGroupValue gv)
                return;

            gv.IsOpened = !gv.IsOpened;

            if (gv.IsOpened)
            {
                GroupOpened?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GroupClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is not CloneGroupValue gv)
                return;

            if (gv.IsOpened)
            {
                OpenedSign.Visibility = Visibility.Visible;
                ClosedSign.Visibility = Visibility.Hidden;
            }
            else
            {
                OpenedSign.Visibility = Visibility.Hidden;
                ClosedSign.Visibility = Visibility.Visible;
            }
        }
    }
}
