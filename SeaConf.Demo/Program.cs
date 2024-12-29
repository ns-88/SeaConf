using System.Net;
using SeaConf.Demo.Models;
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;
using SeaConf.Models;

namespace SeaConf.Demo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = ConfigurationBuilder.New
                .WithSource(Sources.Xml.LocalAppDataPath("CompanyName", "AppName"))
                .WithModel<IProgramSettings, ProgramSettings>()
                .WithModel<IStreetSettings, StreetSettings>()
                .WithValueProviderFactory(new EmailValueProviderFactory())
                .Build();

            configuration.Loaded += ConfigurationLoaded;
            configuration.Saved += ConfigurationSaved;
            configuration.PropertyChanged += ConfigurationPropertyChanged;

            var programSettings = configuration.GetModel<IProgramSettings>();
            var streetSettings = configuration.GetModel<IStreetSettings>();
            
            await configuration.LoadAsync();

            programSettings.StringValue = "test1 test2 test3 test4";
            programSettings.BytesValue = new ReadOnlyMemory<byte>([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11]);
            programSettings.DoubleValue = 1.151;
            programSettings.IntValue = 13451;
            programSettings.LongValue = 578902319;
            programSettings.UlongValue = 894932319908124;
            programSettings.BoolValue = true;
            programSettings.EnumValue = Regime.Auto;
            programSettings.DecimalValue = 3925768910942182517;
            programSettings.DateTimeValue = new DateTime(2024, 01, 01, 09, 10, 30);
            programSettings.DateOnlyValue = new DateOnly(2024, 01, 01);
            programSettings.TimeOnlyValue = new TimeOnly(09, 10, 30);
            programSettings.IpEndPointValue = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 9001);
            programSettings.Email = new Email("test@test.com");

            programSettings.UserSettings.IntValue = 237;
            programSettings.UserSettings.StringValue = "UserStringValue";
            programSettings.UserSettings.LongValue = 578902319;

            programSettings.AddressSettings.IntValue = 238;
            programSettings.AddressSettings.StringValue = "AddressStringValue";
            programSettings.AddressSettings.LongValue = 578902319;

            programSettings.PressureSettings.IntValue = 239;
            programSettings.PressureSettings.StringValue = "PressureStringValue";
            programSettings.PressureSettings.LongValue = 578902319;

            programSettings.PressureSettings.ValveSettings.IntValue = 240;
            programSettings.PressureSettings.ValveSettings.StringValue = "ValveStringValue";
            programSettings.PressureSettings.ValveSettings.LongValue = 578902319;

            streetSettings.IntValue = 242;
            streetSettings.StringValue = "StreetStringValue";
            streetSettings.LongValue = 578902319;

            streetSettings.HomeSettings.IntValue = 243;
            streetSettings.HomeSettings.StringValue = "HomeStringValue";
            streetSettings.HomeSettings.LongValue = 578902319;

            foreach (var model in configuration.AsList())
            {
                Console.WriteLine($@"Model: {model.Name}.");

                foreach (var property in model.AsList())
                {
                    Console.WriteLine($@"Property: {property.Name}, value: {property.Get<object>()}.");
                }

                Console.WriteLine();
            }

            var programSettingsMap = ((IMemoryModel)programSettings).AsDictionary();
            var typedValue = programSettingsMap["StringValue"].ToTyped<string>();

            typedValue.Set("New value 1");

            var programSettingsList = ((IMemoryModel)programSettings).AsList();
            var changedProperties = programSettingsList.Where(x => x.IsModified);

            foreach (var property in changedProperties)
            {
                Console.WriteLine($@"Property: {property.Name}, value: {property.Get<object>()}.");
            }

            await configuration.SaveAsync();
        }

        private static void ConfigurationLoaded(object? sender, EventArgs e)
        {

        }

        private static void ConfigurationSaved(object? sender, SavedEventArgs e)
        {

        }

        private static void ConfigurationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

        }
    }
}