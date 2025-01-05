using Moq;
using SeaConf.Core;
using SeaConf.Core.Sources;
using SeaConf.Interfaces.Core;

namespace SeaConf.Test.Infrastructure
{
    internal class SynchronizedStorageSource : SourceBase<IStorageModel>, IStorageSource
    {
        private readonly Dictionary<ModelPath, IStorageModel> _models;

        public SynchronizedStorageSource()
        {
            _models = new Dictionary<ModelPath, IStorageModel>();
        }

        /// <summary>
        /// Getting root configuration elements.
        /// </summary>
        /// <returns>Root configuration elements.</returns>
        public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
        {
            return ValueTask.FromResult<IReadOnlyList<INode>>(_models
                .Where(x => x.Value.Path.Count == 1)
                .Select(x => (INode)x.Value)
                .ToList()
            );
        }

        /// <summary>
        /// Loading.
        /// </summary>
        public ValueTask LoadAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Saving.
        /// </summary>
        public ValueTask SaveAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Adding a data model.
        /// </summary>
        /// <param name="path">Unique path.</param>
        /// <returns>Created data model.</returns>
        public ValueTask<IStorageModel> AddModelAsync(ModelPath path)
        {
            var model = new SynchronizedStorageModel(path[^1], path, _models);

            _models.Add(path, model);

            return ValueTask.FromResult<IStorageModel>(model);
        }

        /// <summary>
        /// Deleting a data model.
        /// </summary>
        /// <param name="path">Unique path.</param>
        public ValueTask DeleteModelAsync(ModelPath path)
        {
            _models.Remove(path);
            return ValueTask.CompletedTask;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.</summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        #region Nested types

        private class SynchronizedStorageModel : StorageModelBase
        {
            private readonly IWriter _writer;
            private readonly IReader _reader;
            private readonly IReadOnlyDictionary<ModelPath, IStorageModel> _models;
            private readonly List<IProperty> _properties;

            public SynchronizedStorageModel(string name, ModelPath path, IReadOnlyDictionary<ModelPath, IStorageModel> models) : base(name, path)
            {
                _writer = Mock.Of<IWriter>();
                _reader = Mock.Of<IReader>();
                _models = models;
                _properties = new List<IProperty>();
            }

            /// <summary>
            /// Getting child elements.
            /// </summary>
            /// <returns>Child elements.</returns>
            public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
            {
                var nodes = new List<INode>();

                foreach (var model in _models)
                {
                    if (model.Value.Path.IsIncluded(Path, limit: 1))
                    {
                        nodes.Add((INode)model.Value);
                    }
                }

                return ValueTask.FromResult<IReadOnlyList<INode>>(nodes);
            }

            /// <summary>
            /// Adding property.
            /// </summary>
            /// <param name="propertyInfo">Information about the stored property.</param>
            public override ValueTask AddPropertyAsync(IProperty propertyInfo)
            {
                _properties.Add(propertyInfo);
                return ValueTask.CompletedTask;
            }

            /// <summary>
            /// Deleting property.
            /// </summary>
            /// <param name="propertyInfo">Information about the stored property.</param>
            public override ValueTask DeletePropertyAsync(IProperty propertyInfo)
            {
                return ValueTask.CompletedTask;
            }

            /// <summary>
            /// Getting all properties.
            /// </summary>
            /// <returns>All properties.</returns>
            public override IEnumerable<IProperty> GetProperties()
            {
                return _properties;
            }

            /// <summary>
            /// Creating a writer.
            /// </summary>
            public override IWriter CreateWriter()
            {
                return _writer;
            }

            /// <summary>
            /// Creating a reader.
            /// </summary>
            public override IReader CreateReader()
            {
                return _reader;
            }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                return $"Name = {Name}";
            }
        }

        #endregion
    }
}