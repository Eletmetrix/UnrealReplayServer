/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/


namespace UnrealReplayServer.Databases.Models
{
    public class SessionFile
    {
        public string Filename { get; set; }

        public byte[] Data { get; set; }

        public int StartTimeMs { get; set; }

        public int EndTimeMs { get; set; }

        public int ChunkIndex { get; set; } = 0;
    }
}
