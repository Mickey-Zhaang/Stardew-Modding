using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace AutoTool
{

    public class ModEntry : Mod
    {
        bool verbose = false;

        public override void Entry(IModHelper helper)
        {
            // Log Initialization
            this.Monitor.Log("AutoTool initialized.", LogLevel.Debug);

            // In Game Subscriptions
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            this.Helper.ConsoleCommands.Add("spawn", "Spawns a monster near the player. Usage: spawn [type] [count]", this.SpawnMonster);
            this.Monitor.Log("  - 'spawn [type] [count]' - Spawn monsters (e.g., 'spawnmonster slime 3')", LogLevel.Info);

        }

        private void SpawnMonster(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                this.Monitor.Log("You must be in-game to use this command.", LogLevel.Warn);
                return;
            }

            if (Game1.player == null || Game1.currentLocation == null)
            {
                this.Monitor.Log("Player or location is null!", LogLevel.Error);
                return;
            }

            // deconstruct args
            string monsterType = args.Length > 0 ? args[0].ToLower() : "slime";
            int count = args.Length > 1 && int.TryParse(args[1], out int parsedCount) ? parsedCount : 1;

            // Get player position in tiles
            Vector2 playerTile = new Vector2(
                (int)(Game1.player.Position.X / 64f),
                (int)(Game1.player.Position.Y / 64f)
            );

            // Spawn monsters in a circle around the player
            for (int i = 0; i < count; i++)
            {
                Monster? monster = null;
                Vector2 spawnPosition = playerTile;

                // Calculate spawn position in a circle around player
                if (count > 1)
                {
                    float angle = (float)(2 * Math.PI * i / count);
                    float radius = 2f; // 2 tiles away
                    spawnPosition = new Vector2(
                        playerTile.X + (float)Math.Cos(angle) * radius,
                        playerTile.Y + (float)Math.Sin(angle) * radius
                    );
                }
                else
                {
                    // Single monster spawns 2 tiles in front of player
                    spawnPosition = playerTile + new Vector2(0, -2);
                }

                // Create monster based on type
                monster = monsterType switch
                {
                    "slime" => new GreenSlime(spawnPosition * 64f, 0),
                    "skeleton" => new Skeleton(),
                    _ => new GreenSlime(spawnPosition * 64f, 0)
                };
                if (monster != null)
                {
                    Game1.currentLocation.characters.Add(monster);
                    DebugLogger($"Monster added: {monster.Name}");
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
