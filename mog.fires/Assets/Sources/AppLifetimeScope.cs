using Psh.MVPToolkit.Core.Application.Services;
using Psh.MVPToolkit.Core.Infrastructure.Caching;
using Psh.MVPToolkit.Core.Infrastructure.FileSystem;
using Psh.MVPToolkit.Core.Services.Localization;
using Sources.App;
using Sources.Data.Models;
using Sources.Data.Repositories;
using Sources.Features.ControlButtons.Presenter;
using Sources.Features.GlobeScreen.Presenter;
using Sources.Features.MapScreen.Presenter;
using Sources.Features.Popup.Presenter;
using Sources.Features.ScreensaverScreen.Presenter;
using Sources.Infrastructure;
using Sources.Infrastructure.Configuration;
using Sources.Infrastructure.Input;
using Sources.Infrastructure.Input.Sources;
using Sources.Presentation.Management;
using Sources.Presentation.Navigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using MediaBackground = Psh.MVPToolkit.Core.UI.MediaBackground;

namespace Sources
{
    
    public class AppLifetimeScope : LifetimeScope
    {

        protected override void Configure(IContainerBuilder builder)
        {
            
            var path = ContentPathResolver.ResolveContentPath("presentation.json");
            const string key = "mog-fires";
            var (_, settings, languages, registry) = NewSchemaRepository.Load(path, key);

            // Localization
            var localizationService = new LocalizationService(languages);
            if (registry is { Count: > 0 })
                localizationService.AddTranslations(registry, overwrite: true);

            // Instances
            builder.RegisterInstance(settings);
            builder.RegisterInstance<ILocalizationService>(localizationService);
            
            AppConfig config = ConfigLoader.Load();
            builder.RegisterInstance(config);

            var inactivityServiceSettings = new InactivityServiceSettings(settings.ScreensaverTimeoutSeconds);
            builder.RegisterInstance(inactivityServiceSettings);

            // Services
            builder.Register<TextureAssetService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterBuildCallback(resolver =>
            {
                MediaBackground.Configure(resolver.Resolve<ITextureAssetService>());
            });
            
            builder.Register<QuitOnEscapeHandler>(Lifetime.Singleton)
                .AsImplementedInterfaces();
            
            // MVP Presenters as singletons
            builder.Register<ScreensaverPresenter>(Lifetime.Singleton).AsSelf();
            builder.Register<GlobePresenter>(Lifetime.Singleton).AsSelf();
            builder.Register<MapPresenter>(Lifetime.Singleton).AsSelf();
            builder.Register<ControlPanelPresenter>(Lifetime.Singleton).AsSelf();
            builder.Register<PopupPresenter>(Lifetime.Singleton).AsSelf();

            // Agregator
            builder.Register<AppContent>(Lifetime.Singleton);

            // MonoBehaviours 
            builder.RegisterComponentInHierarchy<InactivityService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<NavigationService>().AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<EarthController>();
            builder.RegisterComponentInHierarchy<MapController>();
            
            // Control Panel Manager
            builder.Register<ControlPanelManager>(Lifetime.Singleton);
            
            ConfigureInput(builder);
        }

        private void ConfigureInput(IContainerBuilder builder)
        {
            
            // Sources
            builder.Register<KeyboardInputSource>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<MouseInputSource>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<SerialPortInputSource>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            
            // Service
            builder.Register<UnifiedInputService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
    
}