/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnrealReplayServer.Databases.Models;
using UnrealReplayServer.Connectors;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace UnrealReplayServer.Databases
{
    public class EventDatabase : DbContext, IEventDatabase
    {
        private readonly ApplicationDefaults _applicationSettings;
        private readonly DatabaseContext _context;

        public EventDatabase(DatabaseContext context, IOptions<ApplicationDefaults> connectionStrings)
        {
            _context = context;
            _applicationSettings = connectionStrings.Value;
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
            _context.eventList.Add(newEntry);
            await _context.SaveChangesAsync();

            Log("[EVENT ADD] Adding event: " + eventName);
        }

        public async Task UpdateEvent(string setSessionName, string eventName, string group, int? time1, int? time2, string meta, bool? incrementSize, byte[] data)
        {
            var updateEvent = await _context.eventList.SingleOrDefaultAsync(x => x.EventId == eventName);
            if (updateEvent != null)
            {
                updateEvent.GroupName = group;
                updateEvent.Meta = meta;
                updateEvent.SessionName = setSessionName;
                updateEvent.Time1 = time1.Value;
                updateEvent.Time2 = time2.Value;
                updateEvent.Data = data;
                await _context.SaveChangesAsync();

                Log("[EVENT UPDATE] Updating event: " + eventName);
            }
        }

        public async Task<EventEntry[]> GetEventsByGroup(string sessionName, string groupName)
        {
            return await Task.Run(async () =>
            {
                return await _context.eventList
                    .Where(x => x.GroupName == groupName)
                    .ToArrayAsync();
            });
        }

        public async Task<EventEntry> FindEventByName(string eventName)
        {
            return await _context.eventList
                .Where(x => x.EventId == eventName)
                .FirstOrDefaultAsync();
        }

        public async Task<string[]> FindSessionNamesByGroup(string group)
        {
            return await Task.Run(async () =>
            {
                List<string> result = new List<string>();

                return await _context.eventList
                    .Where(x => x.GroupName == group)
                    .Select(x => x.SessionName)
                    .Distinct()
                    .ToArrayAsync();
            });
        }

        private void Log(string line)
        {
            // Empty
        }
    }
}