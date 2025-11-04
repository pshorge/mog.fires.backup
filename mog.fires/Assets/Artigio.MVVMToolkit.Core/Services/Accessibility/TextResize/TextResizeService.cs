using System.Collections.Generic;

namespace Artigio.MVVMToolkit.Core.Services.Accessibility.TextResize
{
    public class TextResizeService : ITextResizeService
    {

        private HashSet<IResizable> _subscibers = new HashSet<IResizable>();

        public bool Maximized { get; protected set; }

        public void RegisterResizableTextObject(IResizable subscriber)
        {
            _subscibers.Add(subscriber);
            subscriber.Resize(Maximized);

        }

        public void UnregisterResizableTextObject(IResizable subscriber)
        {
            _subscibers.Remove(subscriber);
        }


        public void Resize(bool maximized)
        {
            if (maximized == Maximized)
                return;
            Maximized = maximized;
            NotifyAll(maximized);
        }


        private void NotifyAll(bool maximized)
        {
            foreach (var subscriber in _subscibers)
                subscriber.Resize(maximized);
        }

        
    }
}