using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
namespace ExhibitA
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //CsvHelper configurations for read date from file
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t",
            };

            using var streamReader = File.OpenText("files/exhibitA-input.csv");
            using var csvReader = new CsvReader(streamReader, csvConfig);

            //Read data into RecordModel object
            var records = csvReader.GetRecords<RecordModel>();

            //Filter data by date (10/08/2016) then grouping operations applied
            var result = records.Where(x => x.PLAY_TS.Date == DateTime.Parse("10/8/2016"))
                        .GroupBy(x => x.CLIENT_ID)
                        .Select(x => new
                        {
                            Client = x.Key,
                            Count = x.DistinctBy(a => a.SONG_ID).Count(),
                        }).GroupBy(
                            x => x.Count
                        )
                        .Select(x => new
                        ResultModel
                        {
                            CLIENT_COUNT = x.Key,
                            DISTINCT_PLAY_COUNT = x.Count()
                        });

            //Write data configurations
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" };

            //Write result data to new csv file with the tab delimiter
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter("files/result.csv"))
            using (var csvWriter = new CsvWriter(writer, config))
            {
                csvWriter.WriteHeader<ResultModel>();
                csvWriter.WriteRecords(result);
                writer.Flush();
            }
        }
    }
}
