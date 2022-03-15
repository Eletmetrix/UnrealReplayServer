/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using UnrealReplayServer.Databases.Models;
using UnrealReplayServer.Connectors;
using Microsoft.Extensions.Options;

namespace UnrealReplayServer.Databases
{
    public class EventDatabase : IEventDatabase
    {
        private readonly ConnectionStrings _connectionStrings;

        public EventDatabase(IOptions<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.Value;
        }

        public async Task AddEvent(string setSessionName, string group, int? time1, int? time2, string meta, bool? incrementSize, byte[] data)
        {
            MongoClient client = new MongoClient(_connectionStrings.MongoDBConnection);
            var database = client.GetDatabase("replayserver");
            string eventName = Guid.NewGuid().ToString("N");

            var eventList = database.GetCollection<EventEntry>("EventList");
            var newEntry = new EventEntry
            {
                GroupName = group,
                Meta = meta,
                SessionName = setSessionName,
                Time1 = time1.Value,
                Time2 = time2.Value,
                Data = data,
                EventId = eventName
            };
            await eventList.InsertOneAsync(newEntry);

            Log("[EVENT ADD] Adding event: " + eventName);
        }

        public async Task UpdateEvent(string setSessionName, string eventName, string group, int? time1, int? time2, string meta, bool? incrementSize, byte[] data)
        {
            MongoClient client = new MongoClient(_connectionStrings.MongoDBConnection);
            var database = client.GetDatabase("replayserver");
            var eventList = database.GetCollection<EventEntry>("EventList");
            var values = await eventList.Find(x => x.EventId == eventName).ToListAsync();
            if (values.Count() == 0) return;

            var update = Builders<EventEntry>.Update.Set("GroupName", group)
                                                    .Set("Meta", meta)
                                                    .Set("SessionName", setSessionName)
                                                    .Set("Time1", time1.Value)
                                                    .Set("Time2", time2.Value)
                                                    .Set("Data", data);
            await eventList.UpdateOneAsync(r => r.EventId == eventName, update);

            Log("[EVENT UPDATE] Updating event: " + eventName);
        }

        public async Task<EventEntry[]> GetEventsByGroup(string sessionName, string groupName)
        {
            MongoClient client = new MongoClient(_connectionStrings.MongoDBConnection);
            var database = client.GetDatabase("replayserver");
            var eventList = database.GetCollection<EventEntry>("EventList");
            var values = await eventList.Find(x => x.SessionName == sessionName).ToListAsync();
            if (values.Count == 0) return Array.Empty<EventEntry>();

            return await Task.Run(() =>
            {
                var entries = (from ee in values where ee.GroupName == groupName select ee).ToArray();
                return entries;
            });
        }

        public async Task<EventEntry> FindEventByName(string eventName)
        {
            MongoClient client = new MongoClient(_connectionStrings.MongoDBConnection);
            var database = client.GetDatabase("replayserver");
            var eventList = database.GetCollection<EventEntry>("EventList");
            var values = await eventList.Find(x => x.EventId == eventName).ToListAsync();
            if (values.Count() == 0) return null;

            return values.First();
        }

        public async Task<string[]> FindSessionNamesByGroup(string group)
        {
            return await Task.Run(async () =>
            {
                List<string> result = new List<string>();

                MongoClient client = new MongoClient(_connectionStrings.MongoDBConnection);
                var database = client.GetDatabase("replayserver");
                var eventList = database.GetCollection<EventEntry>("EventList");
                var values = await eventList.Find(x => true).ToListAsync();

                foreach (var pair in values)
                {
                    if (pair.GroupName == group)
                    {
                        if (result.Contains(pair.SessionName) == false)
                        {
                            result.Add(pair.SessionName);
                        }
                    }
                }

                return result.ToArray();
            });
        }

        private void Log(string line)
        {
            // Empty
        }
    }
}
