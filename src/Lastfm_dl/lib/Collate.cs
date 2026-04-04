using Newtonsoft.Json;
using System;

namespace Lastfm_dl 
{
    public class Collate
    {
        public Response Work(bool appendToExisting)
        {
            string[] scrobbleEvents = Directory.GetFiles(PathLib.ScrobblesPath, "*.json", SearchOption.AllDirectories);
            if (scrobbleEvents.Length == 0)
                return new Response { 
                    Succeeded = true,
                    Description = $"WARNING - No scrobble pages found"
                };

            List<Scrobble> scobbles = new List<Scrobble>();

            for (int i = 0; i < scrobbleEvents.Length ; i ++)
            {
                string filePath = scrobbleEvents[i];

                string fileContent = File.ReadAllText(filePath);

                List<Scrobble> page = JsonConvert.DeserializeObject<List<Scrobble>>(fileContent);
                scobbles = scobbles.Concat(page).ToList();
                Console.WriteLine($"Collating page {(i+1)} of {scrobbleEvents.Length}");
            }

            if (scobbles.Count() == 0){
                Console.WriteLine("No new/additional scrobbles downloaded, nothing to do.");
                return new Response { Succeeded = true };
            }

            Console.WriteLine($"Processing {scobbles.Count()} new/additional scrobbles, spread over {scrobbleEvents.Length} page(s).");

            string collatedFilePath = PathLib.CollatedFilePath;

            if (appendToExisting && File.Exists(collatedFilePath))
            {
                string collatedText = File.ReadAllText(collatedFilePath);
                List<Scrobble> collated = JsonConvert.DeserializeObject<List<Scrobble>>(collatedText);
                scobbles = scobbles.Concat(collated).ToList();

                Console.WriteLine($"Merged {collated.Count()} previously downloaded scrobbles with newly-downloaded scrobbles.");
            }

            scobbles = scobbles.OrderByDescending(s => s.Timestamp).ToList();

            File.WriteAllText(collatedFilePath, JsonConvert.SerializeObject(scobbles, Formatting.Indented));
            
            Console.WriteLine($"Saved {scobbles.Count()} scrobbles in total.");

            // todo : flesh this out
            return new Response{ Succeeded = true};
        }
    }
}