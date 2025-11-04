namespace Artigio.MVVMToolkit.Core.MVVM.Base
{
    public interface IViewTypeProvider<out T>
    {
        T GetViewType();
    }
}