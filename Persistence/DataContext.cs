using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Persistence.Relational;
using Windows.UI;
using Windows.UI.Xaml.Markup;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Markup;

namespace Persistence
{
    internal class DataContext(IConfiguration configuration) : DbContext
    {
        public DbSet<Comic> Comics { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var ensure = false;
            if (configuration["DataSource"] is string dataSource)
                optionsBuilder.UseSqlite($"Data Source='{dataSource}'");
            else
            {
                optionsBuilder.UseSqlite($"Data Source='{AppContext.BaseDirectory}\\DataBase.db'");
                ensure = true;
            }
            
            base.OnConfiguring(optionsBuilder);
            if (ensure) EnsureExists();
        }

        private void EnsureExists()
        {
            Database.ExecuteSqlRaw(
            """
            PRAGMA foreign_keys = off;
            BEGIN TRANSACTION;

            -- Table: Comics
            CREATE TABLE IF NOT EXISTS Comics (Id BLOB (16) PRIMARY KEY UNIQUE NOT NULL, Name TEXT NOT NULL, Info TEXT, Status INTEGER (2) CHECK (STATUS IN (0, 1, 2)) NOT NULL, Cover BLOB (16));

            -- Table: ComicTags
            CREATE TABLE IF NOT EXISTS ComicTags (Id BLOB (16) PRIMARY KEY UNIQUE NOT NULL, ComicId BLOB (16) REFERENCES Comics (Id) ON DELETE CASCADE NOT NULL, TagId BLOB (16) NOT NULL REFERENCES Tags (Id) ON DELETE CASCADE);

            -- Table: Tags
            CREATE TABLE IF NOT EXISTS Tags (Id BLOB (16) PRIMARY KEY UNIQUE NOT NULL, Name TEXT UNIQUE NOT NULL, Color1 INTEGER (4) NOT NULL, Color2 INTEGER (4) NOT NULL, ColorText INTEGER (4) NOT NULL);

            COMMIT TRANSACTION;
            PRAGMA foreign_keys = on;
            
            """);
        }
        private Expression<Func<Guid, byte[]>> GuidToBytes = static guid => guid.ToByteArray();
        private Expression<Func<byte[], Guid>> BytesToGuid = static bytes => MemoryMarshal.Read<Guid>(bytes);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comic>(model =>
            {
                model.ToTable("Comics").HasKey(c => c.Id);

                model.Property(c => c.Id)
                    .HasColumnName("Id")
                    .HasConversion(GuidToBytes, BytesToGuid);

                model.Property(c => c.Name)
                    .HasColumnName("Name");

                model.Property(c => c.Details)
                    .HasColumnName("Details");

                model.Property(c => c.Status)
                    .HasColumnName("Status");

                model.Property(c => c.Cover)
                    .HasColumnName("Cover");

                model.Property(c => c.Order)
                    .HasColumnName("Order");

                model
                    .HasMany(c => c.Tags)
                    .WithMany(t => t.Comics)
                    .UsingEntity<ComicTag>(
                        r => r.HasOne(ct => ct.Tag).WithMany().HasForeignKey(ct => ct.TagId),
                        l => l.HasOne(ct => ct.Comic).WithMany().HasForeignKey(ct => ct.ComicId),
                        model =>
                        {
                            model.ToTable("ComicTags").HasKey(ct => ct.Id);
                            model.Property(ct => ct.Id).HasColumnName("Id").HasConversion(GuidToBytes, BytesToGuid);
                            model.Property(ct => ct.ComicId).HasColumnName("ComicId").HasConversion(GuidToBytes, BytesToGuid);
                            model.Property(ct => ct.TagId).HasColumnName("TagId").HasConversion(GuidToBytes, BytesToGuid);
                        }
                    );
            });

            modelBuilder.Entity<Tag>(model =>
            {
                Expression<Func<Color, string>> HexToColor =
                static color => color.ToString();
                Func<string, Color> ColorToHex = hex =>
                {
                    var a = Convert.ToByte(hex.Substring(1, 2), 16);
                    var r = Convert.ToByte(hex.Substring(3, 2), 16);
                    var g = Convert.ToByte(hex.Substring(5, 2), 16);
                    var b = Convert.ToByte(hex.Substring(7, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                };

                model.ToTable("Tags").HasKey(t => t.Id);

                model.Property(t => t.Id)
                    .HasColumnName("Id")
                    .HasConversion(GuidToBytes, BytesToGuid);

                model.Property(t => t.Name)
                    .HasColumnName("Name");

                model.Property(t => t.Color1)
                    .HasColumnName("Color1")
                    .HasConversion(HexToColor, hex => ColorToHex(hex));

                model.Property(t => t.Color2)
                    .HasColumnName("Color2")
                    .HasConversion(HexToColor, hex => ColorToHex(hex));

                model.Property(t => t.TextColor)
                    .HasColumnName("TextColor")
                    .HasConversion(HexToColor, hex => ColorToHex(hex));

            });
        }
    }
}
