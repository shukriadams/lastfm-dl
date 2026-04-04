using Newtonsoft.Json;
using System.IO;

namespace Lastfm_data_downloader
{
    public class SessionLib
    {
        public void Update(Session session)
        {
            // write session back to disk
            string sessionFilePath = Path.Join(PathLib.SessionPath, "session.json");

            try
            {

                File.WriteAllText(sessionFilePath, JsonConvert.SerializeObject(session, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update session {sessionFilePath}\n{ex.Message}");
                throw;
            }
        }

        public SessionInitializeResponse Initialize(
            int currentPagesCount, 
            bool ignorePagesMismatch,
            bool forceOverWriteExistingSession)
        {

            if (forceOverWriteExistingSession && Directory.Exists(PathLib.SessionPath))
            {
                try 
                {
                    Directory.Delete(PathLib.SessionPath);
                    Console.WriteLine($"Forced wiped existing session dir {PathLib.SessionPath}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error deleting dir {PathLib.SessionPath} : {ex.Message}"
                    };
                }
            }

            if (!Directory.Exists(PathLib.SessionPath))
            {
                try 
                {
                    Directory.CreateDirectory(PathLib.SessionPath);
                    Directory.CreateDirectory(PathLib.ScrobblesPath);
                    Console.WriteLine($"Created session dir {PathLib.SessionPath}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error creating dir {PathLib.SessionPath} : {ex.Message}"
                    };
                }
            }

            Session session;
            bool isSessionContinued = false;
            string sessionFilePath = Path.Join(PathLib.SessionPath, "session.json");

            try 
            {
                if (File.Exists(sessionFilePath))
                {
                    session = JsonConvert.DeserializeObject<Session>(File.ReadAllText(sessionFilePath));
                    isSessionContinued = true;
                }
                else 
                {
                    // no session file found, create a new one
                    session = new Session {
                        Started = DateTime.Now,
                        CurrentPage = 1,
                        TotalPages = currentPagesCount
                    };
                }
            }
            catch(Exception ex)
            {
                return new SessionInitializeResponse
                {
                    Description = $"Error reading exsting session file {sessionFilePath} : {ex.Message}"
                };
            }

            int pagesWhenSessionCreated = session.TotalPages;
            if (!ignorePagesMismatch && pagesWhenSessionCreated != currentPagesCount)
                return new SessionInitializeResponse{
                    Description = $"Current session was started when lastfm total scrobble playcount was {pagesWhenSessionCreated}, but current total page count is {currentPagesCount}. Aborting download. Use --ignorePageCountMismatch to ignore this error. Note that you the greater the page mismatch, the more plays you will lose from your history."
                };

            string warning = string.Empty;
            if (pagesWhenSessionCreated != currentPagesCount)
                warning = $"Current session was started when lastfm total scrobble playcount was {pagesWhenSessionCreated}, but current total page count is {currentPagesCount}. You have opted to ignore this mismatch. The greater the page mismatch, the more plays you will lose from your history.";

            // write session back to disk
            try
            {
                File.WriteAllText(sessionFilePath, JsonConvert.SerializeObject(session, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return new SessionInitializeResponse {
                    Description = $"Failed to write session file {PathLib.SessionPath}\n{ex.Message}"
                };
            }

            // copy collation file into session directory
            string collatedfile = PathLib.CollatedFilePath;
            string collatedfileCopied = Path.Join(PathLib.ScrobblesPath, "collated.json");
            Scrobble limit = null;
           
            if (File.Exists(collatedfile))
            {
                string fileContent = File.ReadAllText(collatedfile);
                List<Scrobble> collated = JsonConvert.DeserializeObject<List<Scrobble>>(fileContent);
                limit = collated.OrderByDescending(s => s.TimestampDT).FirstOrDefault();
            }

            return new SessionInitializeResponse {
                Warning = warning,
                Succeeded = true,
                Limit = limit,
                IsSessionContinued = isSessionContinued,
                Session = session
            };
        }

        /// Wipes current session
        public Response Remove()
        {
            string sessionDirectory = PathLib.SessionPath;
            Directory.Delete(sessionDirectory, true);

            return new Response {
                Succeeded = true
            };
        }
    }
}