using Microsoft.Xna.Framework;
using StardewValley;

namespace Magicka.Spells
{
    /// <summary>
    /// Abstract base class for all spells, providing common functionality
    /// </summary>
    public abstract class Spell : ISpell
    {
        /// <summary>
        /// The name of the spell - must be implemented by derived classes
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Casts the spell from the player's position toward the target position
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void Cast(Farmer player, Vector2 targetPosition, GameLocation location);

        /// <summary>
        /// Gets the player's center position in world coordinates
        /// </summary>
        protected Vector2 GetCasterCenterPosition(Farmer player)
        {
            return new Vector2(
                player.Position.X + 32,
                player.Position.Y + 32
            );
        }

        /// <summary>
        /// Gets the player's spell cast start position (slightly above center)
        /// </summary>
        protected Vector2 GetSpellStartPosition(Farmer player)
        {
            return new Vector2(
                player.Position.X + 16,  // Center horizontally
                player.Position.Y - 16   // Slightly above center
            );
        }

        /// <summary>
        /// Calculates the direction vector from player position to target position
        /// </summary>
        protected Vector2 CalculateDirection(Vector2 fromPosition, Vector2 toPosition, Farmer player)
        {
            Vector2 direction = toPosition - fromPosition;

            // Normalize direction for velocity
            if (direction.Length() > 0)
            {
                direction.Normalize();
            }
            else
            {
                // If clicking directly on player, default to facing direction
                direction = GetDirectionFromFacing(player.FacingDirection);
            }

            return direction;
        }

        /// <summary>
        /// Gets a direction vector based on the player's facing direction
        /// </summary>
        protected static Vector2 GetDirectionFromFacing(int facingDirection)
        {
            return facingDirection switch
            {
                0 => new Vector2(0, -1),  // Up
                1 => new Vector2(1, 0),   // Right
                2 => new Vector2(0, 1),   // Down
                3 => new Vector2(-1, 0),  // Left
                _ => new Vector2(0, -1)
            };
        }

        /// <summary>
        /// Updates the spell's state. Default implementation does nothing.
        /// Override in derived classes if the spell needs per-frame updates.
        /// </summary>
        public virtual void Update(GameLocation location)
        {
            // Default: no update needed
        }
    }
}

