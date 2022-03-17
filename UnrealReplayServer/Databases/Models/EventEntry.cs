/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UnrealReplayServer.Databases.Models
{
    [BsonIgnoreExtraElements]
    public class EventEntry
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("SessionName")]
        public string SessionName { get; set; }

        [BsonElement("GroupName")]
        public string GroupName { get; set; }

        [BsonElement("EventId")]
        public string EventId { get; set; }

        [BsonElement("Time1")]
        public int Time1 { get; set; }

        [BsonElement("Time2")]
        public int Time2 { get; set; }

        [BsonElement("Meta")]
        public string Meta { get; set; }

        [BsonElement("Data")]
        public byte[] Data { get; set; }
    }

}
