using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Psh.MVPToolkit.Core.Infrastructure.Caching;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace Psh.MVPToolkit.Core.UI
{
    [UxmlElement("MediaBackground")]
    public partial class MediaBackground : VisualElement, IDisposable
    {
        private static ITextureAssetService s_TextureService;
        public static void Configure(ITextureAssetService service) => s_TextureService = service;

        [CreateProperty, UxmlAttribute("source")]

        public string source
        {
            get => _source;
            set
            {
                if (_source == value) return;
                _source = value;
                OnSourceChanged();
            }
        }

        [UxmlAttribute("autoplay")] public bool autoplay { get; set; } = true;
        [UxmlAttribute("loop")] public bool loop { get; set; } = true;
        [UxmlAttribute("mute")] public bool mute { get; set; } = true;

        [UxmlAttribute("video-class")] public string videoClass { get; set; }

        [UxmlAttribute("video-extensions")]
        public string videoExtensions
        {
            get => string.Join(",", _videoExts);
            set => _videoExts = ParseExtensions(value);
        }

        [UxmlAttribute("cache-strategy")] public TextureCacheStrategy cacheStrategy { get; set; } = TextureCacheStrategy.RC;

        public void Play()
        {
            if (string.IsNullOrEmpty(_source)) return;

            _isVideo = LooksLikeVideo(_source);

            if (!_isVideo)
                return;

            if (_videoPlayer == null)
            {
                SetupVideo();
                _pendingPlay = true; 
                return;
            }

            if (_prepared)
            {
                _videoPlayer.Play();
                _pendingPlay = false;
            }
            else
            {
                _pendingPlay = true;
                if (!_videoPlayer.isPrepared)
                    _videoPlayer.Prepare();
            }
        }
        public void Pause()
        {
            _pendingPlay = false;
            if (_videoPlayer != null)
                _videoPlayer.Pause();
        }
        public void Stop() { if (_videoPlayer != null) _videoPlayer.Stop(); }

        private string _source;
        private bool _isVideo;
        private VideoPlayer _videoPlayer;
        private GameObject _hostGo;
        private RenderTexture _rt;
        private bool _prepared;
        private bool _pendingPlay;
        private CancellationTokenSource _cts;
        private string[] _videoExts = { ".webm"};
        private Vector2Int _lastRTSize;
        //private int _version;

        public MediaBackground()
        {
            pickingMode = PickingMode.Ignore; 
            style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Cover));
            RegisterCallback<AttachToPanelEvent>(_ => { if (!string.IsNullOrEmpty(_source)) OnSourceChanged(); });
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private static string[] ParseExtensions(string csv)
        {
            if (string.IsNullOrEmpty(csv)) return Array.Empty<string>();
            var parts = csv.Split(',', ';', ' ');
            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i].Trim().ToLowerInvariant();
                if (!p.StartsWith(".")) p = "." + p;
                parts[i] = p;
            }
            return parts;
        }

        private bool LooksLikeVideo(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var ext = System.IO.Path.GetExtension(path)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(ext) && _videoExts.Any(e => ext == e);
        }

        private void OnSourceChanged()
        {
            _prepared = false;
            CancelPendingLoads();
            ClearBackground();
            DisposeVideo();

            if (string.IsNullOrWhiteSpace(_source)) return;

            _isVideo = LooksLikeVideo(_source);
            if (!string.IsNullOrEmpty(videoClass))
                EnableInClassList(videoClass, _isVideo);

            if (_isVideo)
            {
                SetupVideo();
            }
            else
            {
                this.SetImageElementAsync(_source, s_TextureService).Forget();
            }
        }

        private void SetupVideo()
        {
            EnsureHostGo();
            EnsureRenderTexture();

            _videoPlayer = _hostGo.AddComponent<VideoPlayer>();
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = _rt;
            _videoPlayer.isLooping = loop;
            _videoPlayer.audioOutputMode = mute ? VideoAudioOutputMode.None : VideoAudioOutputMode.Direct;
            _videoPlayer.url = _source;
            _videoPlayer.playOnAwake = false;
            _videoPlayer.waitForFirstFrame = true;

            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.errorReceived += OnVideoError;
            _videoPlayer.Prepare();

            style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_rt));
        }

        private void EnsureHostGo()
        {
            if (_hostGo != null) return;
            _hostGo = new GameObject("MediaBackground.VideoPlayer")
            {
                hideFlags = HideFlags.DontSave
            };
        }

        private void EnsureRenderTexture()
        {
            int w = Mathf.Max(1, Mathf.CeilToInt(resolvedStyle.width));
            int h = Mathf.Max(1, Mathf.CeilToInt(resolvedStyle.height));
            if (w <= 1 || h <= 1) return; 

            var newSize = new Vector2Int(w, h);
            if (_rt != null && newSize == _lastRTSize) return;

            _lastRTSize = newSize;

            if (_rt != null)
            {
                if (_videoPlayer != null && _videoPlayer.targetTexture == _rt) _videoPlayer.targetTexture = null;
                _rt.Release();
                UnityEngine.Object.Destroy(_rt);
                _rt = null;
            }

            _rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32)
            {
                name = "MediaBackground.RT",
                useMipMap = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _rt.Create();

            if (_videoPlayer != null) _videoPlayer.targetTexture = _rt;
            style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_rt));
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            _prepared = true;

            bool isVisible = panel != null && resolvedStyle.display != DisplayStyle.None;
            if ((_pendingPlay || autoplay) && isVisible)
            {
                vp.Play();
                _pendingPlay = false;
            }
        }

        private void OnVideoError(VideoPlayer vp, string message)
        {
            Debug.LogError($"[MediaBackground] Video error: {message} for '{_source}'");
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (_videoPlayer == null) return;
            EnsureRenderTexture();
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            CancelPendingLoads();
            DisposeVideo();
            ClearBackground();
        }

        private void CancelPendingLoads()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void ClearBackground()
        {
            style.backgroundImage = null;
        }

        private void DisposeVideo()
        {

            if (_videoPlayer != null)
            {
                _videoPlayer.prepareCompleted -= OnVideoPrepared;
                _videoPlayer.errorReceived -= OnVideoError;
                _videoPlayer.Stop();
                _videoPlayer.targetTexture = null;
                UnityEngine.Object.Destroy(_videoPlayer);
                _videoPlayer = null;
            }

            if (_rt != null)
            {
                _rt.Release();
                UnityEngine.Object.Destroy(_rt);
                _rt = null;
            }

            if (_hostGo != null)
            {
                UnityEngine.Object.Destroy(_hostGo);
                _hostGo = null;
            }
            _prepared = false;
        }

        public void Dispose()
        {
            CancelPendingLoads();
            DisposeVideo();
        }
    }
}