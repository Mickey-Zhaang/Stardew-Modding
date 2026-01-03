using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Magicka.Animations;

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

        // Animation for SpellTome
        public static SpriteAnimation? SpellTomeAnimation { get; private set; }

        public override void Entry(IModHelper helper)
        {
            // Initialize managers
            _spellManager = new SpellManager();
            _spellTomeManager = new SpellTomeManager(this.Monitor);
            _animationManager = new AnimationManager(this.Monitor);

            // Register SpellTome animation
            SpellTomeAnimation = _animationManager.RegisterAnimation(
                "spelltome",
                helper,
                "assets/spelltome.png",
                frameWidth: 16,
                frameTime: 250f // 4fps
            );

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

            // Update all animations
            _animationManager?.Update(Game1.currentGameTime);

            // Control SpellTome animation based on whether player is holding it
            if (SpellTomeAnimation != null)
            {
                bool isHoldingTome = Game1.player?.CurrentTool is SpellTome;

                if (isHoldingTome && !SpellTomeAnimation.IsPlaying)
                {
                    SpellTomeAnimation.Play();
                }
                else if (!isHoldingTome && SpellTomeAnimation.IsPlaying)
                {
                    SpellTomeAnimation.Stop();
                }
            }
        }

    }
}


