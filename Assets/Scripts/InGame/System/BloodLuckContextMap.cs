using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
namespace InGame.System
{
    public class BloodLuckContextMap
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T instance) where T : class {
            if (instance is not IBloodLuckContext)
            {
                throw new Exception($"you need to implement IBloodLuckContext to register at the context map: {typeof(T)}");
            }
            _services[typeof(T)] = instance;
        }
        public T GetRegisteredInstance<T>() where T : class {
            if (_services.TryGetValue(typeof(T), out var instance))
            {
                return instance as T;
            }
            throw new Exception($"This class was not registered at the context map: {typeof(T)}");
        }
        public async UniTask InitializeAllContextAsync()
        {
            var initTasks = new List<UniTask>();

            foreach (var service in _services.Values)
            {
                if (service is IBloodLuckContext initializable)
                {
                    initTasks.Add(initializable.InitAsync());
                }
            }

            await UniTask.WhenAll(initTasks);
        }
        public void WireContextDependencyAfterInit()
        {
            foreach (var service in _services.Values)
            {
                if (service is IBloodLuckContext initializable)
                {
                    initializable.WireContextAfterInit(this);
                }
            }
        }
    }
}