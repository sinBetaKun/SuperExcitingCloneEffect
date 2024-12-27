using SuperExcitingCloneEffect.Infomations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.CloneController
{
    internal class CloneOrderChangerViewModel : Bindable, IPropertyEditorControl, IDisposable
    {
        readonly INotifyPropertyChanged item;
        readonly ItemProperty[] properties;
        static CloneBlock? clipedBlock = null;

        ImmutableList<CloneBlock> clones = [];

        public event EventHandler? BeginEdit;
        public event EventHandler? EndEdit;

        public ImmutableList<CloneBlock> Clones { get => clones; set => Set(ref clones, value); }
        public int SelectedCloneIndex
        {
            get => selectedCloneIndex;
            set
            {
                SomeBlockSelected = value > -1;
                Set(ref selectedCloneIndex, value);
            }
        }
        int selectedCloneIndex = -1;

        /// <summary>
        /// リスト内のいずれかのクローンブロックが選択されている状態か否か
        /// </summary>
        public bool SomeBlockSelected { get => someBlockSelected; set => Set(ref someBlockSelected, value); }
        bool someBlockSelected = false;

        /// <summary>
        /// 右クリックメニューで「切り取り」を選択したときの処理
        /// </summary>
        public void CutFunc()
        {
            BeginEdit?.Invoke(this, EventArgs.Empty);
            CopyFunc();
            RemoveCloneBlock();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 右クリックメニューで「コピー」を選択したときの処理
        /// </summary>
        public void CopyFunc()
        {
            clipedBlock = new(Clones[SelectedCloneIndex]);
        }

        /// <summary>
        /// 右クリックメニューで「貼り付け」が選択可能か否か
        /// </summary>
        public bool PasteEnable { get => pasteEnable; set => Set(ref pasteEnable, value); }
        bool pasteEnable = false;

        /// <summary>
        /// 右クリックメニューで「貼り付け」を選択したときの処理
        /// </summary>
        public void PasteFunc()
        {
            if (clipedBlock == null)
            {
                string className = GetType().Name;
                string? mthName = MethodBase.GetCurrentMethod()?.Name;
                SuperExcitingCloneDialog.ShowError("疑似クリップボードにnullが代入されている状態での貼り付け処理は、本来なら不可能な処理です。",
                    className, mthName);
                return;
            }

            var tmpSelectedCloneIndex = SelectedCloneIndex;
            BeginEdit?.Invoke(this, EventArgs.Empty);
            if (tmpSelectedCloneIndex < 0)
            {
                Clones = Clones.Add(new CloneBlock(clipedBlock));
                tmpSelectedCloneIndex = Clones.Count - 1;
            }
            else
            {
                Clones = Clones.Insert(tmpSelectedCloneIndex, new CloneBlock(clipedBlock));
            }
            SetProperties();
            SelectedCloneIndex = tmpSelectedCloneIndex;

            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 右クリックメニューで「複製」を選択したときの処理
        /// </summary>
        public void DuplicationFunc()
        {
            BeginEdit?.Invoke(this, EventArgs.Empty);
            var tmpSelectedCloneIndex = SelectedCloneIndex;
            var copied = new CloneBlock(Clones[SelectedCloneIndex]);
            Clones = Clones.Insert(tmpSelectedCloneIndex, new CloneBlock(copied));
            SetProperties();
            SelectedCloneIndex = tmpSelectedCloneIndex + 1;
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 右クリックメニューで「削除」を選択したときの処理
        /// </summary>
        public void RemoveFunc()
        {
            BeginEdit?.Invoke(this, EventArgs.Empty);
            RemoveCloneBlock();
            EndEdit?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 右クリックメニューが開かれている状態か否か
        /// </summary>
        public bool ContextMenuIsOpen
        {
            get => contextMeneIsOpen;
            set
            {
                PasteEnable = clipedBlock != null;
                Set(ref contextMeneIsOpen, value);
            }
        }
        bool contextMeneIsOpen = false;

        public ActionCommand AddCommand { get; }
        public ActionCommand RemoveCommand { get; }
        public ActionCommand MoveUpCommand { get; }
        public ActionCommand MoveDownCommand { get; }

        public CloneOrderChangerViewModel(ItemProperty[] properties)
        {
            this.properties = properties;

            item = (INotifyPropertyChanged)properties[0].PropertyOwner;
            item.PropertyChanged += Item_PropertyChanged;

            AddCommand = new ActionCommand(
                _ => true,
                _ =>
                {
                    var tmpSelectedCloneIndex = SelectedCloneIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    Clones = Clones.Insert(tmpSelectedCloneIndex + 1, new CloneBlock());
                    SetProperties();
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedCloneIndex = tmpSelectedCloneIndex + 1;
                });

            RemoveCommand = new ActionCommand(
                _ => SelectedCloneIndex > -1,
                _ =>
                {
                    var tmpSelectedCloneIndex = SelectedCloneIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    RemoveCloneBlock();
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedCloneIndex = Math.Min(tmpSelectedCloneIndex, clones.Count - 1);
                });

            MoveUpCommand = new ActionCommand(
                _ => SelectedCloneIndex > 0,
                _ =>
                {
                    var tmpSelectedCloneIndex = SelectedCloneIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clone = Clones[SelectedCloneIndex];
                    Clones = Clones.RemoveAt(tmpSelectedCloneIndex).Insert(tmpSelectedCloneIndex - 1, clone);
                    SetProperties();
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedCloneIndex = tmpSelectedCloneIndex - 1;
                });

            MoveDownCommand = new ActionCommand(
                _ => SelectedCloneIndex < clones.Count - 1 && SelectedCloneIndex > -1,
                _ =>
                {
                    var tmpSelectedCloneIndex = SelectedCloneIndex;
                    BeginEdit?.Invoke(this, EventArgs.Empty);
                    var clone = clones[SelectedCloneIndex];
                    Clones = Clones.RemoveAt(tmpSelectedCloneIndex).Insert(tmpSelectedCloneIndex + 1, clone);
                    SetProperties();
                    EndEdit?.Invoke(this, EventArgs.Empty);
                    SelectedCloneIndex = tmpSelectedCloneIndex + 1;
                });

            UpdateClones();
        }

        private void RemoveCloneBlock()
        {
            var tmpSelectedCloneIndex = SelectedCloneIndex;
            Clones = Clones.RemoveAt(tmpSelectedCloneIndex);
            SetProperties();
            if (Clones.Count > 0) SelectedCloneIndex = Math.Min(tmpSelectedCloneIndex, Clones.Count - 1);
            else SelectedCloneIndex = -1;
        }

        private void SetProperties()
        {
            foreach (var property in properties)
                property.SetValue(Clones.Select(x=>new CloneBlock(x)).ToImmutableList());
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == properties[0].PropertyInfo.Name)
                UpdateClones();
        }

        void UpdateClones()
        {
            var values = properties[0].GetValue<ImmutableList<CloneBlock>>() ?? [];
            if (!Clones.SequenceEqual(values))
            {
                Clones = [.. values];
            }

            SomeBlockSelected = !Clones.IsEmpty;

            var commands = new[] { AddCommand, RemoveCommand, MoveUpCommand, MoveDownCommand };
            foreach (var command in commands)
                command.RaiseCanExecuteChanged();
        }

        public void CopyToOtherItems()
        {
            //現在のアイテムの内容を他のアイテムにコピーする
            var otherProperties = properties.Skip(1);
            foreach (var property in otherProperties)
                property.SetValue(Clones.Select(x => new CloneBlock(x)).ToImmutableList());
        }

        public void Dispose()
        {
            item.PropertyChanged -= Item_PropertyChanged;
        }
    }
}
