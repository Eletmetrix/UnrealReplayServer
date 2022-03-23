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
        private readonly ApplicationDefaults _applicationSettings;
        
        private MongoClient client;
        private IMongoDatabase database;
        private IMongoCollection<EventEntry> eventList;

        public EventDatabase(IOptions<ApplicationDefaults> connectionStrings)
        {
            _applicationSettings = connectionStrings.Value;

            string ConnectionString = _applicationSettings.MongoDB.bUseEnvVariable_Connection ? Environment.GetEnvironmentVariable("MONGO_CON_URL") : _applicationSettings.MongoDB.MongoDBConnection;
            string DatabaseName = _applicationSettings.MongoDB.bUseEnvVariable_DatabaseName ? Environment.GetEnvironmentVariable("MONGO_DB_NAME") : _applicationSettings.MongoDB.MongoDBDatabaseName;

            client = new MongoClient(ConnectionString);
            database = client.GetDatabase(DatabaseName);
            eventList = database.GetCollection<EventEntry>("EventList");
        }

        public async Task AddEvent(string setSessionName, string group, int? time1, int? time2, string meta, bool? incrementSize, byte[] data)
        {
            string eventName = Guid.NewGuid().ToString("N");
           
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
            var filter = Builders<EventEntry>.Filter.Eq(x => x.EventId, eventName);
            var update = Builders<EventEntry>.Update.Set("GroupName", group)
                                                    .Set("Meta", meta)
                                                    .Set("SessionName", setSessionName)
                                                    .Set("Time1", time1.Value)
                                                    .Set("Time2", time2.Value)
                                                    .Set("Data", data);
            await eventList.UpdateOneAsync(filter, update);

            Log("[EVENT UPDATE] Updating event: " + eventName);
        }

        public async Task<EventEntry[]> GetEventsByGroup(string sessionName, string groupName)
        {
            return await Task.Run(async () =>
            {
                var filter = Builders<EventEntry>.Filter.And(Builders<EventEntry>.Filter.Eq(x => x.GroupName, groupName),
                    Builders<EventEntry>.Filter.Eq(x => x.SessionName, sessionName));

                var entries = await eventList.Find(filter).ToListAsync();

                return entries.ToArray();
            });
        }

        public async Task<EventEntry> FindEventByName(string eventName)
        {
            var values = await eventList.Find(x => x.EventId == eventName).ToListAsync();
            if (values.Count() == 0) return null;

            return values.First();
        }

        public async Task<string[]> FindSessionNamesByGroup(string group)
        {
            return await Task.Run(async () =>
            {
                List<string> result = new List<string>();

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
