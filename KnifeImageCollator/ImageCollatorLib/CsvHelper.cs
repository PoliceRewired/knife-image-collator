using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Amazon.S3.Transfer;
using CsvHelper;
using CsvHelper.Configuration;

namespace ImageCollatorLib
{
    public class CsvHelper
    {
        public static List<MinimalTweetDTO> ReadCsv(Stream input)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = (args) => args.Header.ToLower()
            };

            using (var reader = new StreamReader(input))
            {
                using (var csv = new CsvReader(reader, config))
                {
                    return csv.GetRecords<MinimalTweetDTO>().ToList();
                }
            }
        }

        public static void AppendCsvFile(string path, params MinimalTweetDTO[] tweets)
        {
            if (!File.Exists(path))
            {
                using (var writer = new StreamWriter(path))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(tweets);
                    }
                }
            }
            else
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };
                using (var stream = File.Open(path, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteRecords(tweets);
                }
            }
        }
    }
}
