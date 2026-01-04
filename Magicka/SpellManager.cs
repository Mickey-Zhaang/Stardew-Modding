using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using Magicka.Spells;

namespace Magicka
{
    /// <summary>
    /// Manages spell casting and delegates update logic to individual spells
    /// </summary>
    public class SpellManager
    {
        private readonly Dictionary<string, ISpell> _spells = new();

        public SpellManager()
        {
            // Register spells
            _spells["fireball"] = new FireballSpell();
        }

        /// <summary>
        /// Gets a spell by name
        /// </summary>
        public ISpell? GetSpell(string spellName)
        {
            return _spells.TryGetValue(spellName.ToLower(), out var spell) ? spell : null;
        }

        /// <summary>
        /// Casts a spell from the player toward the mouse position
        /// </summary>
        public void CastSpellTowardMouse(string spellName, Farmer player, GameLocation location)
        {
            if (player == null || location == null) return;

            // Get mouse position in world coordinates
            Point mouseScreenPos = Game1.getMousePosition();
            Vector2 mouseWorldPos = new Vector2(
                mouseScreenPos.X + Game1.viewport.X,
                mouseScreenPos.Y + Game1.viewport.Y
            );

            var spell = GetSpell(spellName);
            if (spell == null)
            {
                return;
            }

            spell.Cast(player, mouseWorldPos, location);
        }

        /// <summary>
        /// Updates all registered spells (delegates to each spell's Update method)
        /// </summary>
        public void Update(GameLocation location)
        {
            if (location == null) return;

            foreach (var spell in _spells.Values)
            {
                spell.Update(location);
            }
        }
    }
}

