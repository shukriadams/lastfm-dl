using Newtonsoft.Json;
using System;

namespace Lastfm_dl 
{
    public class Collate
    {
        public Response Work(bool appendToExisting, string path)
        {
            string[] scrobbleEvents = Directory.GetFiles(PathLib.ScrobblesPath(path), "*.json", SearchOption.AllDirectories);
            if (scrobbleEvents.Length == 0)
                return new Response { 
                    Succeeded = true,
                    Description = $"WARNING - No scrobble pages found"
                };

            Collation collation = new Collation();

            for (int i = 0; i < scrobbleEvents.Length ; i ++)
            {
                string filePath = scrobbleEvents[i];

                string fileContent = File.ReadAllText(filePath);

                List<Scrobble> page = JsonConvert.DeserializeObject<List<Scrobble>>(fileContent);
                collation.Scrobbles = collation.Scrobbles.Concat(page);
                Console.WriteLine($"Collating page {(i+1)} of {scrobbleEvents.Length}");
            }

            if (collation.Scrobbles.Count() == 0)
            {
                Console.WriteLine("No new scrobbles downloaded, nothing to do.");
                return new Response { Succeeded = true };
            }

            Console.WriteLine($"Processing {collation.Scrobbles.Count()} new scrobbles, in {scrobbleEvents.Length} page(s)");

            string collatedFilePath = PathLib.CollatedFilePath(path);

            if (appendToExisting && File.Exists(collatedFilePath))
            {
                string collatedText = File.ReadAllText(collatedFilePath);
                Collation collated = JsonConvert.DeserializeObject<Collation>(collatedText);
                collation.Scrobbles = collation.Scrobbles.Concat(collated.Scrobbles);

                Console.WriteLine($"Merged {collated.Scrobbles.Count()} previously downloaded scrobbles");
            }

            collation.Scrobbles = collation.Scrobbles.OrderByDescending(s => s.TimestampDT);
            collation.ScrobbleCount = collation.Scrobbles.Count();
            collation.Date = DateTime.Now;
            File.WriteAllText(collatedFilePath, JsonConvert.SerializeObject(collation, Formatting.Indented));
            
            Console.WriteLine($"Saved {collation.Scrobbles.Count()} scrobbles in total at path {collatedFilePath}");

            // todo : flesh this out
            return new Response{ Succeeded = true};
        }
    }
}