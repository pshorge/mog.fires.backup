using Artigio.MVVMToolkit.Core.Application.Services;
using Artigio.MVVMToolkit.Core.Infrastructure.Caching;
using Artigio.MVVMToolkit.Core.Infrastructure.FileSystem;
using Artigio.MVVMToolkit.Core.Services.Accessibility.HighContrast;
using Artigio.MVVMToolkit.Core.Services.Accessibility.TextResize;
using Artigio.MVVMToolkit.Core.Services.Localization;
using Artigio.MVVMToolkit.Core.UI;
using Sources.Data.Models;
using Sources.Data.Repositories;
using Sources.Features.ControlButtons.Model;
using Sources.Features.ScreensaverScreen.Model;
using Sources.Features.StartScreen.Model;
using Sources.Presentation.Management;
using Sources.Presentation.Navigation;
using UnityEngine.Video;
using VContainer;
using VContainer.Unity;

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

            var inactivityServiceSettings = new InactivityServiceSettings(settings.ScreensaverTimeoutSeconds);
            builder.RegisterInstance(inactivityServiceSettings);

            // Services
            builder.Register<TextureAssetService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterBuildCallback(resolver =>
            {
                MediaBackground.Configure(resolver.Resolve<ITextureAssetService>());
            });
            
            builder.Register<TextResizeService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<HighContrastService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            // IDisposable Models as singletons, 
            builder.Register<ScreensaverScreenModel>(Lifetime.Singleton).AsSelf();
            builder.Register<GlobeScreenModel>(Lifetime.Singleton).AsSelf();
            builder.Register<ControlButtonsModel>(Lifetime.Singleton).AsSelf();
           

            // Agregator
            builder.Register<AppContent>(Lifetime.Singleton);

            // MonoBehaviours 
            builder.RegisterComponentInHierarchy<InactivityService>().AsSelf().AsImplementedInterfaces();
            
            builder.RegisterComponentInHierarchy<NavigationService>().AsSelf().AsImplementedInterfaces();
            builder.Register<ControlButtonsPresenter>(Lifetime.Singleton);
            
        }
    }
    
}