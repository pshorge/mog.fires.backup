using System.Collections.Generic;
using Sources.Data.Models;
using Sources.Presentation.Core.Types;

namespace Sources.Presentation.Navigation
{
    public class NavigationConfiguration
    {
        private readonly Dictionary<ViewType, NavigationNode> _nodes = new();
        
        public class NavigationNode
        {
            public ViewType ViewType { get; set; }
            public ViewType? NextView { get; set; }
            public ViewType? PreviousView { get; set; }
            public bool AllowBack { get; set; } = true;
            public bool AllowForward { get; set; } = true;
            public TransitionType TransitionIn { get; set; } = TransitionType.Fade;
            public TransitionType TransitionOut { get; set; } = TransitionType.Fade;
            public Dictionary<string, object> Metadata { get; set; } = new();
        }
        
        public NavigationConfiguration(bool screensaverEnabled)
        {
            ConfigureFlow(screensaverEnabled);
        }
        
        private void ConfigureFlow(bool screensaverEnabled)
        {
            if (screensaverEnabled)
            {
                // Screensaver
                AddNode(ViewType.Screensaver, next: ViewType.Globe, allowBack: false, transitionIn: TransitionType.TopDown);
            
                // Globe
                AddNode(ViewType.Globe, next: ViewType.Map, previous: ViewType.Screensaver, allowBack: true);
            
                // Map
                AddNode(ViewType.Map, previous: ViewType.Globe, allowBack: true);
            }
            else
            {
                // Globe
                AddNode(ViewType.Globe, next: ViewType.Map, allowBack: true);
            
                // Map
                AddNode(ViewType.Map, previous: ViewType.Globe, allowBack: true);
            }
            
            
        }
        
        private void AddNode(ViewType viewType, ViewType? next = null, ViewType? previous = null,
            bool allowBack = true, bool allowForward = true, 
            TransitionType transitionIn = TransitionType.Fade,
            TransitionType transitionOut = TransitionType.Fade)
        {
            _nodes[viewType] = new NavigationNode
            {
                ViewType = viewType,
                NextView = next,
                PreviousView = previous,
                AllowBack = allowBack,
                AllowForward = allowForward,
                TransitionIn = transitionIn,
                TransitionOut = transitionOut
            };
        }
        
        public NavigationNode GetNode(ViewType viewType) => _nodes.GetValueOrDefault(viewType);
        
        public ViewType? GetNextView(ViewType current) => GetNode(current)?.NextView;
        public ViewType? GetPreviousView(ViewType current) => GetNode(current)?.PreviousView;
    }
}