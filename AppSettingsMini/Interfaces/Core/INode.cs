using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppSettingsMini.Interfaces.Core
{
    public interface INode
    {
        string Name { get; }
        ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync();
    }
}