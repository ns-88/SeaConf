namespace SeaConf.Core
{
    /// <summary>
    /// Elements number in model - properties and other models.
    /// </summary>
    public readonly struct ElementsCount
    {
        /// <summary>
        /// Properties.
        /// </summary>
        public readonly int PropertiesCount;

        /// <summary>
        /// Models.
        /// </summary>
        public readonly int ModelsCount;

        public ElementsCount(int propertiesCount, int modelsCount)
        {
            PropertiesCount = propertiesCount;
            ModelsCount = modelsCount;
        }
    }
}