﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.CloneController
{
    /// <summary>
    /// CloneOrderChanger.xaml の相互作用ロジック
    /// </summary>
    public partial class CloneOrderChanger : System.Windows.Controls.UserControl, IPropertyEditorControl
    {
        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public CloneOrderChanger()
        {
            InitializeComponent();
            DataContextChanged += PointsEditor_DataContextChanged;
        }

        private void PointsEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CloneOrderChangerViewModel oldVm)
            {
                oldVm.BeginEdit -= PropertiesEditor_BeginEdit;
                oldVm.EndEdit -= PropertiesEditor_EndEdit;
            }
            if (e.NewValue is CloneOrderChangerViewModel newVm)
            {
                newVm.BeginEdit += PropertiesEditor_BeginEdit;
                newVm.EndEdit += PropertiesEditor_EndEdit;
            }
        }

        private void PropertiesEditor_BeginEdit(object? sender, EventArgs e)
        {
            BeginEdit?.Invoke(this, e);
        }

        private void PropertiesEditor_EndEdit(object? sender, EventArgs e)
        {
            //Point内のAnimationを変更した際にClonesを更新する
            //複数のアイテムを選択している場合にすべてのアイテムを更新するために必要
            var vm = DataContext as CloneOrderChangerViewModel;
            vm?.CopyToOtherItems();
            EndEdit?.Invoke(this, e);
        }

        private void Cut_Clicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is CloneOrderChangerViewModel viewModel)
            {
                viewModel.CutFunc();
            }
        }

        private void Copy_Clicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is CloneOrderChangerViewModel viewModel)
            {
                viewModel.CopyFunc();
            }
        }

        private void Paste_Clicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is CloneOrderChangerViewModel viewModel)
            {
                viewModel.PasteFunc();
            }
        }

        private void Duplication_Clicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is CloneOrderChangerViewModel viewModel)
            {
                viewModel.DuplicationFunc();
            }
        }

        private void Remove_Clicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is CloneOrderChangerViewModel viewModel)
            {
                viewModel.RemoveFunc();
            }
        }
    }
}
