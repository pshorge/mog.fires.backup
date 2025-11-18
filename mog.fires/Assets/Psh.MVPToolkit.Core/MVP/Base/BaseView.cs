using System;
using System.Collections;
using Psh.MVPToolkit.Core.Infrastructure.Caching;
using Psh.MVPToolkit.Core.UI;
using Cysharp.Threading.Tasks;
using Psh.MVPToolkit.Core.MVP.Contracts;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Psh.MVPToolkit.Core.MVP.Base
{
    public abstract class BaseView : MonoBehaviour {}

    /// <summary>
    /// Base class for MVP Views
    /// Handles UI logic and user interactions
    /// </summary>
    public abstract class BaseView<TViewType, TPresenter> : BaseView, IView<TViewType> 
        where TPresenter : BaseDataSource, IPresenter
    {
        public abstract TViewType GetViewType();
        public bool IsVisible { get; protected set; }
        protected virtual string ContainerName { get; }

        [HideInInspector, SerializeField] protected UIDocument _uiDocument;
        public VisualElement Container { get; protected set; }
        
        protected abstract TPresenter Presenter { get; set; }
        
        [Inject] protected ITextureAssetService TextureAssetService { get; set; }

        protected virtual void OnEnable()
        {
            if (_uiDocument == null)
                _uiDocument = FindAnyObjectByType<UIDocument>();
            
            Container = _uiDocument.rootVisualElement.Q<VisualElement>(ContainerName);
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        public virtual void Show()
        {
            Container.style.display = DisplayStyle.Flex;
            Container.BringToFront();
            IsVisible = true;
            var opacity = Container.style.opacity.value;
            Container.experimental.animation.Start(opacity, 1f, 500, (b, val) =>
            {
                b.style.opacity = val;
            });
        }

        public virtual void Hide()
        {
            var opacity = Container.style.opacity.value;
            Container.experimental.animation.Start(opacity, 0f, 0, (b, val) =>
            {
                b.style.opacity = val;
            }).OnCompleted(() =>
            {
                Container.style.display = DisplayStyle.None;
                IsVisible = false;
            });
        }

        protected void SetBackgroundFromResources(VisualElement element, string path)
        {
            element.SetBackgroundFromResources(path);
        }

        protected async UniTask<Texture2D> SetImageElementAsync(VisualElement element, string path)
        {
            return await element.SetImageElementAsync(path, TextureAssetService);
        }
        
        protected void SetImageElement(VisualElement element, string path)
        {
            element.SetImageElementAsync(path, TextureAssetService).Forget();
        }

        protected void SetTextElement(TextElement textElement, string data)
        {
            textElement.text = data;
        }
        
        protected void DelayAction(Action action, float delayInSeconds)
        {
            StartCoroutine(DelayActionCoroutine(action, delayInSeconds));
        }
        
        private IEnumerator DelayActionCoroutine(Action action, float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            action?.Invoke();
        }
    }
}