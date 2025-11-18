using Cysharp.Threading.Tasks;
using Psh.MVPToolkit.Core.MVP.Contracts;
using Sources.Presentation.Core.Types;

namespace Sources.Presentation.Navigation
{
    public enum TransitionType
    {
        Instant,
        Fade,
        TopDown,
        SlideLeft,
        SlideRight
    }
    
    public class ViewTransition
    {
        public ViewType FromView { get; }
        public ViewType ToView { get; }
        public TransitionType Type { get; }
        public float Duration { get; }
        
        public ViewTransition(ViewType from, ViewType to, TransitionType type = TransitionType.Fade, float duration = 0.5f)
        {
            FromView = from;
            ToView = to;
            Type = type;
            Duration = duration;
        }
    }
    
    public interface IViewTransitionExecutor
    {
        UniTask ExecuteTransitionAsync(IView<ViewType> from, IView<ViewType> to, ViewTransition transition);
    }
}