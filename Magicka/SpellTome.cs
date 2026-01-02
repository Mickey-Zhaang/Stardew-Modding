using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;

namespace Magicka
{
    /// <summary>
    /// A magical tome that allows the player to cast spells
    /// </summary>
    public class SpellTome : Tool
    {
        public SpellTome()
            : base("Spell Tome", 0, 0, 0, false)
        {
            // Initialize basic properties
            this.Name = "Spell Tome";
            this.InstantUse = true; // Can be used instantly without animation delay

            // Set ParentSheetIndex to use an existing sprite from Objects sprite sheet
            // Using index 102 (Book) - looks like a tome!
            this.ParentSheetIndex = 102;

            // Ensure the tool is properly initialized
            this.IndexOfMenuItemView = 102; // For menu display
        }

        /// <summary>
        /// Called when the tool is used. The actual spell casting is handled in ModEntry.
        /// </summary>
        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            // Don't call base - we don't want tool behavior, just spell casting
            // The ModEntry will handle the actual spell logic based on button inputs
        }

        /// <summary>
        /// Required override for Item
        /// </summary>
        protected override Item GetOneNew()
        {
            return new SpellTome();
        }

        /// <summary>
        /// Override to set display name
        /// </summary>
        protected override string loadDisplayName()
        {
            return "Spell Tome";
        }

        /// <summary>
        /// Override to set description
        /// </summary>
        protected override string loadDescription()
        {
            return "An ancient tome containing powerful spells. Left-click to cast Fireball.";
        }

        /// <summary>
        /// Override draw method to use object sprite sheet instead of tools sprite sheet
        /// </summary>
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            // Draw using the Objects sprite sheet instead of Tools sprite sheet
            // ParentSheetIndex 102 = Book sprite
            Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.ParentSheetIndex, 16, 16);
            spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, sourceRect, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
        }
    }
}
