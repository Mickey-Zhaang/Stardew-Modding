using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace Magicka.Animations
{
    /// <summary>
    /// Manages all sprite animations in the mod
    /// </summary>
    public class AnimationManager
    {
        private readonly IMonitor _monitor;
        private readonly Dictionary<string, SpriteAnimation> _animations = new();

        public AnimationManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Registers a new animation
        /// </summary>
        /// <param name="name">Unique name for the animation</param>
        /// <param name="helper">SMAPI helper for loading content</param>
        /// <param name="texturePath">Path to the sprite sheet texture</param>
        /// <param name="frameWidth">Width of each frame in pixels</param>
        /// <param name="frameTime">Time per frame in milliseconds</param>
        /// <returns>The created animation, or null if loading failed</returns>
        public SpriteAnimation? RegisterAnimation(string name, IModHelper helper, string texturePath, int frameWidth = 16, float frameTime = 250f)
        {
            var animation = new SpriteAnimation(helper, texturePath, frameWidth, frameTime, _monitor);

            if (animation.IsLoaded)
            {
                _animations[name] = animation;
                return animation;
            }

            return null;
        }

        /// <summary>
        /// Gets an animation by name
        /// </summary>
        public SpriteAnimation? GetAnimation(string name)
        {
            return _animations.TryGetValue(name, out var animation) ? animation : null;
        }

        /// <summary>
        /// Updates all animations
        /// </summary>
        /// <param name="elapsedMilliseconds">Elapsed time since last update</param>
        public void Update(float elapsedMilliseconds)
        {
            foreach (var animation in _animations.Values)
            {
                animation.Update(elapsedMilliseconds);
            }
        }

        /// <summary>
        /// Updates all animations using game time
        /// </summary>
        /// <param name="gameTime">Current game time</param>
        public void Update(GameTime gameTime)
        {
            float elapsedMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Update(elapsedMs);
        }
    }
}

