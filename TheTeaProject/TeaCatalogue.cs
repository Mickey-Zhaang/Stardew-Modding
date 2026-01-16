using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using StardewModdingAPI;

namespace TheTeaProject
{
    /// <summary>
    /// Custom tool: Tea Catalogue with 3-frame animation
    /// </summary>
    public class TeaCatalogue : Tool
    {
        private const int FRAME_COUNT = 3;
        private const int FRAME_TIME_MS = 200; // 200ms per frame = 5 fps
        private int _currentFrame = 0;
        private double _lastFrameTime = 0;
        private Texture2D? _customTexture;
        private int _frameWidth;

        /// <summary>
        /// Construct an empty instance (required for serialization)
        /// </summary>
        public TeaCatalogue()
            : base("Tea Catalogue", 0, 0, 0, false)
        {
            this.Name = "Tea Catalogue";
            this.InstantUse = true;
            this.ParentSheetIndex = -1; // We'll use custom texture

            // Try to load texture if available
            LoadTexture();
        }

        /// <summary>
        /// Construct an instance with custom texture
        /// </summary>
        public TeaCatalogue(Texture2D texture, int frameWidth)
            : this()
        {
            _customTexture = texture;
            _frameWidth = frameWidth;
        }

        /// <summary>
        /// Load the texture from the mod's assets
        /// </summary>
        private void LoadTexture()
        {
            try
            {
                // Access the mod helper through a static reference or pass it in
                // For now, we'll set it when the tool is created in ModEntry
            }
            catch
            {
                // Texture loading will be handled by ModEntry
            }
        }

        /// <summary>
        /// Set the texture after construction (called from ModEntry)
        /// </summary>
        public void SetTexture(Texture2D texture, int frameWidth)
        {
            _customTexture = texture;
            _frameWidth = frameWidth;
        }

        /// <summary>
        /// Called when the tool is used
        /// </summary>
        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            // Add your tool functionality here
            // For example: open a menu, display tea information, etc.

            if (who == null || location == null) return;

            // Example: Show a message or open a menu
            Game1.showGlobalMessage("Tea Catalogue opened!");

            // You can add more functionality here:
            // - Open a custom menu
            // - Display tea information
            // - Trigger events
        }

        /// <summary>
        /// Draw the tool with animation
        /// </summary>
        public override void draw(SpriteBatch spriteBatch)
        {
            if (_customTexture == null)
            {
                // Fallback to default tool drawing if texture not loaded
                base.draw(spriteBatch);
                return;
            }

            // Update animation frame
            UpdateAnimation();

            // Calculate source rectangle for current frame
            Rectangle sourceRect = new Rectangle(
                _currentFrame * _frameWidth,
                0,
                _frameWidth,
                _customTexture.Height
            );

            // Draw the tool
            if (this.IndexOfMenuItemView != -1)
            {
                // Draw in inventory/menu
                spriteBatch.Draw(
                    _customTexture,
                    new Vector2(this.IndexOfMenuItemView % 12 * 64, this.IndexOfMenuItemView / 12 * 64),
                    sourceRect,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    0.86f
                );
            }
        }

        /// <summary>
        /// Draw the tool in the player's hand
        /// </summary>
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (_customTexture == null)
            {
                base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
                return;
            }

            // Update animation frame
            UpdateAnimation();

            // Calculate source rectangle for current frame
            Rectangle sourceRect = new Rectangle(
                _currentFrame * _frameWidth,
                0,
                _frameWidth,
                _customTexture.Height
            );

            // Draw the tool in menu
            spriteBatch.Draw(
                _customTexture,
                location + new Vector2(32f, 32f),
                sourceRect,
                color * transparency,
                0f,
                new Vector2(_frameWidth / 2f, _customTexture.Height / 2f),
                4f * scaleSize,
                SpriteEffects.None,
                layerDepth
            );
        }

        /// <summary>
        /// Update animation frame based on time
        /// Only animates if the tool is currently selected/equipped
        /// </summary>
        private void UpdateAnimation()
        {
            if (_customTexture == null || Game1.currentGameTime == null) return;

            // Check if this tool is currently selected/equipped
            bool isSelected = IsToolSelected();

            if (isSelected)
            {
                // Animate when selected
                double currentTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;

                // Check if it's time to advance to next frame
                if (currentTime - _lastFrameTime >= FRAME_TIME_MS)
                {
                    _currentFrame = (_currentFrame + 1) % FRAME_COUNT;
                    _lastFrameTime = currentTime;
                }
            }
            else
            {
                // Keep on frame 1 (index 0, second frame) when not selected
                _currentFrame = 0;
            }
        }

        /// <summary>
        /// Check if this tool is currently selected/equipped by the player
        /// </summary>
        private bool IsToolSelected()
        {
            if (Game1.player == null) return false;

            // Check if this tool is the currently selected tool
            if (Game1.player.CurrentTool == this)
            {
                return true;
            }

            // Also check if it's selected in inventory (hovered over)
            // This handles the case when viewing inventory
            if (Game1.activeClickableMenu != null)
            {
                // Check if player is hovering over this item in inventory
                // This is a simplified check - you might need to adjust based on your needs
                return false; // For now, only animate when equipped
            }

            return false;
        }

        /// <summary>
        /// Required override for Item
        /// </summary>
        protected override Item GetOneNew()
        {
            return new TeaCatalogue();
        }

        /// <summary>
        /// Override to set display name
        /// </summary>
        protected override string loadDisplayName()
        {
            return "Tea Catalogue";
        }

        /// <summary>
        /// Override to set description
        /// </summary>
        protected override string loadDescription()
        {
            return "A catalogue containing information about various teas.";
        }

        /// <summary>
        /// Get the current texture (for external access)
        /// </summary>
        public Texture2D? GetTexture()
        {
            return _customTexture;
        }
    }
}
