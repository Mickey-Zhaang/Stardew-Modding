using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace TheTeaProject
{
    public class ModEntry : Mod
    {
        bool verbose = false;
        private IContentPatcherAPI? _contentPatcher;
        private Texture2D? _teaCatalogueTexture;

        public override void Entry(IModHelper helper)
        {
            // Log Initialization\
            DebugLogger("TheTeaProject initialized.");

            // Load tea catalogue texture
            try
            {
                _teaCatalogueTexture = helper.ModContent.Load<Texture2D>("assets/tea_catalogue.png");
            }
            catch (Exception ex)
            {
                DebugLogger($"Error loading tea catalogue texture: {ex.Message}");
            }

            // Subscribe to events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        /// <summary>
        /// Called when the game launches - good time to register Content Patcher tokens
        /// Content Patcher API is now available after all mods are initialized
        /// </summary>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // Get Content Patcher API (now that all mods are initialized)
            _contentPatcher = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (_contentPatcher == null)
            {
                DebugLogger("Content Patcher API not available. Some features may not work.");
            }
            else
            {
                DebugLogger("Content Patcher API loaded successfully.");
                RegisterContentPatcherTokens();
            }
        }

        /// <summary>
        /// Called when a save is loaded - good time to trigger content reloads if needed
        /// </summary>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (_contentPatcher != null)
            {
                // Example: Trigger a content reload if conditions changed
                // _contentPatcher.InvalidateCache();
                DebugLogger("Save loaded - Content Patcher patches are active.");
            }
        }

        /// <summary>
        /// Register custom tokens for Content Patcher to use in content.json
        /// Only register tokens if you need dynamic values that change based on game state
        /// </summary>
        private void RegisterContentPatcherTokens()
        {
            if (_contentPatcher == null) return;

            // Example: Register a token for dynamic values
            // Use tokens when values need to change based on game state
            // For static values, just hardcode them in content.json instead
            _contentPatcher.RegisterToken(this.ModManifest, "TeaLevel", () =>
            {
                // Return a token value that Content Patcher can use
                if (Context.IsWorldReady && Game1.player != null)
                {
                    // Example: Return player's tea level or some game state
                    // Return as IEnumerable<string> for Content Patcher
                    return new[] { "5" }; // Example value
                }
                return new[] { "0" }; // Default value when not in game
            });
            DebugLogger("Registered custom Content Patcher tokens.");
        }

        /// <summary>
        /// Handle button presses (e.g., give tool to player for testing)
        /// </summary>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null) return;

            // Example: Press 'Q' to give Tea Catalogue (for testing)
            if (e.Button == SButton.Q && _teaCatalogueTexture != null)
            {
                int frameWidth = _teaCatalogueTexture.Width / 3; // 3 frames horizontally
                TeaCatalogue catalogue = new TeaCatalogue(_teaCatalogueTexture, frameWidth);

                // Set texture if not already set
                if (catalogue.GetTexture() == null)
                {
                    catalogue.SetTexture(_teaCatalogueTexture, frameWidth);
                }

                if (Game1.player.addItemToInventoryBool(catalogue))
                {
                    DebugLogger("Tea Catalogue added to inventory.");
                }
                else
                {
                    DebugLogger("Inventory full - Tea Catalogue dropped at feet.");
                    Game1.createItemDebris(catalogue, Game1.player.getStandingPosition(), -1, null);
                }
            }
        }

        /// <summary>
        /// Debug statement toggler
        /// </summary>
        /// <param name="statement">Debug statement that will be logged to the Monitor</param>
        private void DebugLogger(string statement)
        {
            if (verbose)
                this.Monitor.Log($"Debug statement: {statement}", LogLevel.Debug);
        }
    }
}
