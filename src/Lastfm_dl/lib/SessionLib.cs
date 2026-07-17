using Newtonsoft.Json;
using System.IO;

namespace Lastfm_dl
{
    public class SessionLib
    {
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// Init
        /// </summary>
        public SessionInitializeResponse Initialize(
            int currentPagesCount, 
            string user,
            bool ignorePagesMismatch,
            bool clearSession,
            string path)
        {
            string sessionPath = PathLib.SessionPath(path);

            // delete current session if it exists
            if (clearSession && Directory.Exists(sessionPath))
            {
                try 
                {
                    Directory.Delete(sessionPath, true);
                    Console.WriteLine($"Clearing existing session dir {sessionPath}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error deleting dir {sessionPath} : {ex.Message}"
                    };
                }
            }

            // create a new session directory if needed
            if (!Directory.Exists(sessionPath))
            {
                try 
                {
                    Directory.CreateDirectory(sessionPath);
                    Directory.CreateDirectory(PathLib.ScrobblesPath(path));
                    Console.WriteLine($"Created session dir {sessionPath}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error creating dir {sessionPath} : {ex.Message}"
                    };
                }
            }

            // create scrobbles dir
            string scrobblesDir = PathLib.ScrobblesPath(path);
            if (!Directory.Exists(scrobblesDir))
            {
                try 
                {
                    Directory.CreateDirectory(scrobblesDir);
                    Console.WriteLine($"Created session scrobbles dir {scrobblesDir}.");
                }
                catch(Exception ex)
                {
                    return new SessionInitializeResponse
                    {
                        Description = $"Error creating dir {scrobblesDir} : {ex.Message}"
                    };
                }
            }

            // read session json file on disk, if it exists, This contains info about the session.
            Session session;
            bool isSessionContinued = false;
            string sessionFilePath = Path.Join(sessionPath, "session.json");
    
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

            // ensure user in session matches current user
            if (!string.IsNullOrEmpty(session.User) && session.User != user)
                return new SessionInitializeResponse
                {
                    Description = $"The existing session was created for a different user {session.User}. Remove this session first before switching users."
                };


            // write updated session back to disk
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
            int pages = 0;

            if (File.Exists(collatedfile))
            {
                string fileContent = File.ReadAllText(collatedfile);
                Collation collation = JsonConvert.DeserializeObject<Collation>(fileContent);
                pages = collation.Pages;
                limit = collation.Scrobbles.OrderByDescending(s => s.TimestampDT).FirstOrDefault();
            }

            return new SessionInitializeResponse {
                Succeeded = true,
                Limit = limit,
                Pages = pages,
                IsSessionContinued = isSessionContinued,
                Session = session
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public SessionGetResponse GetSession(string workpath)
        {
    
            string sessionFilePath = Path.Join(PathLib.SessionPath(workpath), "session.json");
            Session session = null;
            
            try 
            {
                if (File.Exists(sessionFilePath))
                    session = JsonConvert.DeserializeObject<Session>(File.ReadAllText(sessionFilePath));

            }
            catch(Exception ex)
            {
                return new SessionGetResponse
                {
                    Description = $"Error reading exsting session file {sessionFilePath} : {ex.Message}"
                };
            }

            string collationFilePath = PathLib.CollatedFilePath(workpath);
            Collation collation = null;

            try 
            {
                if (File.Exists(collationFilePath))
                    collation = JsonConvert.DeserializeObject<Collation>(File.ReadAllText(collationFilePath));
            }
            catch(Exception ex)
            {
                return new SessionGetResponse
                {
                    Description = $"Error reading exsting session file {sessionFilePath} : {ex.Message}"
                };
            }

            return new SessionGetResponse {
                Succeeded = true,
                Collation = collation,
                Session = session
            };
        }

        /// <summary>
        /// Wipes current session
        /// </summary>
        public Response Remove(string workPath)
        {
            string sessionDirectory = PathLib.SessionPath(workPath);
            Directory.Delete(sessionDirectory, true);

            return new Response {
                Succeeded = true
            };
        }
    }
}