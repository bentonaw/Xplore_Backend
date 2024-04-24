﻿using CC_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace CC_Backend.Data
{
    public class NatureAIContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<Friends> Friends  { get; set; }
        public DbSet<Geodata> GeoData { get; set; }
        public DbSet<Stamp> Stamps { get; set; }
        public DbSet<StampCollected> StampsCollected { get; set; }

        public NatureAIContext(DbContextOptions<NatureAIContext> options): base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Friends>().HasNoKey();
            // Other configurations...
        }

    }
}