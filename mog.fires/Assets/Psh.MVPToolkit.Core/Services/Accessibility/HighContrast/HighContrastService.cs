using System.Collections.Generic;

namespace Psh.MVPToolkit.Core.Services.Accessibility.HighContrast
{
    public class HighContrastService : IHighContrastService
    {

        private readonly HashSet<IContrastable> _subscribers = new();

        public bool ContrastEnabled { get; private set; }

        public void RegisterHighContrastObject(IContrastable subscriber)
        {
            _subscribers.Add(subscriber);
            subscriber.UpdateContrast(ContrastEnabled);
        }

        public void UnregisterHighContrastObject(IContrastable subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public void SwitchContrast(bool enabled)
        {
            ContrastEnabled = enabled;
            NotifyAll();
        }


        private void NotifyAll()
        {
            foreach (var subscriber in _subscribers)
                subscriber.UpdateContrast(ContrastEnabled);
        }

       
    }
}