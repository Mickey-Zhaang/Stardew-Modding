using StardewModdingAPI;
using StardewValley;

namespace Magicka
{
    /// <summary>
    /// Manages Spell Tome item operations
    /// </summary>
    public class SpellTomeManager
    {
        private readonly IMonitor _monitor;

        public SpellTomeManager(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Gives a Spell Tome to the player
        /// </summary>
        public void GiveSpellTome()
        {
            if (!StardewModdingAPI.Context.IsWorldReady)
            {
                this._monitor.Log("You must be in-game to use this command.", LogLevel.Warn);
                return;
            }

            if (Game1.player == null)
            {
                _monitor.Log("Player is null!", LogLevel.Error);
                return;
            }

            try
            {
                SpellTome tome = new SpellTome();

                bool addedToInventory = Game1.player.addItemToInventoryBool(tome);

                if (!addedToInventory)
                {
                    _monitor.Log("Inventory is full! Spell Tome dropped at your feet.", LogLevel.Warn);
                    Game1.createItemDebris(tome, Game1.player.getStandingPosition(), -1, null);
                }
            }
            catch (System.Exception ex)
            {
                _monitor.Log($"Error creating SpellTome: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }
        }
    }
}

