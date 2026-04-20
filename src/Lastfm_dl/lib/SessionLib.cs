using Newtonsoft.Json;
using System.IO;

namespace Lastfm_dl
{
    public class SessionLib
    {
        public void Update(Session session, string path)
        {
            // write session back to disk
            string sessionFilePath = Path.Join(PathLib.SessionPath(path), "session.json");

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
            bool clearSession,
            string path)
        {
            string workingPath = PathLib.SessionPath(path);

            if (clearSession && Directory.Exists(workingPath))
            {
                try 
                {
                    Directory.Delete(workingPath, true);
                    Console.WriteLine($"Clearing existing session dir {workingPath}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error deleting dir {workingPath} : {ex.Message}"
                    };
                }
            }

            if (!Directory.Exists(workingPath))
            {
                try 
                {
                    Directory.CreateDirectory(workingPath);
                    Directory.CreateDirectory(PathLib.ScrobblesPath(path));
                    Console.WriteLine($"Created session dir {workingPath}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error creating dir {workingPath} : {ex.Message}"
                    };
                }
            }

            Session session;
            bool isSessionContinued = false;
            string sessionFilePath = Path.Join(workingPath, "session.json");

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
                    Description = $"Current session was started when lastfm total scrobble playcount was {pagesWhenSessionCreated}, but current total page count is {currentPagesCount}. Aborting download. Use --ignore to ignore this error, or --clear to start a new session. Note that you the greater the page mismatch, the more plays you will lose from your history."
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

            // check if collated file exists. If so, session is a top-up import, find the latest record in the collated
            // file and set that as limit
            string collatedfile = PathLib.CollatedFilePath(path);
            Scrobble limit = null;
            if (File.Exists(collatedfile))
            {
                string fileContent = File.ReadAllText(collatedfile);
                Collation collation = JsonConvert.DeserializeObject<Collation>(fileContent);
                limit = collation.Scrobbles.OrderByDescending(s => s.TimestampDT).FirstOrDefault();
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
        public Response Remove(string root)
        {
            string sessionDirectory = PathLib.SessionPath(root);
            Directory.Delete(sessionDirectory, true);

            return new Response {
                Succeeded = true
            };
        }
    }
}