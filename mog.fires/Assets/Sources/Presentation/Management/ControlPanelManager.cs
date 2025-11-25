using System;
using Sources.Features.ControlButtons.View;
using Sources.Presentation.Core.Types;

namespace Sources.Presentation.Management
{
    /// <summary>
    /// Manages control panel visibility based on current view
    /// </summary>
    public class ControlPanelManager
    {
        private readonly ControlPanelView _controlPanelView;

        public ControlPanelManager(ControlPanelView controlPanelView)
        {
            _controlPanelView = controlPanelView;
        }

        public void ConfigureFor(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.None:
                case ViewType.Screensaver:
                    _controlPanelView.EnableLeftButtons(false);
                    _controlPanelView.EnableRightButtons(false);
                    _controlPanelView.EnableBackButton(false);
                    break;
                case ViewType.Globe:
                    _controlPanelView.SetDefault();
                    _controlPanelView.EnableLeftButtons(false);
                    _controlPanelView.EnableRightButtons(true);
                    _controlPanelView.EnableBackButton(false);
                    break;
                case ViewType.Map:
                    _controlPanelView.SetDefault();
                    _controlPanelView.EnableLeftButtons(false);
                    _controlPanelView.EnableRightButtons(true);
                    _controlPanelView.EnableBackButton(false);
                    break;
                default:
                    throw new NotSupportedException($"ViewType {viewType} is not supported");
            }
        }
    }
}