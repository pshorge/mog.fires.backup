using System;

namespace Psh.MVPToolkit.Core.MVP.Contracts
{
    /// <summary>
    /// Interface for MVP Presenters handling business logic
    /// </summary>
    public interface IPresenter : IDisposable 
    {
        void Initialize();
        void RefreshData();
    }
}