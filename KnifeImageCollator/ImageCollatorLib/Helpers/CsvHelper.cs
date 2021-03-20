using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Helpers
{
    public class CsvHelper
    {
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
