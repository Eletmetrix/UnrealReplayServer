/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System.ComponentModel.DataAnnotations;

namespace UnrealReplayServer.Databases.Models
{
    public class ApplicationSettings
    {
        [Required]
        [Key]
        public int Id { get; set; } = 1;


        // Remove all live sessions after they are ended.
        public bool bLiveStreamMode { get; set; }

        // Heartbeat control loop delay time.
        public int HeartbeatCheckTime { get; set; } = 30000;

        // Timeout value (in negative seconds) for inactive live sessions.
        public int TimeoutOfLiveSession { get; set; } = 30;


        // You need to override engine codes (FHttpNetworkReplayStreamer) to add authorization header.
        // enables authorization header check.
        public bool bUseAuthorizationHeader { get; set; } = false;

        // You need to override engine codes (FHttpNetworkReplayStreamer) to add authorization header.
        // @TODO: Authorization keys (permanent/temp).
        public string AuthorizationHeaderValue { get; set; } = "bearer DummyAuthTicket";


        // User-Agent filter
        // Possibly User-Agent format:        <product>/<product-version> <comment>
        // Example:                           UnrealTest/++UE4+Release-4.20-CL-4369336 Windows/10.0.22000.1.256.64bit
        // See full details on this webpage:  https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/User-Agent
        public string[] AllowedUserAgents { get; set; } = {
            "UnrealTest/++UE4+Release-5.3.1-CL-4369336",
            "UnrealTest/++UE4+Release-5.3.1",
            "UnrealTest/++UE4+Release-CL-1256"
            //"Mozilla/5.0"
        };

        // Enables User-Agent filter.
        public bool bUseUserAgentFilter { get; set; } = false;
    }
}
