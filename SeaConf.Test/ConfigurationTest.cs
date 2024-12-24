using System.Net;
using AutoFixture;
using AutoFixture.Kernel;
using Moq;
using NUnit.Framework;
using SeaConf.Core;
using SeaConf.Interfaces.Core;
using SeaConf.Models;
using SeaConf.Test.Infrastructure;
using SeaConf.Test.Infrastructure.Models;

namespace SeaConf.Test
{
    [TestFixture]
    public class ConfigurationTest
    {
        private readonly Fixture _fixture;

        public ConfigurationTest()
        {
            _fixture = new Fixture();

            _fixture.Customizations.Add(new TypeRelay(typeof(IUserSettings), typeof(MockUserSettings)));
            _fixture.Customizations.Add(new TypeRelay(typeof(IAddressSettings), typeof(MockAddressSettings)));
        }

        [Test]
        public void Load_ValidData_Success()
        {
            // #### Arrange ####
            const string enumValueText = "Auto";
            var factory = MockSourceFactory.CreateFactory(out _, out var readerMocks);
            var configuration = ConfigurationBuilder.New
                .WithSource(factory)
                .WithModel<IProgramSettings, ProgramSettings>()
                .Build();
            var eventLoadedRaised = false;
            var programSettings = configuration.GetModel<IProgramSettings>();

            var expectedResult = _fixture
                .Build<MockProgramSettings>()
                .With(x => x.DateOnlyValue, new DateOnly(2024, 01, 01))
                .With(x => x.TimeOnlyValue, new TimeOnly(09, 10, 30))
                .With(x => x.EnumValue, Regime.Auto)
                .Create();

            configuration.Loaded += (_, _) => eventLoadedRaised = true;

            #region ProgramSettings

            readerMocks.Program
                .Setup(x => x.PropertyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            readerMocks.Program
                .Setup(x => x.ReadBytesAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.BytesValue);

            readerMocks.Program
                .Setup(x => x.ReadIntAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.IntValue);

            readerMocks.Program
                .Setup(x => x.ReadLongAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.LongValue);

            readerMocks.Program
                .Setup(x => x.ReadUlongAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UlongValue);

            readerMocks.Program
                .Setup(x => x.ReadDecimalAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.DecimalValue);

            readerMocks.Program
                .Setup(x => x.ReadDoubleAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.DoubleValue);

            readerMocks.Program
                .Setup(x => x.ReadStringAsync(nameof(IProgramSettings.StringValue)))
                .ReturnsAsync(expectedResult.StringValue!);

            readerMocks.Program
                .Setup(x => x.ReadStringAsync(nameof(IProgramSettings.EnumValue)))
                .ReturnsAsync(enumValueText);

            readerMocks.Program
                .Setup(x => x.ReadStringAsync(nameof(IProgramSettings.DateTimeValue)))
                .ReturnsAsync(expectedResult.DateTimeValue.ToString);

            readerMocks.Program
                .Setup(x => x.ReadStringAsync(nameof(IProgramSettings.DateOnlyValue)))
                .ReturnsAsync(expectedResult.DateOnlyValue.ToString);

            readerMocks.Program
                .Setup(x => x.ReadStringAsync(nameof(IProgramSettings.TimeOnlyValue)))
                .ReturnsAsync(expectedResult.TimeOnlyValue.ToString);

            readerMocks.Program
                .Setup(x => x.ReadStringAsync(nameof(IProgramSettings.IpEndPointValue)))
                .ReturnsAsync(expectedResult.IpEndPointValue!.ToString);

            readerMocks.Program
                .Setup(x => x.ReadBooleanAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.BoolValue);

            #endregion

            #region ProgramSettings\UserSettings

            readerMocks.User
                .Setup(x => x.PropertyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            readerMocks.User
                .Setup(x => x.ReadIntAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UserSettings.IntValue);

            readerMocks.User
                .Setup(x => x.ReadLongAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UserSettings.LongValue);

            readerMocks.User
                .Setup(x => x.ReadStringAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UserSettings.StringValue!);

            #endregion

            #region ProgramSettings\UserSettings\AddressSettings

            readerMocks.Address
                .Setup(x => x.PropertyExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            readerMocks.Address
                .Setup(x => x.ReadIntAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UserSettings.AddressSettings.IntValue);

            readerMocks.Address
                .Setup(x => x.ReadLongAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UserSettings.AddressSettings.LongValue);

            readerMocks.Address
                .Setup(x => x.ReadStringAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResult.UserSettings.AddressSettings.StringValue!);

            #endregion

            // #### Act/Assert ####
            Assert.DoesNotThrowAsync(async () => await configuration.LoadAsync());
            Assert.That(eventLoadedRaised);
            Assert.That(programSettings, Is.EqualTo(expectedResult).Using(new LoadEqualityComparer()));

            #region ProgramSettings

            readerMocks.Program
                .Verify(x => x.PropertyExistsAsync(It.IsAny<string>()), Times.Exactly(13));

            readerMocks.Program
                .Verify(x => x.ReadBytesAsync(nameof(IProgramSettings.BytesValue)), Times.Once);

            readerMocks.Program
                .Verify(x => x.ReadIntAsync(nameof(IProgramSettings.IntValue)), Times.Once);

            readerMocks.Program
                .Verify(x => x.ReadLongAsync(nameof(IProgramSettings.LongValue)), Times.Once);

            readerMocks.Program
                .Verify(x => x.ReadUlongAsync(nameof(IProgramSettings.UlongValue)), Times.Once);

            readerMocks.Program
                .Verify(x => x.ReadDoubleAsync(nameof(IProgramSettings.DoubleValue)), Times.Once);

            readerMocks.Program
                .Verify(x => x.ReadDecimalAsync(nameof(IProgramSettings.DecimalValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadStringAsync(nameof(IProgramSettings.StringValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadStringAsync(nameof(IProgramSettings.DateTimeValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadStringAsync(nameof(IProgramSettings.DateOnlyValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadStringAsync(nameof(IProgramSettings.TimeOnlyValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadStringAsync(nameof(IProgramSettings.IpEndPointValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadStringAsync(nameof(IProgramSettings.EnumValue)), Times.Exactly(1));

            readerMocks.Program
                .Verify(x => x.ReadBooleanAsync(nameof(IProgramSettings.BoolValue)), Times.Once);

            readerMocks.Program.VerifyNoOtherCalls();

            #endregion

            #region ProgramSettings\UserSettings

            readerMocks.User
                .Verify(x => x.PropertyExistsAsync(It.IsAny<string>()), Times.Exactly(3));

            readerMocks.User
                .Verify(x => x.ReadIntAsync(It.IsAny<string>()), Times.Once);

            readerMocks.User
                .Verify(x => x.ReadLongAsync(It.IsAny<string>()), Times.Once);

            readerMocks.User
                .Verify(x => x.ReadStringAsync(It.IsAny<string>()), Times.Once);

            readerMocks.User.VerifyNoOtherCalls();

            #endregion

            #region ProgramSettings\UserSettings\AddressSettings

            readerMocks.Address
                .Verify(x => x.PropertyExistsAsync(It.IsAny<string>()), Times.Exactly(3));

            readerMocks.Address
                .Verify(x => x.ReadIntAsync(It.IsAny<string>()), Times.Once);

            readerMocks.Address
                .Verify(x => x.ReadLongAsync(It.IsAny<string>()), Times.Once);

            readerMocks.Address
                .Verify(x => x.ReadStringAsync(It.IsAny<string>()), Times.Once);

            readerMocks.Address.VerifyNoOtherCalls();

            #endregion
        }

        [Test]
        public void Save_ValidData_Success()
        {
            // #### Arrange ####
            var factory = MockSourceFactory.CreateFactory(out var mockWriter, out _);
            var configuration = ConfigurationBuilder.New
                .WithSource(factory)
                .WithModel<IProgramSettings, ProgramSettings>()
                .Build();
            var eventSavedRaised = false;
            var methodArgs = new Dictionary<string, Dictionary<string, object>>();
            var programSettings = configuration.GetModel<IProgramSettings>();

            var expectedChangedProperties = new List<string>
            {
                nameof(IProgramSettings.StringValue),
                nameof(IProgramSettings.IntValue),
                nameof(IProgramSettings.LongValue),
                nameof(IProgramSettings.UlongValue),
                nameof(IProgramSettings.DoubleValue),
                nameof(IProgramSettings.BoolValue),
                nameof(IProgramSettings.EnumValue),
                nameof(IProgramSettings.BytesValue),
                nameof(IProgramSettings.DecimalValue),
                nameof(IProgramSettings.DateTimeValue),
                nameof(IProgramSettings.DateOnlyValue),
                nameof(IProgramSettings.TimeOnlyValue),
                nameof(IProgramSettings.IpEndPointValue),

                nameof(IUserSettings.IntValue),
                nameof(IUserSettings.LongValue),
                nameof(IUserSettings.StringValue),

                nameof(IAddressSettings.IntValue),
                nameof(IAddressSettings.LongValue),
                nameof(IAddressSettings.StringValue)
            };

            SavedEventArgs? changedModelsEventArgs = null;

            var expectedResult = _fixture
                .Build<MockProgramSettings>()
                .With(x => x.DateOnlyValue, new DateOnly(2024, 01, 01))
                .With(x => x.TimeOnlyValue, new TimeOnly(09, 10, 30))
                .With(x => x.BoolValue, true)
                .With(x => x.EnumValue, Regime.Auto)
                .Create();

            configuration.Saved += (_, e) =>
            {
                eventSavedRaised = true;
                changedModelsEventArgs = e;
            };

            #region ProgramSettings

            mockWriter.Program
                .Setup(x => x.WriteBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteUlongAsync(It.IsAny<ulong>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteDecimalAsync(It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteDoubleAsync(It.IsAny<double>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.StringValue)))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.EnumValue)))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateTimeValue)))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateOnlyValue)))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.TimeOnlyValue)))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.IpEndPointValue)))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            mockWriter.Program
                .Setup(x => x.WriteBooleanAsync(It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IProgramSettings))));

