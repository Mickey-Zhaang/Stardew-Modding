using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Numerics;
using StardewValley.Objects;
using StardewValley.Monsters;

namespace AutoTool
{
    /// TODOS:
    /// ( optimization ) Preload player inventory into hash map and continuously update it (still constant space complexity as limited inventory space)
    ///                  - ( Time ) Makes finding tool to switch to: O(n) -> O(1)
    ///                  - ( Space ) Still O(1)
    /// 
    /// ( energy )       Find a way to prevent tool usage if no action can be done with the given tools in inventory
    ///                  - Is it even possible to stop an action?
    ///                  - Also what parameters would we even have to consider?

    public class ModEntry : Mod
    {
        bool verbose = false;

        // Buffer time in milliseconds to prevent tool switching too quickly
        private const int TOOL_SWITCH_BUFFER_MS = 500;

        // Track when the last tool switch occurred
        private double _lastToolSwitchTime = 0;

        public override void Entry(IModHelper helper)
        {
            // Log Initialization
            this.Monitor.Log("AutoTool initialized.", LogLevel.Debug);

            // In Game Subscriptions
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        }

        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            // Update last tool switch time if player is using a tool
            if (Game1.player != null && Game1.player.UsingTool)
            {
                _lastToolSwitchTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null) return;

            if (e.Button.IsUseToolButton())
            {
                // Check if we should prevent tool switching
                if (ShouldPreventToolSwitch())
                {
                    DebugLogger("Tool switch prevented - player is using tool or buffer time not elapsed");
                    return;
                }

                string? tool = DetermineRequiredTool(e.Cursor.GrabTile);
                if (tool != null)
                {
                    // Only switch if it's a different tool
                    if (GetCurrentToolType() != tool)
                    {
                        if (SwitchTools(tool))
                        {
                            _lastToolSwitchTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Checks if tool switching should be prevented (player using tool or buffer time not elapsed)
        /// </summary>
        private bool ShouldPreventToolSwitch()
        {
            if (Game1.player == null) return true;

            // Prevent if player is currently using a tool
            if (Game1.player.UsingTool)
            {
                return true;
            }

            // Prevent if buffer time hasn't elapsed since last switch
            double currentTime = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
            double timeSinceLastSwitch = currentTime - _lastToolSwitchTime;

            if (timeSinceLastSwitch < TOOL_SWITCH_BUFFER_MS)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current tool type as a string (for comparison)
        /// </summary>
        private string? GetCurrentToolType()
        {
            if (Game1.player?.CurrentTool == null) return null;

            var tool = Game1.player.CurrentTool;
            if (tool is MeleeWeapon melee && melee.isScythe()) return "Scythe";
            if (tool is MeleeWeapon) return "MeleeWeapon";
            if (tool is Pickaxe) return "Pickaxe";
            if (tool is Axe) return "Axe";

            return null;
        }

        /// <summary>
        /// Helper method to SwitchTools. Essentially helps determine what tool is needed for when we try to use a tool on a given tile
        /// </summary>
        /// <param name="tile">The given tile to determine tools</param>
        /// <returns>( string? ) A tool name to feed into SwitchTools()</returns>
        private string? DetermineRequiredTool(Microsoft.Xna.Framework.Vector2 tile)
        {

            // Check debris objects
            var obj = Game1.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y);
            if (obj != null)
            {
                DebugLogger($"Found obj: {obj.Name}");
                return obj switch
                {
                    _ when obj.IsWeeds() => "Scythe",
                    _ when obj.IsBreakableStone() => "Pickaxe",
                    _ when obj.IsTwig() => "Axe",
                    _ when obj is BreakableContainer => "MeleeWeapon",
                    // Objects like Chests, machines, etc. don't need tools - return null
                    _ => null
                };
            }

            // Check Terrain Features
            if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var feature))
            {
                DebugLogger($"Found Feature: {feature}");
                // Check for trees
                if (feature is Tree || feature is FruitTree)
                    return "Axe";

                // Check for grasses
                if (feature is Grass)
                    return "Scythe";
            }

            // Check resource clumps
            var clump = GetResourceClumpAtTile(tile);
            if (clump != null)
            {
                return clump.parentSheetIndex.Value switch
                {
                    600 or 602 => "Axe",
                    148 or 622 or 672 or 752 or 754 or 756 or 758 => "Pickaxe",
                    44 or 46 => "Scythe",
                    // Unknown resource clump - return null instead of throwing
                    _ => null
                };
            }

            // check for monsters last
            NPC? monster = GetMonsterNearby(tile);
            if (monster != null)
            {
                return "MeleeWeapon";
            }

            return null;
        }


        /// <summary>
        /// Switches tools automatically for players
        /// </summary>
        /// <param name="toolName">The name of tool to be swapped</param>
        /// <returns>( bool ) Whether or not we can swap tools</returns>
        private bool SwitchTools(string toolName)
        {

            // look through player's inventory
            for (int i = 0; i < Game1.player.Items.Count; i++)
            {
                var item = Game1.player.Items[i];

                if (item is Tool tool)
                {
                    // Check to see if player has tool
                    bool hasTool = toolName switch
                    {
                        "Scythe" => tool is MeleeWeapon melee && melee.isScythe(),
                        "MeleeWeapon" => tool is MeleeWeapon melee,
                        "Pickaxe" => tool is Pickaxe,
                        "Axe" => tool is Axe,
                        _ => false
                    };

                    // if player has tool -> swap to that tool's index
                    if (hasTool)
                    {
                        DebugLogger($"Found tool to use -> switching to {tool.Name}");
                        Game1.player.CurrentToolIndex = i;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the resource clump at given tile
        /// </summary>
        /// <param name="tile">The tile we will be checking</param>
        /// <returns> ( ResourceClump? ) The resource clump at our given tile</returns>
        private ResourceClump? GetResourceClumpAtTile(Microsoft.Xna.Framework.Vector2 tile)
        {
            // Check through all current location's resource clumps
            foreach (var resourceClump in Game1.currentLocation.resourceClumps)
            {
                if (resourceClump.occupiesTile((int)tile.X, (int)tile.Y))
                {
                    // return's clump at our current tile
                    DebugLogger($"Found Clump: {resourceClump.parentSheetIndex.Value}");
                    return resourceClump;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the monster near the given tile or null
        /// </summary>
        /// <param name="tile">The given tile to search around</param>
        /// <param name="radius">The radius for searching for monsters</param>
        /// <returns>( NPC? ) Returns the monster found</returns>
        private NPC? GetMonsterNearby(Microsoft.Xna.Framework.Vector2 tile, int radius = 1)
        {
            int tileSize = 64;
            Microsoft.Xna.Framework.Rectangle checkArea = new Microsoft.Xna.Framework.Rectangle(
                (int)(tile.X * tileSize) - tileSize,
                (int)(tile.Y * tileSize) - tileSize,
                tileSize * 3,
                tileSize * 3
            );

            foreach (NPC character in Game1.currentLocation.characters)
            {
                if (character.IsMonster && character.GetBoundingBox().Intersects(checkArea))
                {
                    DebugLogger($"Found Monster: {character.Name}");
                    return character;
                }
            }
            return null;
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
