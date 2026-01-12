using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Magicka.Animations;
using Magicka.Projectiles;

namespace Magicka
{
    /// <summary>
    /// Main entry point for the Magicka mod
    /// </summary>
    public class ModEntry : Mod
    {
        private SpellManager? _spellManager;
        private SpellTomeManager? _spellTomeManager;
        private AnimationManager? _animationManager;

        // Expose AnimationManager for accessing animations (e.g., from SpellTome)
        public static AnimationManager? AnimationManager { get; private set; }

        public override void Entry(IModHelper helper)
        {
            // Initialize managers
            _spellManager = new SpellManager();
            _spellTomeManager = new SpellTomeManager(this.Monitor);
            _animationManager = new AnimationManager(this.Monitor);
            AnimationManager = _animationManager; // Expose for static access

            // Register SpellTome animation with automatic play/stop condition
            _animationManager.RegisterAnimation(
                "spelltome",
                helper,
                "assets/spelltome.png",
                frameWidth: 16,
                frameTime: 250f, // 4fps
                shouldPlay: () => Game1.player?.CurrentTool is SpellTome
            );

            // Load custom firebolt sprite for FireballProjectile - REQUIRED, no fallback
            try
            {
                Texture2D fireboltTexture = helper.ModContent.Load<Texture2D>("assets/firebolt.png");
                FireballProjectile.SetCustomTexture(fireboltTexture);
                this.Monitor.Log("Custom firebolt sprite loaded successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"ERROR: Could not load firebolt sprite: {ex.Message}. Fireball projectiles will not render.", LogLevel.Error);
                // Don't allow mod to continue if sprite doesn't load - projectiles won't work
            }

            // Log initialization
            this.Monitor.Log("Magicka initialized", LogLevel.Info);

            // Subscribe to events
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            _spellTomeManager?.GiveSpellTome();
        }


        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return;
            if (Game1.player == null || Game1.currentLocation == null) return;

            // Check if player has Spell Tome equipped
            if (Game1.player.CurrentTool is SpellTome)
            {
                // Create Interactive Spell Chooser
                // Q = Spell Chooser

                // Left click = Fireball
                if (e.Button == SButton.MouseLeft)
                {
                    _spellManager?.CastSpellTowardMouse("fireball", Game1.player, Game1.currentLocation);
                    return; // Prevent default action
                }

            }
        }

        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.currentLocation == null) return;

            // Update spell manager (handles projectile tracking and max range)
            _spellManager?.Update(Game1.currentLocation);

            // Update all animations (handles frame updates and automatic play/stop)
            _animationManager?.Update(Game1.currentGameTime);
        }

    }
}


