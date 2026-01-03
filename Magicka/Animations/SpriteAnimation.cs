using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace Magicka.Animations
{
    /// <summary>
    /// Manages a sprite sheet animation with frame cycling
    /// </summary>
    public class SpriteAnimation
    {
        private Texture2D? _texture;
        private int _currentFrame = 0;
        private float _timer = 0f;
        private bool _isPlaying = false;

        /// <summary>
        /// The loaded texture containing all animation frames
        /// </summary>
        public Texture2D? Texture => _texture;

        /// <summary>
        /// Width of each frame in pixels
        /// </summary>
        public int FrameWidth { get; private set; }

        /// <summary>
        /// Height of each frame in pixels
        /// </summary>
        public int FrameHeight { get; private set; }

        /// <summary>
        /// Total number of frames in the animation
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// Current frame index (0-based)
        /// </summary>
        public int CurrentFrame => _currentFrame;

        /// <summary>
        /// Time per frame in milliseconds
        /// </summary>
        public float FrameTime { get; set; }

        /// <summary>
        /// Whether the animation is currently playing
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Whether the texture was successfully loaded
        /// </summary>
        public bool IsLoaded => _texture != null;

        /// <summary>
        /// Creates a new sprite animation
        /// </summary>
        /// <param name="helper">SMAPI helper for loading content</param>
        /// <param name="texturePath">Path to the sprite sheet texture</param>
        /// <param name="frameWidth">Width of each frame in pixels (default: 16)</param>
        /// <param name="frameTime">Time per frame in milliseconds (default: 250ms = 4fps)</param>
        /// <param name="monitor">Optional monitor for logging</param>
        public SpriteAnimation(IModHelper helper, string texturePath, int frameWidth = 16, float frameTime = 250f, IMonitor? monitor = null)
        {
            FrameWidth = frameWidth;
            FrameTime = frameTime;

            try
            {
                _texture = helper.ModContent.Load<Texture2D>(texturePath);
                if (_texture != null)
                {
                    FrameHeight = _texture.Height;
                    FrameCount = _texture.Width / FrameWidth;
                }
            }
            catch (Exception ex)
            {
                if (monitor != null)
                {
                    monitor.Log($"Could not load animation texture '{texturePath}': {ex.Message}", LogLevel.Warn);
                }
            }
        }

        /// <summary>
        /// Starts playing the animation
        /// </summary>
        public void Play()
        {
            _isPlaying = true;
        }

        /// <summary>
        /// Stops the animation and resets to frame 0
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _currentFrame = 0;
            _timer = 0f;
        }

        /// <summary>
        /// Pauses the animation without resetting
        /// </summary>
        public void Pause()
        {
            _isPlaying = false;
        }

        /// <summary>
        /// Updates the animation frame based on elapsed time
        /// </summary>
        /// <param name="elapsedMilliseconds">Elapsed time since last update in milliseconds</param>
        public void Update(float elapsedMilliseconds)
        {
            if (!_isPlaying || _texture == null || FrameCount == 0) return;

            _timer += elapsedMilliseconds;

            if (_timer >= FrameTime)
            {
                _currentFrame = (_currentFrame + 1) % FrameCount;
                _timer = 0f;
            }
        }

        /// <summary>
        /// Gets the source rectangle for a specific frame
        /// </summary>
        /// <param name="frameIndex">Frame index (uses current frame if null)</param>
        /// <returns>Rectangle representing the frame in the sprite sheet</returns>
        public Rectangle GetFrameRect(int? frameIndex = null)
        {
            int frame = frameIndex ?? _currentFrame;
            return new Rectangle(
                frame * FrameWidth,
                0,
                FrameWidth,
                FrameHeight
            );
        }
    }
}

