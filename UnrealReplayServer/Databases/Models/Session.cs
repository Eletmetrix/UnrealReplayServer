/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnrealReplayServer.Databases.Models
{
    public class SessionViewer
    {
        [BsonElement("Username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("LastSeen")]
        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;
    }

    [BsonIgnoreExtraElements]
    public class Session
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("IsLive")]
        public bool IsLive { get; set; } = false;

        [BsonElement("SessionName")]
        public string SessionName { get; set; } = string.Empty;

        [BsonElement("AppVersion")]
        public string AppVersion { get; set; } = string.Empty;

        [BsonElement("NetVersion")]
        public string NetVersion { get; set; } = string.Empty;

        [BsonElement("Changelist")]
        public int Changelist { get; set; } = 0;

        [BsonElement("Meta")]
        public string Meta { get; set; } = string.Empty;

        [BsonElement("PlatformFriendlyName")]
        public string PlatformFriendlyName { get; set; } = string.Empty;

        [BsonElement("TotalDemoTimeMs")]
        public int TotalDemoTimeMs { get; set; } = 0;

        [BsonElement("TotalUploadedBytes")]
        public int TotalUploadedBytes { get; set; } = 0;

        [BsonElement("TotalChunks")]
        public int TotalChunks { get; set; } = 0;

        [BsonElement("CreationDate")]
        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;

        [BsonElement("Users")]
        public string[] Users { get; set; } = Array.Empty<string>();

        [BsonElement("Viewers")]
        public List<SessionViewer> Viewers { get; set; } = new List<SessionViewer>();

        [BsonElement("HeaderFile")]
        public SessionFile HeaderFile { get; set; }

        [BsonElement("SessionFiles")]
        public List<SessionFile> SessionFiles = new List<SessionFile>();

        internal void CheckViewersTimeout()
        {
            CheckViewersTimeout(TimeSpan.FromSeconds(30), DateTimeOffset.UtcNow);
        }

        // TODO: CheckViewersTimeout based on MongoDB values.
        internal void CheckViewersTimeout(TimeSpan maxDeltaTime, DateTimeOffset referenceTime)
        {
            foreach (var item in Viewers)
            {
                if (item.LastSeen < referenceTime - maxDeltaTime)
                {
                    string name = item.Username;
                    Viewers.Remove(item);
                    Log($"Removed viewer {name} due to inaactivity");
                }
            }
        }

        private void Log(string ling)
        {
            // Empty
        }
    }
}
