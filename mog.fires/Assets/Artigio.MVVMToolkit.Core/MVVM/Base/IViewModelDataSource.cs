
using System;

namespace Artigio.MVVMToolkit.Core.MVVM.Base
{
   
    public interface IViewModelDataSource : IDisposable 
    {
        void FetchModel();
    }
}