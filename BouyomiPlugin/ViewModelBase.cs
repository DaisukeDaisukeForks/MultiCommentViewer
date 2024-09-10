﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BouyomiPlugin
{
    public class ViewModelBase
    {
        #region INotifyPropertyChanged
        [NonSerialized]
        private PropertyChangedEventHandler? _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
