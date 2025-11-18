namespace Psh.MVPToolkit.Core.MVP.Contracts
{
    public interface IViewTypeProvider<out T>
    {
        T GetViewType();
    }
}