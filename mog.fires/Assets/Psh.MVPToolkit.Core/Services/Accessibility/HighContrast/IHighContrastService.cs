namespace Psh.MVPToolkit.Core.Services.Accessibility.HighContrast
{
    public interface IHighContrastService 
    {

        bool ContrastEnabled { get; }

        void RegisterHighContrastObject(IContrastable obj);
        void UnregisterHighContrastObject(IContrastable obj);
        void SwitchContrast(bool enabled);
    }
}