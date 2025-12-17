using System;
using Reflex.Core;
using Reflex.Extensions;
using Singletons;

namespace DependencyInjection
{
    /// <summary>
    ///     Provides runtime dependency resolution using Reflex DI containers with hierarchical scope fallback.
    ///     Implements singleton pattern to ensure a single resolver instance throughout the application lifecycle.
    /// </summary>
    /// <remarks>
    ///     This resolver attempts to resolve dependencies from Scene containers first (narrower scope),
    ///     then falls back to the Project container (broader scope). Container references are cached
    ///     for performance optimization and cleared automatically on destruction.
    /// </remarks>
    public sealed class RuntimeResolver : Singleton<RuntimeResolver>
    {
        /// <summary>
        ///     Cached reference to the project-scoped DI container.
        /// </summary>
        private Container _cachedProjectContainer;

        /// <summary>
        ///     Cached reference to the scene-scoped DI container.
        /// </summary>
        private Container _cachedSceneContainer;

        /// <summary>
        ///     Clears cached container references when the GameObject is destroyed.
        ///     This prevents stale references when scenes are unloaded or reloaded.
        /// </summary>
        private void OnDestroy()
        {
            _cachedSceneContainer = null;
            _cachedProjectContainer = null;
        }

        /// <summary>
        ///     Resolves a dependency of the specified type from available DI containers.
        /// </summary>
        /// <typeparam name="T">The type of dependency to resolve.</typeparam>
        /// <returns>An instance of the requested dependency type.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the dependency cannot be resolved from either Scene or Project containers.
        /// </exception>
        /// <remarks>
        ///     Resolution order:
        ///     1. Scene container (if available)
        ///     2. Project container (if available)
        ///     Containers are cached after first access to improve performance on subsequent calls.
        /// </remarks>
        public T Resolve<T>()
        {
            // Cache containers to avoid repeated lookups
            _cachedSceneContainer ??= TryGetSceneContainer();
            _cachedProjectContainer ??= TryGetProjectContainer();

            // Try scene container first (more specific scope)
            T dep = default;

            if (_cachedSceneContainer != null) dep = _cachedSceneContainer.Resolve<T>();

            // Fall back to project container
            if (_cachedProjectContainer != null && dep == null) dep = _cachedProjectContainer.Resolve<T>();

            if (dep == null)
                throw new InvalidOperationException(
                    $"Could not resolve dependency of type {typeof(T).Name}. " +
                    "Ensure it's registered in either Scene or Project container.");

            return dep;
        }

        /// <summary>
        ///     Attempts to resolve a dependency of the specified type without throwing exceptions on failure.
        /// </summary>
        /// <typeparam name="T">The type of dependency to resolve.</typeparam>
        /// <param name="dependency">
        ///     When this method returns, contains the resolved dependency if successful;
        ///     otherwise, the default value for type <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the dependency was successfully resolved; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     This method follows the same resolution order as <see cref="Resolve{T}" />,
        ///     but returns false instead of throwing an exception when resolution fails.
        /// </remarks>
        public bool TryResolve<T>(out T dependency)
        {
            dependency = default;
            _cachedSceneContainer ??= TryGetSceneContainer();
            _cachedProjectContainer ??= TryGetProjectContainer();
            if (_cachedProjectContainer != null)
            {
                dependency = _cachedProjectContainer.Resolve<T>();
            }

            if (_cachedProjectContainer != null && dependency != null) return true;
            
            if (_cachedSceneContainer!= null)
            {
                dependency = _cachedSceneContainer.Resolve<T>();
            }

            if (_cachedSceneContainer != null && dependency != null) return true;

           

            return false;
        }

        /// <summary>
        ///     Safely attempts to retrieve the scene-scoped DI container for the current GameObject's scene.
        /// </summary>
        /// <returns>
        ///     The scene's DI container if available; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     This method catches all exceptions during container retrieval to prevent crashes
        ///     when the scene container is not properly initialized or the scene is being unloaded.
        /// </remarks>
        private Container TryGetSceneContainer()
        {
            try
            {
                return gameObject.scene.GetSceneContainer();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Safely attempts to retrieve the project-scoped (global) DI container.
        /// </summary>
        /// <returns>
        ///     The project's DI container if available; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        ///     This method catches all exceptions during container retrieval to prevent crashes
        ///     when the project container is not properly initialized.
        /// </remarks>
        private Container TryGetProjectContainer()
        {
            try
            {
                return Container.ProjectContainer; // Adjust based on your Reflex version
            }
            catch
            {
                return null;
            }
        }
    }
}