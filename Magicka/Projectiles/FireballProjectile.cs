using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace Magicka.Projectiles
{
    /// <summary>
    /// Custom standalone projectile class for the fireball spell
    /// </summary>
    public class FireballProjectile : Projectile
    {
        /// <summary>Delegate for collision behavior callback</summary>
        public delegate void OnCollisionBehavior(GameLocation location, int xPosition, int yPosition, Character who);

        /// <summary>The amount of damage caused when this projectile hits a monster or player.</summary>
        public readonly NetInt damageToFarmer = new NetInt();

        /// <summary>The sound played when the projectile collides with something.</summary>
        public readonly NetString collisionSound = new NetString();

        /// <summary>Whether the projectile explodes when it collides with something.</summary>
        public readonly NetBool explode = new NetBool();

        /// <summary>A callback to invoke after the projectile collides with a player, monster, or wall.</summary>
        public OnCollisionBehavior collisionBehavior;

        /// <summary>Custom texture for the projectile sprite. Required - no default fallback.</summary>
        private static Texture2D? _customTexture;

        /// <summary>Custom source rectangle for the sprite. If null, uses full texture or first 16x16.</summary>
        private Rectangle? _customSourceRect;

        /// <summary>Construct an empty instance.</summary>
        public FireballProjectile()
        {
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="damageToFarmer">The amount of damage caused when this projectile hits a monster or player.</param>
        /// <param name="spriteIndex">The index of the sprite to draw in the projectile sheet.</param>
        /// <param name="bouncesTillDestruct">The number of times the projectile can bounce off walls before being destroyed.</param>
        /// <param name="tailLength">The length of the tail which trails behind the main projectile.</param>
        /// <param name="rotationVelocity">The rotation velocity.</param>
        /// <param name="xVelocity">The speed at which the projectile moves along the X axis.</param>
        /// <param name="yVelocity">The speed at which the projectile moves along the Y axis.</param>
        /// <param name="startingPosition">The pixel world position at which the projectile will start moving.</param>
        /// <param name="collisionSound">The sound played when the projectile collides with something.</param>
        /// <param name="bounceSound">The sound played when the projectile bounces off a wall.</param>
        /// <param name="firingSound">The sound played when the projectile is fired.</param>
        /// <param name="explode">Whether the projectile explodes when it collides with something.</param>
        /// <param name="damagesMonsters">Whether the projectile damage monsters (true) or players (false).</param>
        /// <param name="location">The location containing the projectile.</param>
        /// <param name="firer">The character who fired the projectile.</param>
        /// <param name="collisionBehavior">A callback to invoke after the projectile collides with a player, monster, or wall.</param>
        public FireballProjectile(
            int damageToFarmer,
            int spriteIndex,
            int bouncesTillDestruct,
            int tailLength,
            float rotationVelocity,
            float xVelocity,
            float yVelocity,
            Vector2 startingPosition,
            string collisionSound = null,
            string bounceSound = null,
            string firingSound = null,
            bool explode = false,
            bool damagesMonsters = false,
            GameLocation location = null,
            Character firer = null,
            OnCollisionBehavior collisionBehavior = null)
            : this()
        {
            this.damageToFarmer.Value = damageToFarmer;
            currentTileSheetIndex.Value = spriteIndex;
            bouncesLeft.Value = bouncesTillDestruct;
            base.tailLength.Value = tailLength;
            base.rotationVelocity.Value = rotationVelocity;
            base.xVelocity.Value = xVelocity;
            base.yVelocity.Value = yVelocity;
            position.Value = startingPosition;
            this.explode.Value = explode;
            this.collisionSound.Value = collisionSound;
            base.bounceSound.Value = bounceSound;
            base.damagesMonsters.Value = damagesMonsters;
            theOneWhoFiredMe.Set(location, firer);
            this.collisionBehavior = collisionBehavior;

            if (!string.IsNullOrEmpty(firingSound))
            {
                location?.playSound(firingSound);
            }
        }

        /// <inheritdoc />
        public override void updatePosition(GameTime time)
        {
            xVelocity.Value += acceleration.X;
            yVelocity.Value += acceleration.Y;
            if (maxVelocity.Value != -1f && Math.Sqrt(xVelocity.Value * xVelocity.Value + yVelocity.Value * yVelocity.Value) >= (double)maxVelocity.Value)
            {
                xVelocity.Value -= acceleration.X;
                yVelocity.Value -= acceleration.Y;
            }
            position.X += xVelocity.Value;
            position.Y += yVelocity.Value;
        }

        /// <inheritdoc />
        protected override void InitNetFields()
        {
            base.InitNetFields();
            base.NetFields.AddField(damageToFarmer, "damageToFarmer")
                .AddField(collisionSound, "collisionSound")
                .AddField(explode, "explode");
        }

        /// <inheritdoc />
        public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
        {
            if (damagesMonsters.Value)
            {
                return;
            }

            if (player.CanBeDamaged())
            {
                piercesLeft.Value--;
            }
            player.takeDamage(damageToFarmer.Value, overrideParry: false, null);
            explosionAnimation(location);
        }

        /// <inheritdoc />
        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location)
        {
            t.performUseAction(tileLocation);
            explosionAnimation(location);
            piercesLeft.Value--;
        }

        /// <inheritdoc />
        public override void behaviorOnCollisionWithOther(GameLocation location)
        {
            if (!ignoreObjectCollisions.Value)
            {
                explosionAnimation(location);
                piercesLeft.Value--;
            }
        }

        /// <inheritdoc />
        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            if (!damagesMonsters.Value)
            {
                return;
            }

            Farmer player = GetPlayerWhoFiredMe(location);
            explosionAnimation(location);

            if (n is Monster monster)
            {
                location.damageMonster(n.GetBoundingBox(), damageToFarmer.Value, damageToFarmer.Value + 1, isBomb: false, player, isProjectile: true);
                if (!monster.IsInvisible)
                {
                    piercesLeft.Value--;
                }
            }
        }

        /// <summary>
        /// Plays the explosion animation and handles collision behavior
        /// </summary>
        protected virtual void explosionAnimation(GameLocation location)
        {
            Rectangle sourceRect = GetSourceRect();
            sourceRect.X += 4;
            sourceRect.Y += 4;
            sourceRect.Width = 8;
            sourceRect.Height = 8;

            // Use custom texture for debris if available, otherwise skip debris
            if (_customTexture != null)
            {
                Rectangle debrisRect = GetSourceRect();
                debrisRect.X += 4;
                debrisRect.Y += 4;
                debrisRect.Width = 8;
                debrisRect.Height = 8;

                // Create debris using custom texture
                // Note: createRadialDebris_MoreNatural expects a texture path, so we'll use a simpler approach
                // or create debris manually. For now, we'll skip the debris animation if custom texture is used.
                // You could implement custom debris particles here if needed.
            }

            if (!string.IsNullOrEmpty(collisionSound.Value))
            {
                location.playSound(collisionSound.Value);
            }

            if (explode.Value)
            {
                location.temporarySprites.Add(
                    new TemporaryAnimatedSprite(
                        362,
                        Game1.random.Next(30, 90),
                        6,
                        1,
                        position.Value,
                        flicker: false,
                        Game1.random.NextBool()));
            }

            collisionBehavior?.Invoke(
                location,
                getBoundingBox().Center.X,
                getBoundingBox().Center.Y,
                GetPlayerWhoFiredMe(location));

            destroyMe = true;
        }

        /// <summary>Get the player who fired this projectile.</summary>
        /// <param name="location">The location containing the player.</param>
        public virtual Farmer GetPlayerWhoFiredMe(GameLocation location)
        {
            return (theOneWhoFiredMe.Get(location) as Farmer) ?? Game1.player;
        }

        /// <summary>
        /// Sets a custom texture for all FireballProjectile instances.
        /// Call this once during mod initialization (e.g., in ModEntry.Entry).
        /// </summary>
        /// <param name="texture">The custom texture to use (16x16 or larger sprite)</param>
        public static void SetCustomTexture(Texture2D texture)
        {
            _customTexture = texture;
        }

        /// <summary>
        /// Sets a custom source rectangle for this specific projectile instance.
        /// Useful for sprite sheets with multiple frames.
        /// </summary>
        /// <param name="sourceRect">The source rectangle defining which part of the texture to use</param>
        public void SetCustomSourceRect(Rectangle sourceRect)
        {
            _customSourceRect = sourceRect;
        }

        /// <summary>
        /// GetTexture - only returns custom texture, no default fallback
        /// </summary>
        public new Texture2D GetTexture()
        {
            if (_customTexture == null)
            {
                throw new InvalidOperationException("FireballProjectile custom texture not loaded. Ensure firebolt.png is in assets folder and loaded in ModEntry.");
            }
            return _customTexture;
        }

        /// <summary>
        /// GetSourceRect - only uses custom texture, no default fallback
        /// </summary>
        public new Rectangle GetSourceRect()
        {
            if (_customTexture == null)
            {
                throw new InvalidOperationException("FireballProjectile custom texture not loaded.");
            }

            if (_customSourceRect.HasValue)
            {
                return _customSourceRect.Value;
            }

            // If custom texture is set but no custom rect, use full texture dimensions
            return new Rectangle(0, 0, _customTexture.Width, _customTexture.Height);
        }

        /// <summary>
        /// Override draw to only draw if custom texture is loaded
        /// </summary>
        public override void draw(SpriteBatch b)
        {
            // Only draw if custom texture is loaded
            if (_customTexture == null)
            {
                return; // Don't draw anything if texture isn't loaded
            }

            float current_scale = 4f * localScale;
            Texture2D texture = GetTexture();
            Rectangle sourceRect = GetSourceRect();
            Vector2 pixelPosition = position.Value;

            // Calculate origin point based on actual texture size (center of sprite)
            Vector2 origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);

            b.Draw(
                texture,
                Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)),
                sourceRect,
                color.Value * alpha.Value,
                0f, // No rotation
                origin,
                current_scale,
                SpriteEffects.None,
                (pixelPosition.Y + 96f) / 10000f);

            if (height.Value > 0f)
            {
                b.Draw(
                    Game1.shadowTexture,
                    Game1.GlobalToLocal(Game1.viewport, pixelPosition + new Vector2(32f, 32f)),
                    Game1.shadowTexture.Bounds,
                    Color.White * alpha.Value * 0.75f,
                    0f,
                    new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                    2f,
                    SpriteEffects.None,
                    (pixelPosition.Y - 1f) / 10000f);
            }

            // Draw tail
            float tailAlpha = alpha.Value;
            for (int i = tail.Count - 1; i >= 0; i--)
            {
                b.Draw(
                    texture,
                    Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == tail.Count - 1) ? pixelPosition : tail.ElementAt(i + 1), tail.ElementAt(i), (float)tailCounter / 50f) + new Vector2(0f, 0f - height.Value) + new Vector2(32f, 32f)),
                    sourceRect,
                    color.Value * tailAlpha,
                    0f, // No rotation
                    origin, // Use same origin as main sprite
                    current_scale,
                    SpriteEffects.None,
                    (pixelPosition.Y - (float)(tail.Count - i) + 96f) / 10000f);
                tailAlpha -= 1f / (float)tail.Count;
                current_scale = 0.8f * (float)(4 - 4 / (i + 4));
            }
        }
    }
}
