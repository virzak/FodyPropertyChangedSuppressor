using PropertyChanged;

namespace TestLibrary
{
    [AddINotifyPropertyChangedInterface]
    internal partial class TestClass
    {

        private void OnPropInSamePartialClassChanged()
        {
        }

        private void OnPropInOtherFileChanged()
        {
        }

        private void OnPropInOtherPartialClassChanged()
        {
        }

        private void OnPropDoesntExistChanged()
        {
        }

        // with parameter. No suppression here
        private void OnPropInSamePartialClassChanged(int _)
        {
        }

        private void UnusedProp()
        {
        }

        int OnFieldChanged;
        int Field;

        public int PropInSamePartialClass { get; set; }
    }

    internal partial class TestClass
    {

        public int PropInOtherPartialClass { get; set; }
    }
}
