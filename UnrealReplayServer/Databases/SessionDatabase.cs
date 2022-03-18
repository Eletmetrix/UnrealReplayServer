/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayServer.Connectors;
using UnrealReplayServer.Databases.Models;

namespace UnrealReplayServer.Databases
{
    public class SessionDatabase : ISessionDatabase
    {
        public readonly ApplicationDefaults _applicationSettings;

        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<Session> SessionList;

        private readonly int TimeoutOfLiveSession;

        public SessionDatabase(IOptions<ApplicationDefaults> connectionString)
        {
            _applicationSettings = connectionString.Value;

            client = new MongoClient(_applicationSettings.MongoDBConnection);
            database = client.GetDatabase(_applicationSettings.MongoDBDatabaseName);
            SessionList = database.GetCollection<Session>("SessionList");

            TimeoutOfLiveSession = _applicationSettings.TimeoutOfLiveSession * -1;
        }

        public async Task<string> CreateSession(string setSessionName, string setAppVersion, string setNetVersion, int? setChangelist,
            string setPlatformFriendlyName)
        {
            var values = await SessionList.Find(x => x.SessionName == setSessionName).ToListAsync();
            if (values.Count() > 0)
            {
                await SessionList.DeleteOneAsync(x => x.SessionName == setSessionName);
            }

            Session newSession = new Session()
            {
                AppVersion = setAppVersion,
                NetVersion = setNetVersion,
                PlatformFriendlyName = setPlatformFriendlyName,
                Changelist = setChangelist != null ? setChangelist.Value : 0,
                SessionName = setSessionName,
                IsLive = true
            };
            await SessionList.InsertOneAsync(newSession);
            return newSession.SessionName;
        }

        public async Task<Session> GetSessionByName(string sessionName)
        {
            var values = await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() > 0)
            {
                return values.Find(x => x.SessionName == sessionName);
            }
            return null;
        }

