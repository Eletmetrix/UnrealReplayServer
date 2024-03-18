/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

namespace UnrealReplayServer.Models
{
    public class StartDownloadingResponse
    {
        public string State { get; set; } = string.Empty;

        public int NumChunks { get; set; } = 0;

        public int Time { get; set; } = 0;

        public string ViewerId { get; set; } = string.Empty;
    }
}
