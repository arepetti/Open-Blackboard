using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Provides static methods to load and save <see cref="ProtocolDescriptor"/> instances to a
    /// persistent storage medium encoded as a JSON object.
    /// </summary>
    public static class ProtocolDescriptorJsonStorage
    {
        /// <summary>
        /// Loads a new instance of <see cref="ProtocolDescriptor"/> from the specified reader.
        /// </summary>
        /// <param name="reader">Reader from which to read the protocol content.</param>
        /// <returns>
        /// A new instance of <see cref="ProtocolDescriptor"/> which content is loaded from
        /// specified text reader.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="reader"/> is <see langword="null"/>.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ProtocolDescriptor Load(TextReader reader)
        {
            using (var jsonReader = new JsonTextReader(reader))
            {
                return CreateSerializer().Deserialize<ProtocolDescriptor>(jsonReader);
            }
        }

        /// <summary>
        /// Loadds a new instance of <see cref="ProtocolDescriptor"/> from the specified path
        /// of a text file saved with the given encoding.
        /// </summary>
        /// <param name="path">Full path of the text file.</param>
        /// <param name="encoding">Encoding used for the text.</param>
        /// <returns>
        /// A new instance of <see cref="ProtocolDescriptor"/> which content is loaded from
        /// specified text file, encoded with the given encoding.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="encoding"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// If specified file does not exist or it is not accessible.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// If user has not enough privileges to read specified file.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ProtocolDescriptor Load(string path, Encoding encoding)
        {
            using (var reader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete), encoding))
            {
                return Load(reader);
            }
        }

        /// <summary>
        /// Loads a new instance of <see cref="ProtocolDescriptor"/> from the specified
        /// path of a  UTF-8 encoded text file.
        /// </summary>
        /// <param name="path">Full path of the text file.</param>
        /// <returns>
        /// A new instance of <see cref="ProtocolDescriptor"/> which content is loaded from
        /// specified UTF-8 encoded text file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// If specified file does not exist or it is not accessible.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// If user has not enough privileges to read specified file.
        /// </exception>
        public static ProtocolDescriptor Load(string path)
        {
            return Load(path, Encoding.UTF8);
        }

        /// <summary>
        /// Loads a new instance of <see cref="ProtocolDescriptor"/> from the specified
        /// path of a  UTF-8 encoded text file.
        /// </summary>
        /// <param name="path">Full path of the text file.</param>
        /// <returns>
        /// A new instance of <see cref="ProtocolDescriptor"/> which content is loaded from
        /// specified UTF-8 encoded text file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// If specified file does not exist or it is not accessible.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// If user has not enough privileges to read specified file.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static async Task<ProtocolDescriptor> LoadAsync(string path)
        {
            // TODO: check if there is any gain on reading from stream asynchronously. JSON parsing might be
            // a better candidate (or maybe there isn't any advantage at all...)
            using (var reader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete), Encoding.UTF8))
            {
                var content = reader.ReadToEndAsync();
                var serializer = CreateSerializer();

                using (var stringReader = new StringReader(await content))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return serializer.Deserialize<ProtocolDescriptor>(jsonReader);
                }
            }
        }

        /// <summary>
        /// Saves the content of specified protocol into given text writer.
        /// </summary>
        /// <param name="writer">Writer where protocol JSON representation should be saved to.</param>
        /// <param name="protocol">Protocol to save.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="writer"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="protocol"/> is <see langword="null"/>.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Save(TextWriter writer, ProtocolDescriptor protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            using (var jsonWriter = new JsonTextWriter(writer))
            {
                CreateSerializer().Serialize(jsonWriter, protocol, typeof(ProtocolDescriptor));
            }
        }

        /// <summary>
        /// Saves the content of the specified protocol into the specified file using the
        /// given text encoding.
        /// </summary>
        /// <param name="path">Full file path into which protocol JSON representation should be saved to.</param>
        /// <param name="encoding">Encoding to be used to encode text in specified file.</param>
        /// <param name="protocol">Protocol to save.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="encoding"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="protocol"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurred while opening or writing the specified file.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// If user has not enough privileges to write to the specified file.
        /// <br/>-or-<br/>
        /// If specified file already exists and it is <see cref="FileAttributes.Hidden"/>.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void Save(string path, Encoding encoding, ProtocolDescriptor protocol)
        {
            using (var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None), encoding))
            {
                Save(writer, protocol);
            }
        }

        /// <summary>
        /// Saves the content of the specified protocol into the specified file using UTF-8 encoding.
        /// </summary>
        /// <param name="path">Full path into which protocol JSON representation should be save to.</param>
        /// <param name="protocol">Protocol to save.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="path"/> is <see langword="null"/>.
        /// <br/>-or-<br/>
        /// If <paramref name="protocol"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="IOException">
        /// If an I/O error occurred while opening or writing the specified file.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// If user has not enough privileges to write to the specified file.
        /// <br/>-or-<br/>
        /// If specified file already exists and it is <see cref="FileAttributes.Hidden"/>.
        /// </exception>
        public static void Save(string path, ProtocolDescriptor protocol)
        {
            Save(path, Encoding.UTF8, protocol);
        }

        private static JsonSerializer CreateSerializer()
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.Converters.Add(new StringEnumConverter());

            return JsonSerializer.Create(settings);
        }
    }
}
