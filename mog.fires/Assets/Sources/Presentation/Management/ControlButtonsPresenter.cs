using System;
using Sources.Features.ControlButtons.ViewModel;
using Sources.Presentation.Core.Types;

namespace Sources.Presentation.Management
{
    public class ControlButtonsPresenter
    {
        private readonly ControlButtonsViewModel _viewModel;

        public ControlButtonsPresenter(ControlButtonsViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        public void ConfigureFor(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.None:
                case ViewType.Screensaver:
                    _viewModel.EnableLeftButtons(false);
                    _viewModel.EnableRightButtons(false);
                    _viewModel.EnableBackButton(false);
                    break;
                case ViewType.Globe:
                    _viewModel.SetDefault();
                    _viewModel.EnableLeftButtons(false);
                    _viewModel.EnableRightButtons(true);
                    _viewModel.EnableBackButton(false);
                    break;
                default:
                    throw new NotSupportedException($"ViewType {viewType} does not support arguments");

            }
        }
    }
}