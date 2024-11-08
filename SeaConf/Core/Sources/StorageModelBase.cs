using System.Collections.Generic;
using System.Threading.Tasks;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core.Sources
{
    /// <summary>
    /// Configuration data model in storage.
    /// </summary>
    public abstract class StorageModelBase : INode, IStorageModel
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Path.
        /// </summary>
        public ModelPath Path { get; }

        protected StorageModelBase(string name, ModelPath path)
        {
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Getting child elements.
        /// </summary>
        /// <returns>Child elements.</returns>
        public abstract ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync();

        /// <summary>
        /// Creating a writer.
        /// </summary>
        public abstract IWriter CreateWriter();

        /// <summary>
        /// Creating a reader.
        /// </summary>
        public abstract IReader CreateReader();

        /// <summary>
        /// Loading.
        /// </summary>
        public virtual ValueTask LoadAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Saving.
        /// </summary>
        public virtual ValueTask SaveAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}