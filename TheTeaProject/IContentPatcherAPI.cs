using System.Collections.Generic;
using StardewModdingAPI;

namespace TheTeaProject
{
    /// <summary>
    /// Interface for Content Patcher's API
    /// Minimal interface with only the methods we actually use
    /// </summary>
    public interface IContentPatcherAPI
    {
        /// <summary>
        /// Register a simple token.
        /// </summary>
        /// <param name="mod">The manifest of the mod defining the token.</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>YourName.YourMod/SomeToken</c>.</param>
        /// <param name="getValue">A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context.</param>
        void RegisterToken(IManifest mod, string name, System.Func<IEnumerable<string>> getValue);
    }
}