            #endregion

            #region ProgramSettings\UserSettings

            mockWriter.User
                .Setup(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IUserSettings))));

            mockWriter.User
                .Setup(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IUserSettings))));

            mockWriter.User
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IUserSettings))));

            #endregion

            #region ProgramSettings\UserSettings\AddressSettings

            mockWriter.Address
                .Setup(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IAddressSettings))));

            mockWriter.Address
                .Setup(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IAddressSettings))));

            mockWriter.Address
                .Setup(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ValueTask.CompletedTask)
                .Callback(new InvocationAction(x => AddArgs(x, nameof(IAddressSettings))));

            #endregion

            // #### Act ####
            programSettings.BytesValue = expectedResult.BytesValue;
            programSettings.IntValue = expectedResult.IntValue;
            programSettings.LongValue = expectedResult.LongValue;
            programSettings.UlongValue = expectedResult.UlongValue;
            programSettings.DoubleValue = expectedResult.DoubleValue;
            programSettings.StringValue = expectedResult.StringValue;
            programSettings.BoolValue = expectedResult.BoolValue;
            programSettings.EnumValue = expectedResult.EnumValue;
            programSettings.DecimalValue = expectedResult.DecimalValue;
            programSettings.DateTimeValue = expectedResult.DateTimeValue;
            programSettings.DateOnlyValue = expectedResult.DateOnlyValue;
            programSettings.TimeOnlyValue = expectedResult.TimeOnlyValue;
            programSettings.IpEndPointValue = expectedResult.IpEndPointValue;
            programSettings.UserSettings.IntValue = expectedResult.UserSettings.IntValue;
            programSettings.UserSettings.LongValue = expectedResult.UserSettings.LongValue;
            programSettings.UserSettings.StringValue = expectedResult.UserSettings.StringValue;
            programSettings.UserSettings.AddressSettings.IntValue = expectedResult.UserSettings.AddressSettings.IntValue;
            programSettings.UserSettings.AddressSettings.LongValue = expectedResult.UserSettings.AddressSettings.LongValue;
            programSettings.UserSettings.AddressSettings.StringValue = expectedResult.UserSettings.AddressSettings.StringValue;

            // #### Act/Assert ####
            Assert.DoesNotThrowAsync(async () => await configuration.SaveAsync());
            Assert.That(eventSavedRaised);
            Assert.That(changedModelsEventArgs!.ChangedModels.HasChanged);

            var programSettingsHasChanged = changedModelsEventArgs.ChangedModels.TryGetProperties<IProgramSettings>("ProgramSettings", out var programSettingsActualChangedProperties);
            var userSettingsHasChanged = changedModelsEventArgs.ChangedModels.TryGetProperties<IUserSettings>("UserSettings", out var userSettingsActualChangedProperties);
            var addressSettingsHasChanged = changedModelsEventArgs.ChangedModels.TryGetProperties<IAddressSettings>("AddressSettings", out var addressSettingsActualChangedProperties);

            Assert.That(programSettingsHasChanged);
            Assert.That(userSettingsHasChanged);
            Assert.That(addressSettingsHasChanged);

            var actualChangedProperties = programSettingsActualChangedProperties!
                .Concat(userSettingsActualChangedProperties!)
                .Concat(addressSettingsActualChangedProperties!);

            Assert.That(expectedChangedProperties, Is.EquivalentTo(actualChangedProperties.Select(x => x.Name)));
            Assert.That(methodArgs, Is.EqualTo(expectedResult).Using(new SaveEqualityComparer()));

            #region ProgramSettings

            mockWriter.Program
                .Verify(x => x.WriteBytesAsync(It.IsAny<ReadOnlyMemory<byte>>(), nameof(IProgramSettings.BytesValue)), Times.Once);

            mockWriter.Program
                .Verify(x => x.WriteIntAsync(It.IsAny<int>(), nameof(IProgramSettings.IntValue)), Times.Once);

            mockWriter.Program
                .Verify(x => x.WriteLongAsync(It.IsAny<long>(), nameof(IProgramSettings.LongValue)), Times.Once);

            mockWriter.Program
                .Verify(x => x.WriteUlongAsync(It.IsAny<ulong>(), nameof(IProgramSettings.UlongValue)), Times.Once);

            mockWriter.Program
                .Verify(x => x.WriteDoubleAsync(It.IsAny<double>(), nameof(IProgramSettings.DoubleValue)), Times.Once);

            mockWriter.Program
                .Verify(x => x.WriteDecimalAsync(It.IsAny<decimal>(), nameof(IProgramSettings.DecimalValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.StringValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateTimeValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.DateOnlyValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.TimeOnlyValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.IpEndPointValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), nameof(IProgramSettings.EnumValue)), Times.Exactly(1));

            mockWriter.Program
                .Verify(x => x.WriteBooleanAsync(It.IsAny<bool>(), nameof(IProgramSettings.BoolValue)), Times.Once);

            mockWriter.Program.VerifyNoOtherCalls();

            #endregion

            #region ProgramSettings\UserSettings

            mockWriter.User
                .Verify(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);

            mockWriter.User
                .Verify(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Once);

            mockWriter.User
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            mockWriter.User.VerifyNoOtherCalls();

            #endregion

            #region ProgramSettings\UserSettings\AddressSettings

            mockWriter.Address
                .Verify(x => x.WriteIntAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Once);

            mockWriter.Address
                .Verify(x => x.WriteLongAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Once);

            mockWriter.Address
                .Verify(x => x.WriteStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            mockWriter.Address.VerifyNoOtherCalls();

            #endregion

            return;

            void AddArgs(IInvocation invocation, string modelName)
            {
                if (methodArgs.TryGetValue(modelName, out var modelArgs))
                {
                    modelArgs.Add((string)invocation.Arguments[1], invocation.Arguments[0]);
                }
                else
                {
                    modelArgs = new Dictionary<string, object> { { (string)invocation.Arguments[1], invocation.Arguments[0] } };
                    methodArgs.Add(modelName, modelArgs);
                }
            }
        }

        [Test]
        public void ChangeProperties_RaisePropertyChangedEvent_Success()
        {
            // #### Arrange ####
            var factory = MockSourceFactory.CreateFactory(out _, out _);
            var configuration = ConfigurationBuilder.New
                .WithSource(factory)
                .WithModel<IProgramSettings, ProgramSettings>()
                .Build();
            var changedProperties = new List<PropertyChangedEventArgs>();
            var programSettings = configuration.GetModel<IProgramSettings>();

            var settings = _fixture
                .Build<MockProgramSettings>()
                .With(x => x.DateOnlyValue, new DateOnly(2024, 01, 01))
                .With(x => x.TimeOnlyValue, new TimeOnly(09, 10, 30))
                .With(x => x.EnumValue, Regime.Auto)
                .Create();

            configuration.PropertyChanged += (_, e) => changedProperties.Add(e);

            // #### Act ####
            programSettings.BytesValue = settings.BytesValue;
            programSettings.IntValue = settings.IntValue;
            programSettings.LongValue = settings.LongValue;
            programSettings.UlongValue = settings.UlongValue;
            programSettings.DoubleValue = settings.DoubleValue;
            programSettings.StringValue = settings.StringValue;
            programSettings.BoolValue = settings.BoolValue;
            programSettings.EnumValue = settings.EnumValue;
            programSettings.DecimalValue = settings.DecimalValue;
            programSettings.DateTimeValue = settings.DateTimeValue;
            programSettings.DateOnlyValue = settings.DateOnlyValue;
            programSettings.TimeOnlyValue = settings.TimeOnlyValue;
            programSettings.IpEndPointValue = settings.IpEndPointValue;
            programSettings.UserSettings.IntValue = settings.UserSettings.IntValue;
            programSettings.UserSettings.LongValue = settings.UserSettings.LongValue;
            programSettings.UserSettings.StringValue = settings.UserSettings.StringValue;
            programSettings.UserSettings.AddressSettings.IntValue = settings.UserSettings.AddressSettings.IntValue;
            programSettings.UserSettings.AddressSettings.LongValue = settings.UserSettings.AddressSettings.LongValue;
            programSettings.UserSettings.AddressSettings.StringValue = settings.UserSettings.AddressSettings.StringValue;

            // #### Assert ####
            Assert.That(changedProperties.Count == 19);

            #region ProgramSettings
            {
                Assert.That(changedProperties[0].TryGetValue<ReadOnlyMemory<byte>, IProgramSettings>("ProgramSettings", out var bytesValue, nameof(IProgramSettings.BytesValue)));
                Assert.That(settings.BytesValue, Is.EqualTo(bytesValue));

                Assert.That(changedProperties[1].TryGetValue<int, IProgramSettings>("ProgramSettings", out var intValue, nameof(IProgramSettings.IntValue)));
                Assert.That(settings.IntValue, Is.EqualTo(intValue));

                Assert.That(changedProperties[2].TryGetValue<long, IProgramSettings>("ProgramSettings", out var longValue, nameof(IProgramSettings.LongValue)));
                Assert.That(settings.LongValue, Is.EqualTo(longValue));

                Assert.That(changedProperties[3].TryGetValue<ulong, IProgramSettings>("ProgramSettings", out var ulongValue, nameof(IProgramSettings.UlongValue)));
                Assert.That(settings.UlongValue, Is.EqualTo(ulongValue));

                Assert.That(changedProperties[4].TryGetValue<double, IProgramSettings>("ProgramSettings", out var doubleValue, nameof(IProgramSettings.DoubleValue)));
                Assert.That(settings.DoubleValue, Is.EqualTo(doubleValue));

                Assert.That(changedProperties[5].TryGetValue<string, IProgramSettings>("ProgramSettings", out var stringValue, nameof(IProgramSettings.StringValue)));
                Assert.That(settings.StringValue, Is.EqualTo(stringValue));

                Assert.That(changedProperties[6].TryGetValue<bool, IProgramSettings>("ProgramSettings", out var boolValue, nameof(IProgramSettings.BoolValue)));
                Assert.That(settings.BoolValue, Is.EqualTo(boolValue));

                Assert.That(changedProperties[7].TryGetValue<Regime, IProgramSettings>("ProgramSettings", out var enumValue, nameof(IProgramSettings.EnumValue)));
                Assert.That(settings.EnumValue, Is.EqualTo(enumValue));

                Assert.That(changedProperties[8].TryGetValue<decimal, IProgramSettings>("ProgramSettings", out var decimalValue, nameof(IProgramSettings.DecimalValue)));
                Assert.That(settings.DecimalValue, Is.EqualTo(decimalValue));

                Assert.That(changedProperties[9].TryGetValue<DateTime, IProgramSettings>("ProgramSettings", out var dateTimeValue, nameof(IProgramSettings.DateTimeValue)));
                Assert.That(settings.DateTimeValue, Is.EqualTo(dateTimeValue));

                Assert.That(changedProperties[10].TryGetValue<DateOnly, IProgramSettings>("ProgramSettings", out var dateOnlyValue, nameof(IProgramSettings.DateOnlyValue)));
                Assert.That(settings.DateOnlyValue, Is.EqualTo(dateOnlyValue));

                Assert.That(changedProperties[11].TryGetValue<TimeOnly, IProgramSettings>("ProgramSettings", out var timeOnlyValue, nameof(IProgramSettings.TimeOnlyValue)));
                Assert.That(settings.TimeOnlyValue, Is.EqualTo(timeOnlyValue));

                Assert.That(changedProperties[12].TryGetValue<IPEndPoint, IProgramSettings>("ProgramSettings", out var ipEndPointValue, nameof(IProgramSettings.IpEndPointValue)));
                Assert.That(settings.IpEndPointValue, Is.EqualTo(ipEndPointValue));
            }
            #endregion

            #region ProgramSettings\UserSettings
            {
                Assert.That(changedProperties[13].TryGetValue<int, IUserSettings>("UserSettings", out var intValue, nameof(IUserSettings.IntValue)));
                Assert.That(settings.UserSettings.IntValue, Is.EqualTo(intValue));

                Assert.That(changedProperties[14].TryGetValue<long, IUserSettings>("UserSettings", out var longValue, nameof(IUserSettings.LongValue)));
                Assert.That(settings.UserSettings.LongValue, Is.EqualTo(longValue));

                Assert.That(changedProperties[15].TryGetValue<string, IUserSettings>("UserSettings", out var stringValue, nameof(IUserSettings.StringValue)));
                Assert.That(settings.UserSettings.StringValue, Is.EqualTo(stringValue));
            }
            #endregion

            #region ProgramSettings\UserSettings\AddressSettings
            {
                Assert.That(changedProperties[16].TryGetValue<int, IAddressSettings>("AddressSettings", out var intValue, nameof(IAddressSettings.IntValue)));
                Assert.That(settings.UserSettings.AddressSettings.IntValue, Is.EqualTo(intValue));

                Assert.That(changedProperties[17].TryGetValue<long, IAddressSettings>("AddressSettings", out var longValue, nameof(IAddressSettings.LongValue)));
                Assert.That(settings.UserSettings.AddressSettings.LongValue, Is.EqualTo(longValue));

                Assert.That(changedProperties[18].TryGetValue<string, IAddressSettings>("AddressSettings", out var stringValue, nameof(IAddressSettings.StringValue)));
                Assert.That(settings.UserSettings.AddressSettings.StringValue, Is.EqualTo(stringValue));
            }
            #endregion
        }

        [Test]
        public async Task SynchronizationModels_WithSave_Success()
        {
            // #### Arrange ####
            var factory = MockSourceFactory.CreateFactory();
            var storageSource = factory.StorageSource;
            var configuration = ConfigurationBuilder.New
                .WithSource(factory)
                .WithModel<IProgramSettings, ProgramSettings>()
                .WithSyncMode(SyncMode.Enable)
                .Build();

            var expectedRootNodes = new List<ModelPath> { new("ProgramSettings") };
            var expectedStorageModels = new List<ModelPath>
            {
                new("ProgramSettings"),
                new("UserSettings", new ModelPath("ProgramSettings")),
                new("AddressSettings", new ModelPath("UserSettings", new ModelPath("ProgramSettings")))
            };

            // #### Act/Assert ####
            Assert.DoesNotThrowAsync(async () => await configuration.SaveAsync());

            var actualRootNodes = await storageSource.GetRootNodesAsync();
            var actualStorageModels = await storageSource.GetModelsAsync(actualRootNodes).ToListAsync();

            var rootNodesCount = actualRootNodes
                .Select(x => ((IStorageModel)x).Path)
                .Join(expectedRootNodes, lhs => lhs, rhs => rhs, (lhs, _) => lhs)
                .Count();

            var storageModelsCount = actualStorageModels
                .Select(x => x.Path)
                .Join(expectedStorageModels, lhs => lhs, rhs => rhs, (lhs, _) => lhs)
                .Count();

            Assert.That(actualRootNodes.Count == expectedRootNodes.Count);
            Assert.That(actualStorageModels.Count == expectedStorageModels.Count);
            Assert.That(rootNodesCount == expectedRootNodes.Count);
            Assert.That(storageModelsCount == expectedStorageModels.Count);
        }

        [Test]
        public async Task SynchronizationModels_WithLoad_Success()
        {
            // #### Arrange ####
            var factory = MockSourceFactory.CreateFactory();
            var storageSource = factory.StorageSource;
            var configuration = ConfigurationBuilder.New
                .WithSource(factory)
                .WithModel<IProgramSettings, ProgramSettings>()
                .WithSyncMode(SyncMode.Enable)
                .Build();

            var expectedRootNodes = new List<ModelPath> { new("ProgramSettings") };
            var expectedStorageModels = new List<ModelPath>
            {
                new("ProgramSettings"),
                new("UserSettings", new ModelPath("ProgramSettings")),
                new("AddressSettings", new ModelPath("UserSettings", new ModelPath("ProgramSettings")))
            };

            // #### Act/Assert ####
            Assert.DoesNotThrowAsync(async () => await configuration.LoadAsync());

            var actualRootNodes = await storageSource.GetRootNodesAsync();
            var actualStorageModels = await storageSource.GetModelsAsync(actualRootNodes).ToListAsync();

            var rootNodesCount = actualRootNodes
                .Select(x => ((IStorageModel)x).Path)
                .Join(expectedRootNodes, lhs => lhs, rhs => rhs, (lhs, _) => lhs)
                .Count();

            var storageModelsCount = actualStorageModels
                .Select(x => x.Path)
                .Join(expectedStorageModels, lhs => lhs, rhs => rhs, (lhs, _) => lhs)
                .Count();

            Assert.That(actualRootNodes.Count == expectedRootNodes.Count);
            Assert.That(actualStorageModels.Count == expectedStorageModels.Count);
            Assert.That(rootNodesCount == expectedRootNodes.Count);
            Assert.That(storageModelsCount == expectedStorageModels.Count);
        }
    }
}