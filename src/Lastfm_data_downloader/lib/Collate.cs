using Newtonsoft.Json;
using System;

namespace Lastfm_data_downloader 
{
    public class Collate
    {
        public void Work()
        {
            string[] scrobbleEvents = Directory.GetFiles("./working/scrobbles", "*.*", SearchOption.AllDirectories);
            Console.WriteLine($"Collating {scrobbleEvents.Length} pages");
            List<Scrobble> scobbles = new List<Scrobble>();

            for (int i = 0; i < scrobbleEvents.Length ; i ++)
            {
                string filePath = scrobbleEvents[i];

                string fileContent = File.ReadAllText(filePath);

                List<Scrobble> page = JsonConvert.DeserializeObject<List<Scrobble>>(fileContent);
                scobbles = scobbles.Concat(page).ToList();
            }
            string collatedFilePath = $"./working/all_scrobbles.json";

            File.WriteAllText(collatedFilePath, JsonConvert.SerializeObject(scobbles, Formatting.Indented));
        }
    }
}