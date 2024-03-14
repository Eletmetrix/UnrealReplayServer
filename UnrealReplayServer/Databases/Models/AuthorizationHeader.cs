/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System;

namespace UnrealReplayServer.Databases.Models
{
    public class AuthorizationHeader
    {
        public int Id { get; set; }

        public string AuthorizationHeaderValue { get; set; }

        public int RemainingUse { get; set; } = -1;

        public int ExpirationDateAsMinute { get; set; } = 0;

        public bool bUseRemainingUse { get; set; } = false;

        public bool bExpirationDate { get; set; } = false;

        public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;
    }
}
