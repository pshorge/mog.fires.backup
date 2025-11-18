using Sources.Features.ControlButtons.Presenter;
using Sources.Features.GlobeScreen.Presenter;
using VContainer;

namespace Sources.Data.Models
{
    public sealed class AppContent
    {
        [Inject] public ControlPanelPresenter ControlPanelPresenter { get; init; }
        [Inject] public GlobePresenter GlobePresenter { get; init; }
    }
}