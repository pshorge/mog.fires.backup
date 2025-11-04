
using Cysharp.Threading.Tasks;

namespace Artigio.MVVMToolkit.Core.Navigation
{
    public interface INavigationStrategy<in TViewType> where TViewType : struct
    {
        bool CanExecute(INavigationState<TViewType> from, INavigationState<TViewType> to);
        UniTask ExecuteAsync(INavigationState<TViewType> from, INavigationState<TViewType> to);
    }
}