using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Monsters;

namespace Magicka
{
    public class ModEntry : Mod
    {
        bool verbose = true;
        const float FIREBALL_SPEED = 12f;
        const float MAX_SPELL_RANGE = 400f;  // Max range in pixels

        // Track fireballs with their start positions (for max range calculation)
        Dictionary<BasicProjectile, Vector2> fireballStarts = new Dictionary<BasicProjectile, Vector2>();

        public override void Entry(IModHelper helper)
        {
            // Log Initialization - using Info level so it always shows
            this.Monitor.Log("Magicka initialized", LogLevel.Debug);

            // In Game Subscriptions
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("Game launched - registering console commands...", LogLevel.Debug);
            // Add console command to give the Spell Tome
            this.Helper.ConsoleCommands.Add("spelltome", "Gives you a Spell Tome", this.GiveSpellTome);
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            this.Monitor.Log("Save loaded - checking for Spell Tome...", LogLevel.Debug);
            // Optionally auto-give the tome on save load for testing
            // Uncomment the line below if you want it automatically added
            // GiveSpellTome("auto", new string[0]);
        }

        private void GiveSpellTome(string command, string[] args)
        {
            this.Monitor.Log($"GiveSpellTome called with command: {command}", LogLevel.Debug);

            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("You must be in-game to use this command.", LogLevel.Warn);
                return;
            }

            if (Game1.player == null)
            {
                this.Monitor.Log("Player is null!", LogLevel.Error);
                return;
            }

            try
            {
                this.Monitor.Log("Creating SpellTome instance...", LogLevel.Debug);
                SpellTome tome = new SpellTome();
                this.Monitor.Log($"SpellTome created: Name={tome.Name}, ParentSheetIndex={tome.ParentSheetIndex}", LogLevel.Debug);

                if (Game1.player.addItemToInventoryBool(tome))
                {
                    this.Monitor.Log("Spell Tome added to inventory!", LogLevel.Info);
                }
                else
                {
                    this.Monitor.Log("Inventory is full! Spell Tome dropped at your feet.", LogLevel.Warn);
                    Game1.createItemDebris(tome, Game1.player.getStandingPosition(), -1, null);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error creating SpellTome: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return;

            // Check if player has Spell Tome equipped
            if (Game1.player.CurrentTool is SpellTome)
            {
                // Left click = Fireball
                if (e.Button == SButton.MouseLeft)
                {
                    ShootFireBall();
                    return; // Prevent default action
                }
                // Right click = Lightning (if you add it later)
                // if (e.Button == SButton.MouseRight)
                // {
                //     ShootLightning();
                //     return;
                // }
            }
        }

        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            // Check each tracked fireball to see if it reached max range
            List<BasicProjectile> toRemove = new List<BasicProjectile>();

            foreach (var kvp in fireballStarts.ToList())
            {
                BasicProjectile fireball = kvp.Key;
                Vector2 startPos = kvp.Value;

                // If fireball was removed (collided with something), just clean up
                if (!Game1.currentLocation.projectiles.Contains(fireball))
                {
                    toRemove.Add(fireball);
                    continue;
                }

                // Get current position and calculate distance traveled
                Vector2 currentPos = fireball.position.Value;
                float distanceTraveled = Vector2.Distance(startPos, currentPos);

                // Explode at max range
                if (distanceTraveled >= MAX_SPELL_RANGE)
                {
                    FireballExplosion(Game1.currentLocation, (int)currentPos.X, (int)currentPos.Y, Game1.player);
                    Game1.currentLocation.projectiles.Remove(fireball);
                    toRemove.Add(fireball);
                }
            }

            // Clean up removed fireballs
            foreach (var fireball in toRemove)
            {
                fireballStarts.Remove(fireball);
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

            // Calculate direction to mouse
            Vector2 direction = mouseWorldPos - playerPos;

            // Normalize direction for velocity
            if (direction.Length() > 0)
            {
                direction.Normalize();
            }
            else
            {
                // If clicking directly on player, default to facing direction
                direction = Game1.player.FacingDirection switch
                {
                    0 => new Vector2(0, -1),  // Up
                    1 => new Vector2(1, 0),   // Right
                    2 => new Vector2(0, 1),   // Down
                    3 => new Vector2(-1, 0),  // Left
                    _ => new Vector2(0, -1)
                };
            }

            Vector2 velocity = direction * FIREBALL_SPEED;

            Vector2 startPos = new Vector2(
                Game1.player.Position.X + 16,  // Center horizontally
                Game1.player.Position.Y - 16   // Slightly above center
            );

            DebugLogger($"Casting fireball! Direction: {direction}, Max range: {MAX_SPELL_RANGE}px");

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
                collisionBehavior: FireballExplosion, // Explode on early collision
                explode: false,
                damagesMonsters: true,
                location: Game1.currentLocation,
                firer: Game1.player
            );

            // Add to world
            Game1.currentLocation.projectiles.Add(fireball);

            // Track start position for max range calculation
            fireballStarts[fireball] = startPos;
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