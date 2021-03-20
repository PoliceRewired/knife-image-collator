using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Helpers
{
    public class CsvFileHelper
    {
        public static List<MediaDetails> ReadCsvText(string input)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = (args) => args.Header.ToLower()
            };

            using (var reader = new StreamReader(GenerateStreamFromString(input)))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    return csv.GetRecords<MediaDetails>().ToList();
                }
            }
        }

        public static async Task<string> CsvTextFromMediaAsync(IEnumerable<MediaDetails> medias)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, config))
                {
                    await csv.WriteRecordsAsync(medias);
                    await csv.FlushAsync();
                    stream.Position = 0;
                    return Encoding.UTF8.GetString(stream.GetBuffer());
                }
            }
        }

        public static Stream GenerateStreamFromString(string input)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(input);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static List<MediaDetails> ReadCsv(Stream input)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = (args) => args.Header.ToLower()
            };

            using (var reader = new StreamReader(input))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    return csv.GetRecords<MediaDetails>().ToList();
                }
            }
        }

        public static void AppendCsvFile(string path, IEnumerable<MediaDetails> medias)
        {
            var exists = File.Exists(path);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !exists,
            };
            using (var stream = File.Open(path, exists ? FileMode.Append : FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(medias);
            }
        }
    }
}
