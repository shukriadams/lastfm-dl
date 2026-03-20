using Newtonsoft.Json;
using System;

namespace Lastfm_data_downloader 
{
    public class Collate
    {
        public void Work()
        {
            string[] scrobbleEvents = Directory.GetFiles("./working/scrobbles", "*.*", SearchOption.AllDirectories);
            Console.WriteLine($"Found {scrobbleEvents.Length} pages");
            List<Scrobble> scobbles = new List<Scrobble>();

            for (int i = 0; i < scrobbleEvents.Length ; i ++)
            {
                string filePath = scrobbleEvents[i];

                string fileContent = File.ReadAllText(filePath);

                List<Scrobble> page = JsonConvert.DeserializeObject<List<Scrobble>>(fileContent);
                scobbles = scobbles.Concat(page).ToList();
                Console.WriteLine($"Collating page {(i+1)} of {scrobbleEvents.Length}");
            }

            string collatedFilePath = $"./working/all_scrobbles.json";
            scobbles = scobbles.OrderBy(s => s.Page).ThenBy(s => s.Index).ToList();

            File.WriteAllText(collatedFilePath, JsonConvert.SerializeObject(scobbles, Formatting.Indented));
            Console.WriteLine($"Finished collating {scrobbleEvents.Length} pages, {scobbles.Count()} scrobbles.");
        }
    }
}