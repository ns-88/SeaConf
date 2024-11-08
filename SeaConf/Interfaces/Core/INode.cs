using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeaConf.Interfaces.Core
{
    /// <summary>
    /// Configuration tree element.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Getting child elements.
        /// </summary>
        /// <returns>Child elements.</returns>
        ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync();
    }
}