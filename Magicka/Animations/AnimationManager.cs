using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Magicka.Animations
{
    /// <summary>
    /// Manages all sprite animations in the mod with automatic play/stop control
    /// </summary>
    public class AnimationManager
    {
        private class AnimationController
        {
            public SpriteAnimation Animation { get; }
            public Func<bool>? ShouldPlay { get; }

            public AnimationController(SpriteAnimation animation, Func<bool>? shouldPlay = null)
            {
                Animation = animation;
                ShouldPlay = shouldPlay;
            }
        }

        private readonly IMonitor _monitor;
        private readonly Dictionary<string, AnimationController> _animations = new();

        public AnimationManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Registers a new animation with automatic play/stop control
        /// </summary>
        /// <param name="name">Unique name for the animation</param>
        /// <param name="helper">SMAPI helper for loading content</param>
        /// <param name="texturePath">Path to the sprite sheet texture</param>
        /// <param name="frameWidth">Width of each frame in pixels</param>
        /// <param name="frameTime">Time per frame in milliseconds</param>
        /// <param name="shouldPlay">Optional condition function that returns true when the animation should play</param>
        /// <returns>The created animation, or null if loading failed</returns>
        public SpriteAnimation? RegisterAnimation(string name, IModHelper helper, string texturePath, int frameWidth = 16, float frameTime = 250f, Func<bool>? shouldPlay = null)
        {
            var animation = new SpriteAnimation(helper, texturePath, frameWidth, frameTime, _monitor);

            if (animation.IsLoaded)
            {
                _animations[name] = new AnimationController(animation, shouldPlay);
                return animation;
            }

            return null;
        }

        /// <summary>
        /// Gets an animation by name
        /// </summary>
        public SpriteAnimation? GetAnimation(string name)
        {
            return _animations.TryGetValue(name, out var controller) ? controller.Animation : null;
        }

        /// <summary>
        /// Updates all animations and handles automatic play/stop based on conditions
        /// </summary>
        /// <param name="gameTime">Current game time</param>
        public void Update(GameTime gameTime)
        {
            float elapsedMs = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            foreach (var kvp in _animations)
            {
                var controller = kvp.Value;
                var animation = controller.Animation;

                // Update animation frame timing
                animation.Update(elapsedMs);

                // Handle automatic play/stop if condition is provided
                if (controller.ShouldPlay != null)
                {
                    bool shouldBePlaying = controller.ShouldPlay();

                    if (shouldBePlaying && !animation.IsPlaying)
                    {
                        animation.Play();
                    }
                    else if (!shouldBePlaying && animation.IsPlaying)
                    {
                        animation.Stop();
                    }
                }
            }
        }
    }
}

