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
    public DbSet<ApplicationSettings> applicationSettings { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>().HasKey(x => x.Id);
        modelBuilder.Entity<EventEntry>().HasKey(x => x.Id);
        modelBuilder.Entity<AuthorizationHeader>().HasKey(x => x.Id);
        modelBuilder.Entity<ApplicationSettings>().HasKey(x => x.Id);

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