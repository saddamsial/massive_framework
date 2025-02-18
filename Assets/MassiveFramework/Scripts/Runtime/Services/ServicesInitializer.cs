using System.Linq;
using UniRx;
using Unity.Linq;

namespace MassiveCore.Framework.Runtime
{
    public class ServicesInitializer : BaseMonoBehaviour
    {
        private ReactiveProperty<bool> _initialized;

        public ReadOnlyReactiveProperty<bool> Initialized { get; private set; }

        private void Awake()
        {
            InitializeLoadedReactiveProperties();
            InitializeServices();
        }

        private void InitializeLoadedReactiveProperties()
        {
            _initialized = new ReactiveProperty<bool>();
            Initialized = _initialized.ToReadOnlyReactiveProperty();
        }

        private async void InitializeServices()
        {
            var initializers = CacheGameObject.Children().OfInterfaceComponent<IServiceInitializer>()
                .Where(service => (service as BaseMonoBehaviour).Activity()).ToArray();

            foreach (var initializer in initializers)
            {
                var result = await initializer.Initialize();
                var serviceName = (initializer as BaseMonoBehaviour).name;
                if (result)
                {
                    _logger.Print($"Service \"{serviceName}\" initialized!");
                }
                else
                {
                    _logger.PrintError($"Service \"{serviceName}\" didn't initialize!");
                    return;
                }
            }

            _initialized.Value = true;
        }
    }
}
