/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnrealReplayServer.Databases.Models
{
    public class SessionViewer
    {
        [Required]
        [Key]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;

        public virtual Session Session { get; set; }
    }

    public class Session
    {
        [Required]
        [Key]
        public int Id { get; set; }

        public bool IsLive { get; set; } = false;

        public string SessionName { get; set; } = string.Empty;

        public string AppVersion { get; set; } = string.Empty;

        public string NetVersion { get; set; } = string.Empty;

        public int Changelist { get; set; } = 0;

        public string Meta { get; set; } = string.Empty;

        public string PlatformFriendlyName { get; set; } = string.Empty;

        public int TotalDemoTimeMs { get; set; } = 0;

        public int TotalUploadedBytes { get; set; } = 0;

        public int TotalChunks { get; set; } = 0;

        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;

        public string[] Users { get; set; } = Array.Empty<string>();

        public virtual ICollection<SessionViewer> Viewers { get; set; } = new List<SessionViewer>();

        public SessionFile HeaderFile { get; set; }

        public virtual ICollection<SessionFile> SessionFiles { get; set; } = new List<SessionFile>();
    }
}
