/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayServer.Connectors;
using UnrealReplayServer.Databases.Models;

namespace UnrealReplayServer.Databases
{
    public class SessionDatabase : DbContext, ISessionDatabase
    {
        private readonly ApplicationDefaults _applicationSettings;
        private readonly DatabaseContext _context;
        private readonly int TimeoutOfLiveSession;

        public SessionDatabase(DatabaseContext context, IOptions<ApplicationDefaults> connectionString)
        {
            _context = context;
            _applicationSettings = connectionString.Value;

            TimeoutOfLiveSession = _applicationSettings.TimeoutOfLiveSession * -1;
        }

        public async Task<string> CreateSession(string setSessionName, string setAppVersion, string setNetVersion, int? setChangelist,
            string setPlatformFriendlyName)
        {
            var values = await _context.sessionList.Where(x => x.SessionName == setSessionName).ToListAsync();
            if (values.Any())
            {
                _context.sessionList.RemoveRange(values);
                await _context.SaveChangesAsync();
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
            _context.sessionList.Add(newSession);
            SaveChanges();
            return newSession.SessionName;
        }

        public async Task<Session> GetSessionByName(string sessionName)
        {
            return await _context.sessionList
                .Where(x => x.SessionName == sessionName)
                .FirstOrDefaultAsync();
        }

        public async Task<SessionFile> GetSessionHeader(string sessionName)
        {
            return await _context.sessionList
                .Where(x => x.SessionName == sessionName)
                .Select(x => x.HeaderFile)
                .FirstOrDefaultAsync();
        }

        public async Task<SessionFile> GetSessionChunk(string sessionName, int chunkIndex)
        {
            return await _context.sessionList
                .Where(x => x.SessionName == sessionName && chunkIndex >= 0 && chunkIndex < x.SessionFiles.Count)
                .Select(x => x.SessionFiles[chunkIndex])
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SetUsers(string sessionName, string[] users)
        {
            var session = await _context.sessionList.Where(x => x.SessionName == sessionName).FirstOrDefaultAsync();
            if (session != null)
            {
                session.Users = users;
                await _context.SaveChangesAsync();

                return true;
            }

            LogError($"Session {sessionName} not found");
            return false;
        }

        public async Task<bool> SetHeader(string sessionName, SessionFile sessionFile, int streamChunkIndex, int totalDemoTimeMs)
        {
            var session = await _context.sessionList.Where(x => x.SessionName == sessionName).FirstOrDefaultAsync();
            if (session != null)
            {
                session.HeaderFile = sessionFile;
                session.TotalDemoTimeMs = totalDemoTimeMs;

                Log($"[HEADER] Stats for {sessionName}: TotalDemoTimeMs={totalDemoTimeMs}");
                await _context.SaveChangesAsync();
            }

            LogError($"Session {sessionName} not found");
            return false;
        }

        public async Task<bool> AddChunk(string sessionName, SessionFile sessionFile, int totalDemoTimeMs, int totalChunks, int totalBytes)
        {
            var session = await _context.sessionList.FirstOrDefaultAsync(x => x.SessionName == sessionName);

            if (session != null)
            {
                session.TotalDemoTimeMs = totalDemoTimeMs;
                session.TotalChunks = totalChunks;
                session.TotalUploadedBytes = totalBytes;
                session.SessionFiles.Add(sessionFile);

                await _context.SaveChangesAsync();

                Log($"[CHUNK] Stats for {sessionName}: TotalDemoTimeMs={totalDemoTimeMs}, TotalChunks={totalChunks}, " +
                    $"TotalUploadedBytes={totalBytes}");

                return true;
            }

            LogError($"Session {sessionName} not found");
            return false;
        }

        public async Task StopSession(string sessionName, int totalDemoTimeMs, int totalChunks, int totalBytes)
        {
            var session = await _context.sessionList.FirstOrDefaultAsync(x => x.SessionName == sessionName);

            if (session != null)
            {
                session.IsLive = false;
                session.TotalDemoTimeMs = totalDemoTimeMs;
                session.TotalChunks = totalChunks;
                session.TotalUploadedBytes = totalBytes;

                await _context.SaveChangesAsync();
                await RemoveEndedLiveSessions();

                Log($"[END] Stats for {sessionName}: TotalDemoTimeMs={totalDemoTimeMs}, TotalChunks={totalChunks}, " +
                    $"TotalUploadedBytes={totalBytes}");
            }
        }

        public async Task<Session[]> FindReplaysByGroup(string group, IEventDatabase eventDatabase)
        {
            var sessionNames = await eventDatabase.FindSessionNamesByGroup(group);
            if (sessionNames != null && sessionNames.Length > 0)
            {
                return await _context.sessionList
                    .Where(x => sessionNames.Contains(x.SessionName))
                    .ToArrayAsync();
            }

            return Array.Empty<Session>();
        }

        public async Task<Session[]> FindReplays(string app, int? cl, string version, string meta, string user, bool? recent)
        {
            var query = _context.sessionList.AsQueryable();

            if (app != null)
                query = query.Where(x => x.AppVersion == app);
            if (cl != null)
                query = query.Where(x => x.Changelist == cl);
            if (version != null)
                query = query.Where(x => x.NetVersion == version);
            if (user != null)
                query = query.Where(x => x.Users.Contains(user));

            return await query.ToArrayAsync();
        }

        public async Task CheckViewerInactivity()
        {
            var sessions = await _context.sessionList.Include(s => s.Viewers).ToListAsync();
            foreach (var session in sessions)
            {
                session.Viewers.RemoveAll(v => v.LastSeen <= DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30));
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckSessionInactivity()
        {
            var controlTime = DateTimeOffset.UtcNow.AddSeconds(TimeoutOfLiveSession).ToUnixTimeMilliseconds();
            var inactiveSessions = await _context.sessionList
                .Where(x => x.IsLive && x.CreationDate.ToUnixTimeMilliseconds() + x.TotalDemoTimeMs < controlTime)
                .ToListAsync();

            foreach (var session in inactiveSessions)
            {
                session.IsLive = false;
            }

            await _context.SaveChangesAsync();
            await RemoveEndedLiveSessions();
        }

        public async Task RemoveEndedLiveSessions()
        {
            if (_applicationSettings.bLiveStreamMode)
            {
                var controlTime = DateTimeOffset.UtcNow.AddSeconds(TimeoutOfLiveSession);
                var sessionsToRemove = await _context.sessionList
                    .Where(x => !x.IsLive && x.CreationDate.AddMilliseconds(x.TotalDemoTimeMs) < controlTime)
                    .ToListAsync();
                var eventsToRemove = await _context.eventList
                    .Where(x => sessionsToRemove.Any(s => s.SessionName == x.SessionName))
                    .ToListAsync();

                _context.eventList.RemoveRange(eventsToRemove);
                _context.sessionList.RemoveRange(sessionsToRemove);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DoWorkOnStartup()
        {
            await Task.Run(async () =>
            {
                for (; ; )
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
