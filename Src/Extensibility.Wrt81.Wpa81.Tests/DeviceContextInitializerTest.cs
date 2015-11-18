namespace Microsoft.HockeyApp.Extensibility
{
    using System.Text;
    using System.Threading.Tasks;
    using DataContracts;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

    using global::Windows.Security.Cryptography;
    using global::Windows.Security.Cryptography.Core;
    using global::Windows.Storage.Streams;
    using global::Windows.System.Profile;

    using Assert = Xunit.Assert;

    /// <summary>
    /// Windows-specific tests for <see cref="DeviceContextInitializer"/>.
    /// </summary>
    public partial class DeviceContextInitializerTest
    {
        [TestMethod]
        public async Task ReadingDeviceUniqueIdYieldsCorrectValue()
        {
            DeviceContextInitializer source = new DeviceContextInitializer();
            var telemetryContext = new TelemetryContext();

            Assert.Null(telemetryContext.Device.Id);

            await source.Initialize(telemetryContext);

            string id = telemetryContext.Device.Id;

            Assert.Equal(GetDeviceId(), id);
        }

        private static string GetDeviceId()
        {
            // Per documentation here http://msdn.microsoft.com/en-us/library/windows/apps/jj553431.aspx we are selectively pulling out 
            // specific items from the hardware ID.
            StringBuilder builder = new StringBuilder();
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            using (DataReader dataReader = DataReader.FromBuffer(token.Id))
            {
                int offset = 0;
                while (offset < token.Id.Length)
                {
                    // The first two bytes contain the type of the component and the next two bytes contain the value.
                    byte[] hardwareEntry = new byte[4];
                    dataReader.ReadBytes(hardwareEntry);

                    if ((hardwareEntry[0] == 1 ||  // CPU ID of the processor
                         hardwareEntry[0] == 2 ||  // Size of the memory
                         hardwareEntry[0] == 3 ||  // Serial number of the disk device
                         hardwareEntry[0] == 7 ||  // Mobile broadband ID
                         hardwareEntry[0] == 9) && // BIOS
                        hardwareEntry[1] == 0)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(',');
                        }

                        builder.Append(hardwareEntry[2]);
                        builder.Append('_');
                        builder.Append(hardwareEntry[3]);
                    }

                    offset += 4;
                }
            }

            // create a buffer containing the cleartext device ID
            IBuffer clearBuffer = CryptographicBuffer.ConvertStringToBinary(builder.ToString(), BinaryStringEncoding.Utf8);

            // get a provider for the SHA256 algorithm
            HashAlgorithmProvider hashAlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm("SHA256");

            // hash the input buffer
            IBuffer hashedBuffer = hashAlgorithmProvider.HashData(clearBuffer);

            return CryptographicBuffer.EncodeToBase64String(hashedBuffer);            
        }
    }
}
