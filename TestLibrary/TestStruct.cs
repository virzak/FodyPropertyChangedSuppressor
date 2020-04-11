using PropertyChanged;

namespace TestLibrary
{
    [AddINotifyPropertyChangedInterface]
    internal partial class TestStruct
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

        private void UnusedProp()
        {
        }

        int OnFieldChanged;
        int Field;

        public int PropInSamePartialClass { get; set; }
    }

    internal partial class TestStruct
    {

        public int PropInOtherPartialClass { get; set; }
    }
}
