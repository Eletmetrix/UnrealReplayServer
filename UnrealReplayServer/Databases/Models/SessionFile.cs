/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System.ComponentModel.DataAnnotations;

namespace UnrealReplayServer.Databases.Models
{
    public class SessionFile
    {
        [Required]
        [Key]
        public int Id { get; set; }

        public string Filename { get; set; }

        public byte[] Data { get; set; }

        public int StartTimeMs { get; set; }

        public int EndTimeMs { get; set; }

        public int ChunkIndex { get; set; } = 0;

        public virtual Session Session { get; set; }
    }
}
