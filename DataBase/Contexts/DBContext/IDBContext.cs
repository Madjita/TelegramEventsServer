using DataBase.Entities;
using DataBase.Entities.Entities_DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Contexts.DBContext
{
    public interface IDBContext
    {
        public DbSet<User> User { get; }
        public DbSet<Role> Role { get; }
        public DbSet<XEventUser> XEventUser { get; }
        public DbSet<Event> Event { get; }
        public DbSet<TelegramBotMessage> TelegramBotMessage { get; }
        public DbSet<Org> Org { get; }
        public DbSet<TelegramBots> TelegramBots { get; }
        public DbSet<XOrgUser> XOrgUser { get; }
        public DbSet<TelegramBotInChats> TelegramBotInChats { get; }
        public DbSet<OpenWeatherMap> OpenWeatherMap { get; }
        public DbSet<WeatherSubscribers> WeatherSubscribers { get; }
        public DbSet<WeatherCity> WeatherCity { get; }
        public int SaveChanges();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public partial class DBContext : DbContext, IDBContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            //Database.Migrate();
        }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<XEventUser> XEventUser { get; set; }
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<TelegramBotMessage> TelegramBotMessage { get; set; }
        public virtual DbSet<Org> Org { get; set; }
        public virtual DbSet<TelegramBots> TelegramBots { get; set; }
        public virtual DbSet<XOrgUser> XOrgUser { get; set; }
        public virtual DbSet<TelegramBotInChats> TelegramBotInChats { get; set; }
        public virtual DbSet<OpenWeatherMap> OpenWeatherMap { get; set; }
        public virtual DbSet<WeatherSubscribers> WeatherSubscribers { get; set; }
        
        public virtual DbSet<WeatherCity> WeatherCity { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            /*
             * SQLite doesn't natively support the following data types: DateTimeOffset, Decimal, TimeSpan, UInt64
             */
            // configurationBuilder
            //     .Properties<DateTimeOffset>()
            //     .HaveConversion<DateTimeOffsetConverter>();

            // configurationBuilder
            //     .Properties<DateTime>()
            //     .HaveConversion<DateTimeConverter>();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>();
            builder.Entity<Role>();
            builder.Entity<XEventUser>();
            builder.Entity<Event>();
            builder.Entity<TelegramBotMessage>();
            builder.Entity<Org>();
            builder.Entity<TelegramBots>();
            builder.Entity<XOrgUser>();
            builder.Entity<TelegramBotInChats>();
            builder.Entity<OpenWeatherMap>();
            builder.Entity<WeatherSubscribers>();
            builder.Entity<WeatherCity>();
        }

        #region Helpers

        /// <summary>
        ///     Used to automatically update CreatedAt, UpdatedAt fields
        /// </summary>
        private void UpdateTimestamps()
        {
            try
            {
                var entityEntries = ChangeTracker.Entries();

                foreach (var entityEntry in entityEntries.Where(_ => _.State is EntityState.Modified or EntityState.Added))
                {
                    var dateTimeOffsetUtcNow = DateTimeOffset.UtcNow;

                    //if (entityEntry.State == EntityState.Added)
                    //    ((EntityBase)entityEntry.Entity).CreatedAt = dateTimeOffsetUtcNow;
                    //((EntityBase)entityEntry.Entity).UpdatedAt = dateTimeOffsetUtcNow;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Overrides

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        #endregion
    }
}
