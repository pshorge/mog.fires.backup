using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Artigio.MVVMToolkit.Core.Infrastructure
{
    /// <summary>
    /// Generyczna klasa Reference Counter (RC) dla zarządzania zasobami
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class RC<T> where T : class
    {
        protected T resource;
        protected int refCount;
        protected string resourceId;
        protected Func<string, UniTask<T>> loadFunc;
        protected Action<T> releaseFunc;
        protected bool isLoading;

        // ReSharper disable once MemberCanBeProtected.Global
        public RC(string resourceId, Func<string, UniTask<T>> loadFunc, Action<T> releaseFunc = null)
        {
            this.resourceId = resourceId;
            this.loadFunc = loadFunc;
            this.releaseFunc = releaseFunc;
            this.refCount = 0;
            this.isLoading = false;
        }

        public string ResourceId => resourceId;

        public virtual async UniTask<T> Acquire()
        {
            refCount++;

            if (resource != null || isLoading) 
                return resource;
            
            isLoading = true;
            try
            {
                resource = await loadFunc(resourceId);
            }
            finally
            {
                isLoading = false;
            }

            return resource;
        }

        public virtual bool Release()
        {
            if (refCount <= 0)
            {
                Debug.LogWarning($"Próba zwolnienia zasobu {resourceId}, który nie jest w użyciu.");
                return false;
            }

            refCount--;

            if (refCount != 0 || resource == null) 
                return false;
            
            releaseFunc?.Invoke(resource);
            resource = null;
            return true;

        }

        public virtual int GetRefCount() => refCount;
        public virtual bool IsLoaded() => resource != null;
    }

    /// <summary>
    /// Thread-safe wersja klasy Reference Counter
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class ThreadSafeRC<T> : RC<T> where T : class
    {
        private readonly object _lockObject = new();

        public ThreadSafeRC(string resourceId, Func<string, UniTask<T>> loadFunc, Action<T> releaseFunc = null)
            : base(resourceId, loadFunc, releaseFunc)
        {
        }

        public override async UniTask<T> Acquire()
        {
            bool startLoading = false;
            
            lock (_lockObject)
            {
                refCount++;
                
                if (resource == null && !isLoading)
                {
                    isLoading = true;
                    startLoading = true;
                }
            }

            if (startLoading)
            {
                try
                {
                    T loadedResource = await loadFunc(resourceId);
                    
                    lock (_lockObject)
                    {
                        resource = loadedResource;
                    }
                }
                finally
                {
                    lock (_lockObject)
                    {
                        isLoading = false;
                    }
                }
            }

           
            while (true)
            {
                lock (_lockObject)
                {
                    if (resource != null || !isLoading)
                    {
                        return resource;
                    }
                }
                await UniTask.Delay(10);
            }
        }

        public override bool Release()
        {
            lock (_lockObject)
            {
                if (refCount <= 0)
                {
                    Debug.LogWarning($"Próba zwolnienia zasobu {resourceId}, który nie jest w użyciu.");
                    return false;
                }

                refCount--;

                if (refCount != 0 || resource == null) 
                    return false;
                
                releaseFunc?.Invoke(resource);
                resource = null;
                return true;

            }
        }

        public override int GetRefCount()
        {
            lock (_lockObject)
            {
                return refCount;
            }
        }

        public override bool IsLoaded()
        {
            lock (_lockObject)
            {
                return resource != null;
            }
        }
    }
}