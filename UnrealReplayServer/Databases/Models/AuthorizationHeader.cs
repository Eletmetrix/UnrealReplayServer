/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace UnrealReplayServer.Databases.Models
{
    [BsonIgnoreExtraElements]
    public class AuthorizationHeader
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("AuthorizationHeaderValue")]
        public string AuthorizationHeaderValue { get; set; }

        [BsonElement("RemainingUse")]
        public int RemainingUse { get; set; } = -1;

        [BsonElement("ExpirationDateAsMinute")]
        public int ExpirationDateAsMinute { get; set; } = 0;

        [BsonElement("bUseExpirationDate")]
        public bool bUseRemainingUse { get; set; } = false;

        [BsonElement("bUseExpirationDate")]
        public bool bExpirationDate { get; set; } = false;

        [BsonElement("CreationDate")]
        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;
    }
}
