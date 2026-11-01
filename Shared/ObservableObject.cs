using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shared
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        protected void SetField<T>(ref T _backingField, T value, [CallerMemberName]string propName = "")
        {
            if (propName == string.Empty)
            {
                throw new InvalidOperationException("SetField should be invoked only from within the property's setter.");
            }

            if (_backingField == null || !_backingField.Equals(value))
            {
                _backingField = value;
                RaisePropertyChanged(propName);
            }
        }
    }
}
