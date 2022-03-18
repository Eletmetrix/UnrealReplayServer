/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

namespace UnrealReplayServer.Connectors
{
    public class ApplicationDefaults
    {
        public string MongoDBConnection { get; set; }

        public string MongoDBDatabaseName { get; set; }

        public bool bLiveStreamMode { get; set; }

        public int HeartbeatCheckTime { get; set; }

        public int TimeoutOfLiveSession { get; set; }

        public bool bUseAuthorizationHeader { get; set; }

        public string AuthorizationHeaderValue { get; set; }
    }
}