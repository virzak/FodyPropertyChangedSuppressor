using System.ComponentModel;

namespace TestLibrary
{
    class Base1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }

    class Base2 : Base1
    {
    }

    class Derived : Base2
    {
        public int Property { get; set; }

        private void OnPropertyChanged()
        {
        }

    }

}
