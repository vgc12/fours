using Logging;
using Reflex.Core;
using UnityEngine;
using ILogger = Logging.ILogger;

namespace DependencyInjection
{
    public sealed class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            // builder.AddSingleton(typeof(InGameAudioManager), typeof(IAudioManager));
            builder.AddSingleton(FoursLogger.Instance, typeof(ILogger));

            
        }
    }
}