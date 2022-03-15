/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnrealReplayServer.Databases.Models
{
    public class SessionFile
    {
        [BsonElement("Filename")]
        public string Filename { get; set; }

        [BsonElement("Data")]
        public byte[] Data { get; set; }

        [BsonElement("StartTimeMs")]
        public int StartTimeMs { get; set; }

        [BsonElement("EndTimeMs")]
        public int EndTimeMs { get; set; }

        [BsonElement("ChunkIndex")]
        public int ChunkIndex { get; set; } = 0;
    }
}
