using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Projectiles;
using Magicka.Spells;
using static Magicka.Spells.Constants;

namespace Magicka
{
    /// <summary>
    /// Manages spell casting, tracking, and lifecycle
    /// </summary>
    public class SpellManager
    {
        private class ProjectileData
        {
            public Vector2 StartPosition { get; set; }
            public Farmer Caster { get; set; }

            public ProjectileData(Vector2 startPosition, Farmer caster)
            {
                StartPosition = startPosition;
                Caster = caster;
            }
        }

        private readonly Dictionary<BasicProjectile, ProjectileData> _projectileData = new();
        private readonly Dictionary<string, ISpell> _spells = new();

        public SpellManager()
        {
            // Register spells
            var fireballSpell = new FireballSpell(OnProjectileCreated);
            _spells["fireball"] = fireballSpell;
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
        /// Updates spell projectiles (e.g., checking max range)
        /// </summary>
        public void Update(GameLocation location)
        {
            if (location == null) return;

            List<BasicProjectile> toRemove = new List<BasicProjectile>();

            foreach (var kvp in _projectileData.ToList())
            {
                BasicProjectile projectile = kvp.Key;
                ProjectileData data = kvp.Value;

                // If projectile was removed (collided with something), clean up
                if (!location.projectiles.Contains(projectile))
                {
                    toRemove.Add(projectile);
                    continue;
                }

                // Check if projectile reached max range
                Vector2 currentPos = projectile.position.Value;
                float distanceTraveled = Vector2.Distance(data.StartPosition, currentPos);

                if (distanceTraveled >= Fireball.MaxRange)
                {
                    // Explode at max range
                    ExplodeAtPosition(location, (int)currentPos.X, (int)currentPos.Y, data.Caster);
                    location.projectiles.Remove(projectile);
                    toRemove.Add(projectile);
                }
            }

            // Clean up removed projectiles
            foreach (var projectile in toRemove)
            {
                _projectileData.Remove(projectile);
            }
        }

        /// <summary>
        /// Callback when a projectile is created (for tracking)
        /// </summary>
        private void OnProjectileCreated(BasicProjectile projectile, Vector2 startPosition, Farmer caster)
        {
            _projectileData[projectile] = new ProjectileData(startPosition, caster);
        }

        /// <summary>
        /// Explodes at the given position
        /// </summary>
        private void ExplodeAtPosition(GameLocation location, int xPosition, int yPosition, Farmer caster)
        {
            // Convert pixel position to tile position
            Vector2 tile = new Vector2(xPosition / 64, yPosition / 64);

            // Trigger the explosion
            location.explode(tile, Fireball.ExplosionRadius, caster, false, -1);
        }
    }
}

