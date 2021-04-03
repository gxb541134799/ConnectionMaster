using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectionManster.UI.PC
{
    public class Checkable<T> : ObservableObject
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetValue(ref _isChecked, value, nameof(IsChecked)); }
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set { SetValue(ref _value, value, nameof(Value)); }
        }

        public Checkable(T value, bool isChecked = false)
        {
            Value = value;
            IsChecked = isChecked;
        }

        public event EventHandler IsCheckedChanged;

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(IsChecked))
            {
                IsCheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
