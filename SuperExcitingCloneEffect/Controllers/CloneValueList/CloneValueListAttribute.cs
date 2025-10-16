using System.Windows;
using YukkuriMovieMaker.Commons;

namespace SuperExcitingCloneEffect.Controllers.CloneValueList
{
    internal class CloneValueListAttribute : PropertyEditorAttribute2
    {
        public override FrameworkElement Create()
        {
            return new CloneValueListView();
        }

        public override void SetBindings(FrameworkElement control, ItemProperty[] itemProperties)
        {
            if (control is not CloneValueListView editor)
                return;

            editor.DataContext = new CloneValueListViewModel(itemProperties, editor);
        }

        public override void ClearBindings(FrameworkElement control)
        {
            if (control is not CloneValueListView editor)
                return;

            if (editor.DataContext is CloneValueListViewModel vm)
                vm.Dispose();

            editor.DataContext = null;
        }
    }
}
