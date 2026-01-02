using System;
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

        private readonly Action<BasicProjectile, Vector2, Farmer>? _onProjectileCreated;

        /// <summary>
        /// Creates a new FireballSpell instance
        /// </summary>
        /// <param name="onProjectileCreated">Optional callback when a projectile is created (for tracking)</param>
        public FireballSpell(Action<BasicProjectile, Vector2, Farmer>? onProjectileCreated = null)
        {
            _onProjectileCreated = onProjectileCreated;
        }

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

            _onProjectileCreated?.Invoke(fireball, startPos, player);
        }

        /// <summary>
        /// Explodes the fireball at the given position
        /// </summary>
        private void Explode(GameLocation location, int xPosition, int yPosition, Character who)
        {
            Vector2 tile = new Vector2(xPosition / 64, yPosition / 64);

            location.explode(tile, Fireball.ExplosionRadius, Game1.player, false, -1);
        }

    }
}

