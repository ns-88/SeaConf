namespace AppSettingsMini.Interfaces.ValueProviders
{
    public interface IValueProvider
    {
        Type Type { get; }

        ValueTask<IPropertyData> GetAsync(string propertyName);
        ValueTask SetAsync(IPropertyData value);
    }
}