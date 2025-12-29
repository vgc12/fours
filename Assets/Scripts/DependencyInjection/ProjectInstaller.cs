using Levels;
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
            builder.AddSingleton(FoursLogger.Instance, typeof(ILogger), typeof(FoursLogger));
            builder.AddSingleton(LevelManager.Instance, typeof(LevelManager), typeof(ILevelManager));

        }
    }
}