/*
The MIT License (MIT)
Copyright (c) 2021 Henning Thoele
*/

using Microsoft.EntityFrameworkCore;
using UnrealReplayServer.Databases.Models;

public class DatabaseContext : DbContext
{
    public DbSet<Session> sessionList { get; set; }
    public DbSet<EventEntry> eventList { get; set; }
    public DbSet<AuthorizationHeader> authorizationHeaders { get; set; }
    public DbSet<SessionFile> sessionFiles { get; set; }
    public DbSet<SessionViewer> sessionViewer { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
        sessionList = Set<Session>();
        eventList = Set<EventEntry>();
        authorizationHeaders = Set<AuthorizationHeader>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SessionFile>(x =>
        {
            x.HasKey(y => y.Id);
            x.HasOne(y => y.Session)
                .WithMany(s => s.SessionFiles)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionViewer>(x =>
        {
            x.HasKey(y => y.Id);
            x.HasOne(y => y.Session)
                .WithMany(s => s.Viewers)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}