using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Projectiles;

namespace Magicka
{
    public class ModEntry : Mod
    {
        bool verbose = true;
        bool isSpellMode = false;
        const float FIREBALL_SPEED = 12f;
        const float MAX_SPELL_RANGE = 500f;  // Max range in pixels

        // Track fireballs and their target positions
        Dictionary<BasicProjectile, Vector2> fireballTargets = new Dictionary<BasicProjectile, Vector2>();

        public override void Entry(IModHelper helper)
        {
            // Log Initialization
            this.Monitor.Log("Magicka initialized.", LogLevel.Debug);

            // In Game Subscriptions
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return;

            // toggle spell mode
            if (e.Button == SButton.Q)
            {
                isSpellMode = !isSpellMode;
                DebugLogger(isSpellMode ? "Spell casting mode ON - Click to cast fireball!" : "Spell casting mode OFF");
            }
            // shoot if spell mode on 
            if (isSpellMode && e.Button == SButton.MouseLeft)
            {
                ShootFireBall();
            }
            if (isSpellMode && e.Button.IsUseToolButton())
            {
                return; // Block tool usage
            }


        }

        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // Check each tracked fireball to see if it reached its target
            List<BasicProjectile> toRemove = new List<BasicProjectile>();

            foreach (var kvp in fireballTargets)
            {
                BasicProjectile fireball = kvp.Key;
                Vector2 targetPos = kvp.Value;

                // Check if fireball still exists in the world
                if (!Game1.currentLocation.projectiles.Contains(fireball))
                {
                    toRemove.Add(fireball);
                    continue;
                }

                // Get current position
                Vector2 currentPos = fireball.position.Value;

                // Check if we've reached or passed the target (within 20 pixels for tolerance)
                float distanceToTarget = Vector2.Distance(currentPos, targetPos);
                if (distanceToTarget <= 20f)
                {
                    // Explode at target position
                    int xPos = (int)targetPos.X;
                    int yPos = (int)targetPos.Y;
                    FireballExplosion(Game1.currentLocation, xPos, yPos, Game1.player);

                    // Remove from world and tracking
                    Game1.currentLocation.projectiles.Remove(fireball);
                    toRemove.Add(fireball);
                }
            }

            // Clean up removed fireballs
            foreach (var fireball in toRemove)
            {
                fireballTargets.Remove(fireball);
            }
        }

        private void ShootFireBall()
        {
            Point mouseScreenPos = Game1.getMousePosition();

            Vector2 mouseWorldPos = new Vector2(
                mouseScreenPos.X + Game1.viewport.X,
                mouseScreenPos.Y + Game1.viewport.Y
            );

            Vector2 playerPos = new Vector2(
                Game1.player.Position.X + 32,
                Game1.player.Position.Y + 32
            );

            Vector2 direction = mouseWorldPos - playerPos;
            float distance = direction.Length();

            // Calculate the actual target position (clamped to max range if needed)
            Vector2 targetPos;
            if (distance > MAX_SPELL_RANGE)
            {
                // Clamp to max range
                direction.Normalize();
                targetPos = playerPos + direction * MAX_SPELL_RANGE;
                distance = MAX_SPELL_RANGE;
                DebugLogger($"Target beyond max range! Clamping to {MAX_SPELL_RANGE}px");
            }
            else
            {
                // Use cursor position as target
                targetPos = mouseWorldPos;
            }

            // Normalize direction for velocity calculation
            if (distance > 0)
            {
                direction.Normalize();
            }
            else
            {
                // If clicking directly on player, default to facing direction
                direction = Game1.player.FacingDirection switch
                {
                    0 => new Vector2(0, -1),  // Up
                    1 => new Vector2(1, 0),    // Right
                    2 => new Vector2(0, 1),    // Down
                    3 => new Vector2(-1, 0),   // Left
                    _ => new Vector2(0, -1)
                };
            }

            Vector2 velocity = direction * FIREBALL_SPEED;

            Vector2 startPos = new Vector2(
                Game1.player.Position.X + 16,  // Center horizontally
                Game1.player.Position.Y - 16   // Slightly above center
            );

            DebugLogger($"Casting fireball! Target: {targetPos}, Distance: {distance:F1}px");

            // Create the Projectile
            BasicProjectile fireball = new BasicProjectile(
                damageToFarmer: 10,
                spriteIndex: 10,
                bouncesTillDestruct: 0,
                tailLength: 3,
                rotationVelocity: 1f,
                xVelocity: velocity.X,
                yVelocity: velocity.Y,
                startingPosition: startPos,
                collisionSound: "fireball",
                collisionBehavior: FireballExplosion,
                explode: false,
                damagesMonsters: true,
                location: Game1.currentLocation,
                firer: Game1.player
            );

            // Add to world
            Game1.currentLocation.projectiles.Add(fireball);

            // Track this fireball with its target position (clamped if needed)
            fireballTargets[fireball] = targetPos;
        }

        private void FireballExplosion(GameLocation location, int xPosition, int yPosition, Character who)
        {
            DebugLogger($"Fireball hit at {xPosition}, {yPosition}!");

            // Convert pixel position to Tile position (divide by 64)
            Vector2 tile = new Vector2(xPosition / 64, yPosition / 64);

            // Trigger the Explosion
            // 'false' means it won't damage farmers (damages monsters and terrain)
            location.explode(tile, 4, Game1.player, false, -1);
        }

        private void DebugLogger(string statement)
        {
            if (verbose)
                this.Monitor.Log($"Debug statement: {statement}", LogLevel.Debug);
        }
    }
}