using System;
using System.IO;
using LobbyServerForLinux.Model.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using P2PCore.Models;

namespace RubyApi.Models
{
    public partial class P2PContext : DbContext
    {
        public P2PContext() : base() { }

        //建構Model
        public DbSet<user> user { get; set; }
        public DbSet<download> download { get; set; }
        public DbSet<download_record> download_record { get; set; }
        public DbSet<download_start> download_start { get; set; }
        public DbSet<ip_record> ip_record { get; set; }
        public DbSet<short_url> short_url { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            //加入連線
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseMySql(configuration.GetConnectionString("DefaultConnection"));
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                
            }
        }

    }
}
