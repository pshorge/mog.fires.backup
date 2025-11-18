using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace Psh.MVPToolkit.Core.MVP.Base
{
    /// <summary>
    /// Base class for bindable data sources in MVP pattern
    /// Used by Presenters to provide bindable properties to Views
    /// </summary>
    public abstract class BaseDataSource : INotifyBindablePropertyChanged, IDisposable
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