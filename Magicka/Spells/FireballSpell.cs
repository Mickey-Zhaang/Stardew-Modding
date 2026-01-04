using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Projectiles;
using static Magicka.Spells.Constants;

namespace Magicka.Spells
{
    /// <summary>
    /// Fireball spell that shoots a projectile that explodes on impact or at max range
    /// </summary>
    public class FireballSpell : Spell
    {
        public override string Name => "Fireball";

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

        public override void Cast(Farmer player, Vector2 targetPosition, GameLocation location)
        {
            Vector2 playerPos = GetCasterCenterPosition(player);
            Vector2 direction = CalculateDirection(playerPos, targetPosition, player);
            Vector2 velocity = direction * Fireball.Speed;
            Vector2 startPos = GetSpellStartPosition(player);

            // Create the projectile
            BasicProjectile fireball = new BasicProjectile(
                damageToFarmer: Fireball.Damage,
                spriteIndex: Fireball.SpriteIndex,
                bouncesTillDestruct: 0,
                tailLength: Fireball.TailLength,
                rotationVelocity: Fireball.RotationVelocity,
                xVelocity: velocity.X,
                yVelocity: velocity.Y,
                startingPosition: startPos,
                collisionSound: "fireball",
                collisionBehavior: (loc, x, y, who) => Explode(loc, x, y, who),
                explode: false,
                damagesMonsters: true,
                location: location,
                firer: player
            );

            location.projectiles.Add(fireball);

            // Track the projectile for max range checking
            _projectileData[fireball] = new ProjectileData(startPos, player);
        }

        /// <summary>
        /// Updates fireball projectiles (e.g., checking max range)
        /// </summary>
        public override void Update(GameLocation location)
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
        /// Explodes the fireball at the given position (collision callback)
        /// </summary>
        private void Explode(GameLocation location, int xPosition, int yPosition, Character who)
        {
            Vector2 tile = new Vector2(xPosition / 64, yPosition / 64);
            Farmer caster = who as Farmer ?? Game1.player;

            location.explode(tile, Fireball.ExplosionRadius, caster, false, -1);
        }

        /// <summary>
        /// Explodes at the given position (for max range)
        /// </summary>
        private void ExplodeAtPosition(GameLocation location, int xPosition, int yPosition, Farmer caster)
        {
            Vector2 tile = new Vector2(xPosition / 64, yPosition / 64);
            location.explode(tile, Fireball.ExplosionRadius, caster, false, -1);
        }
    }
}

