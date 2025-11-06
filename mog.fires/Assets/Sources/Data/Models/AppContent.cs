using Sources.Features.ControlButtons.Model;
using Sources.Features.StartScreen.Model;
using VContainer;

namespace Sources.Data.Models
{
    public sealed class AppContent
    {
        [Inject] public ControlButtonsModel ControlButtonsModel { get; init; }
        [Inject] public GlobeScreenModel GlobeScreenModel { get; init; }
    }
}