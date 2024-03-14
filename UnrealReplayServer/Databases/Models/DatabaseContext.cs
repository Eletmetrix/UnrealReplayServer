/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.EntityFrameworkCore;
using UnrealReplayServer.Databases.Models;

public class DatabaseContext : DbContext
{
    public DbSet<Session> sessionList;
    public DbSet<EventEntry> eventList;

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
}