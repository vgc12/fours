using Input;
using Levels;
using Logging;
using Player.Input;
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
            builder.AddSingleton(typeof(SwipeDetector), typeof(ISwipeDetector), typeof(SwipeDetector));
            builder.AddSingleton(InputManager.Instance, typeof(InputManager), typeof(IInputManager), typeof(PlayerInputActions.IMainActions));

        }
    }
}