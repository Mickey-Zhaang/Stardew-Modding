using Microsoft.Xna.Framework;
using StardewValley;

namespace Magicka.Spells
{
    /// <summary>
    /// Interface for all spells in the Magicka mod
    /// </summary>
    public interface ISpell
    {
        /// <summary>
        /// The name of the spell
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Casts the spell from the player's position toward the target position
        /// </summary>
        /// <param name="player">The player casting the spell</param>
        /// <param name="targetPosition">The target position in world coordinates</param>
        /// <param name="location">The game location where the spell is being cast</param>
        void Cast(Farmer player, Vector2 targetPosition, GameLocation location);
    }
}

