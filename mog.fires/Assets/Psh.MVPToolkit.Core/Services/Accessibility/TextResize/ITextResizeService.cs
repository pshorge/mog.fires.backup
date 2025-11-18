namespace Psh.MVPToolkit.Core.Services.Accessibility.TextResize
{
    public interface ITextResizeService
    {
        bool Maximized { get; }
        void RegisterResizableTextObject(IResizable obj);
        void UnregisterResizableTextObject(IResizable obj);
        void Resize(bool maximized);
        
    }
}