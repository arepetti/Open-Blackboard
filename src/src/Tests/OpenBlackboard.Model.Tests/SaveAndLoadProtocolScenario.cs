using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class SaveAndLoadProtocolScenario
    {
        [Fact]
        public void WhenWriteJson_ThenOutputContainsExceptedText()
        {
            var json = SerializeProtocolAsJsonString(ProtocolFactory.CreateTest());

            // Raw validation using known substrings
            Assert.True(!String.IsNullOrWhiteSpace(json));
            Assert.Contains(ProtocolFactory.HeightFieldName, json);
            Assert.Contains(ProtocolFactory.WeightFieldName, json);
            Assert.Contains(ProtocolFactory.GenderFieldName, json);
        }

        [Fact]
        public void WhenReadingFromKnownJson_ThenProtocolIsWhatExpected()
        {
            var json = SerializeProtocolAsJsonString(ProtocolFactory.CreateTest());

            using (var reader = new StringReader(json))
            {
                var protocol = ProtocolDescriptorJsonStorage.Load(reader);
                ProtocolFactory.CheckConsistency(protocol);

                Assert.Equal(json, SerializeProtocolAsJsonString(protocol));
            }
        }

        [Fact]
        public void WhenSavingWithDefaultSettings_ThenOutputFileIsUtf8Encoded()
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                var protocol = ProtocolFactory.CreateTest();
                ProtocolDescriptorJsonStorage.Save(tempFilePath, protocol);

                Assert.True((new FileInfo(tempFilePath)).Length > 0);
                Assert.Equal(SerializeProtocolAsJsonString(protocol), File.ReadAllText(tempFilePath, Encoding.UTF8));
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public void WhenSavingAndLoadingWithUtf16Encoding_ThenRetrievedObjectIsWhatExpected()
        {
            var tempFilePath = Path.GetTempFileName();
            try
            {
                var json = SerializeProtocolAsJsonString(ProtocolFactory.CreateTest());
                File.WriteAllText(tempFilePath, json, Encoding.Unicode);
                ProtocolFactory.CheckConsistency(ProtocolDescriptorJsonStorage.Load(tempFilePath, Encoding.Unicode));
            }
            finally
            {
                File.Delete(tempFilePath);
            }

        }

        private static string SerializeProtocolAsJsonString(ProtocolDescriptor descriptor)
        {
            using (var writer = new StringWriter())
            {
                ProtocolDescriptorJsonStorage.Save(writer, descriptor);
                return writer.ToString();
            }
        }
    }
}