        public async Task<SessionFile> GetSessionHeader(string sessionName)
        {
            var values = await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() > 0)
            {
                return values.Find(x => x.SessionName == sessionName).HeaderFile;
            }
            return null;
        }

        public async Task<SessionFile> GetSessionChunk(string sessionName, int chunkIndex)
        {
            var values = await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() > 0)
            {
                var session = values.Find(x => x.SessionName == sessionName);
                if (chunkIndex >= 0 && chunkIndex < session.SessionFiles.Count)
                {
                    return session.SessionFiles[chunkIndex];
                }
            }
            return null;
        }

        public async Task<bool> SetUsers(string sessionName, string[] users)
        {
            var values =  await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() == 0)
            {
                LogError($"Session {sessionName} not found");
                return false;
            }

            var update = Builders<Session>.Update.Set(x => x.Users, users);
            await SessionList.UpdateOneAsync(x => x.SessionName == sessionName, update);

            return true;
        }

        public async Task<bool> SetHeader(string sessionName, SessionFile sessionFile, int streamChunkIndex, int totalDemoTimeMs)
        {
            var values = await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() == 0)
            {
                LogError($"Session {sessionName} not found");
                return false;
            }

            var update = Builders<Session>.Update.Set(x => x.HeaderFile, sessionFile)
                                                 .Set(x => x.TotalDemoTimeMs, totalDemoTimeMs);
            await SessionList.UpdateOneAsync(x => x.SessionName == sessionName, update);

            Log($"[HEADER] Stats for {sessionName}: TotalDemoTimeMs={totalDemoTimeMs}");

            return true;
        }

        public async Task<bool> AddChunk(string sessionName, SessionFile sessionFile, int totalDemoTimeMs, int totalChunks, int totalBytes)
        {
            var values = await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() == 0)
            {
                LogError($"Session {sessionName} not found");
                return false;
            }

            var update = Builders<Session>.Update.Set(x => x.TotalDemoTimeMs, totalDemoTimeMs)
                                                 .Set(x => x.TotalChunks, totalChunks)
                                                 .Set(x => x.TotalUploadedBytes, totalBytes)
                                                 .Push(x => x.SessionFiles, sessionFile);
            await SessionList.UpdateOneAsync(x => x.SessionName == sessionName, update);

            Log($"[CHUNK] Stats for {sessionName}: TotalDemoTimeMs={totalDemoTimeMs}, TotalChunks={totalChunks}, " +
                $"TotalUploadedBytes={totalBytes}");

            return true;
        }

        public async Task StopSession(string sessionName, int totalDemoTimeMs, int totalChunks, int totalBytes)
        {
            var values = await SessionList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count() == 0)
            {
                LogError($"Session {sessionName} not found");
                return;
            }

            var update = Builders<Session>.Update.Set(x => x.IsLive, false)
                                                 .Set(x => x.TotalDemoTimeMs, totalDemoTimeMs)
                                                 .Set(x => x.TotalChunks, totalChunks)
                                                 .Set(x => x.TotalUploadedBytes, totalBytes);
            await SessionList.UpdateOneAsync(x => x.SessionName == sessionName, update);

            await RemoveEndedLiveSessions();

            Log($"[END] Stats for {sessionName}: TotalDemoTimeMs={totalDemoTimeMs}, TotalChunks={totalChunks}, " +
                $"TotalUploadedBytes={totalBytes}");
        }

        public async Task<Session[]> FindReplaysByGroup(string group, IEventDatabase eventDatabase)
        {
            return await Task.Run(async () =>
            {
                var sessionNames = await eventDatabase.FindSessionNamesByGroup(group);
                if (sessionNames == null || sessionNames.Length == 0)
                    return Array.Empty<Session>();

                List<Session> sessions = new List<Session>();

                for (int i = 0; i < sessionNames.Length; i++)
                {
                    var values = await SessionList.Find(x => x.SessionName == sessionNames[i]).ToListAsync();
                    sessions.AddRange(values.FindAll(x => x.SessionName == sessionNames[i]));
                }

                return sessions.ToArray();
            });
        }

        public async Task<Session[]> FindReplays(string app, int? cl, string version, string meta, string user, bool? recent)
        {
            FilterDefinition<Session> pQuery = Builders<Session>.Filter.Empty;
            if (app != null)
                pQuery &= Builders<Session>.Filter.Eq(x => x.AppVersion, app);
            if (cl != null)
                pQuery &= Builders<Session>.Filter.Eq(x => x.Changelist, cl);
            if (version != null)
                pQuery &= Builders<Session>.Filter.Eq(x => x.NetVersion, version);
            if (user != null)
                pQuery &= Builders<Session>.Filter.AnyEq(x => x.Users, user);

            return await Task.Run(async () =>
            {
                var result = await SessionList.Find(pQuery).ToListAsync();

                return result.ToArray();
            });
        }

        public async Task CheckViewerInactivity()
        {
            await Task.Run(async () =>
            {
                var filter = Builders<Session>.Filter.Where(x => true);
                var update = Builders<Session>.Update.PullFilter(x => x.Viewers, v => v.LastSeen <= DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30));
                await SessionList.UpdateManyAsync(filter, update);
            });
        }

        public async Task CheckSessionInactivity()
        {
            await Task.Run(async () =>
            {
                long controlTime = DateTimeOffset.UtcNow.AddSeconds(TimeoutOfLiveSession).ToUnixTimeMilliseconds();
                var filter = Builders<Session>.Filter.Eq(x => x.IsLive, true);

                var result = await SessionList.Find(filter).ToListAsync();

                List<string> sessionNames = new List<string>();
                for (int i = 0; i < result.Count(); i++)
                {
                    long elapsedTime = result[i].CreationDate.ToUnixTimeMilliseconds() + result[i].TotalDemoTimeMs;
                    if (elapsedTime < controlTime)
                    {
                        sessionNames.Add(result[i].SessionName);
                    }
                }

                var filter1 = Builders<Session>.Filter.In(x => x.SessionName, sessionNames.ToArray());
                var update = Builders<Session>.Update.Set(x => x.IsLive, false);

                await SessionList.UpdateManyAsync(filter1, update);

                await RemoveEndedLiveSessions();
            });
        }

        public async Task RemoveEndedLiveSessions()
        {
            if (_applicationSettings.bLiveStreamMode)
            {
                await Task.Run(async () =>
                {
                    var eventList = database.GetCollection<EventEntry>("EventList");

                    long controlTime = DateTimeOffset.UtcNow.AddSeconds(TimeoutOfLiveSession).ToUnixTimeMilliseconds();
                    var filter = Builders<Session>.Filter.Eq(x => x.IsLive, false);

                    var result = await SessionList.Find(filter).ToListAsync();

                    var pQuery = Builders<EventEntry>.Filter.Empty;
                    for (int i = result.Count() - 1; i >= 0; i--)
                    {
                        long elapsedTime = result[i].CreationDate.ToUnixTimeMilliseconds() + result[i].TotalDemoTimeMs;
                        if (elapsedTime < controlTime)
                        {
                            pQuery |= Builders<EventEntry>.Filter.Eq(x => x.SessionName, result[i].SessionName);
                        }
                        else
                        {
                            result.RemoveAt(i);
                        }
                    }

                    await eventList.DeleteManyAsync(pQuery);
                    await SessionList.DeleteManyAsync(filter);
                });
            }
        }

        public async Task DoWorkOnStartup()
        {
            await Task.Run(async () =>
            {
                for (;;)
                {
                    await Task.Delay(_applicationSettings.HeartbeatCheckTime);
                    await CheckViewerInactivity();
                    await CheckSessionInactivity();
                    await RemoveEndedLiveSessions();
                }
            });
        }

        private void Log(string line)
        {
            // Empty
        }

        private void LogError(string line)
        {
            // Empty
        }
    }
}
