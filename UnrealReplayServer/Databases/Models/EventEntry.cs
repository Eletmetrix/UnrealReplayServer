/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System.ComponentModel.DataAnnotations;

namespace UnrealReplayServer.Databases.Models
{
    public class EventEntry
    {
        [Required]
        [Key]
        public int Id { get; set; }

        public string SessionName { get; set; }

        public string GroupName { get; set; }

        public string EventId { get; set; }

        public int Time1 { get; set; }

        public int Time2 { get; set; }

        public string Meta { get; set; }

        public byte[] Data { get; set; }
    }

}
