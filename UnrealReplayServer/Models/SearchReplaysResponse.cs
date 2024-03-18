/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System;

namespace UnrealReplayServer.Models
{
    public class SearchReplaysResponse
    {
        public class SearchReplaysResponseEntry
        {
            public string App { get; set; }

            public string SessionName { get; set; }

            public string FriendlyName { get; set; }

            public DateTime Timestamp { get; set; }

            public int SizeInBytes { get; set; }

            public int DemoTimeInMs { get; set; }

            public int NumViewers { get; set; }

            public bool bIsLive { get; set; }

            public int Changelist { get; set; }

            public bool shouldKeep { get; set; }
        }

        public SearchReplaysResponseEntry[] Replays { get; set; } = Array.Empty<SearchReplaysResponseEntry>();
    }
}
