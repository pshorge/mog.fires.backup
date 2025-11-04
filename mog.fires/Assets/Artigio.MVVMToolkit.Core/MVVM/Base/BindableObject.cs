using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace Artigio.MVVMToolkit.Core.MVVM.Base
{
 
    public abstract class BindableObject : INotifyBindablePropertyChanged, IDisposable
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        private bool _disposed = false;

        protected void Notify([CallerMemberName] string propertyName = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }
        
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}