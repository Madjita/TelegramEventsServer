using DataBase.Entities;
using DataBase.Entities.Entities_DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBase.Entities.QrCodeEntities;

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
        public DbSet<TelegramBotTypes> TelegramBotTypes { get; }
        
        public int SaveChanges();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        
        
        public DbSet<WeatherSubscribers> WeatherSubscribers { get; }
        public DbSet<WeatherCity> WeatherCity { get; }
        public DbSet<CheckData> CheckData { get; }
        public DbSet<CheckParsedItems> CheckParsedItems { get; }
        public DbSet<CheckCompany> CheckCompany { get; }

    }

    public partial class DBContext : DbContext, IDBContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            //Database.Migrate();
            
            // Включаем ленивую загрузку
            ChangeTracker.LazyLoadingEnabled = true;
        }

        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<XEventUser> XEventUser { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<TelegramBotMessage> TelegramBotMessage { get; set; }
        public DbSet<Org> Org { get; set; }
        public DbSet<TelegramBots> TelegramBots { get; set; }
        public DbSet<XOrgUser> XOrgUser { get; set; }
        public DbSet<TelegramBotInChats> TelegramBotInChats { get; set; }
        public DbSet<OpenWeatherMap> OpenWeatherMap { get; set; }
        public DbSet<TelegramBotTypes> TelegramBotTypes { get; set; }
        
        public DbSet<WeatherSubscribers> WeatherSubscribers { get; set; }
        public DbSet<WeatherCity> WeatherCity { get; set; }
        public DbSet<CheckData> CheckData { get; set; }
        public DbSet<CheckParsedItems> CheckParsedItems { get; set; }
        public DbSet<CheckCompany> CheckCompany { get; set; }
        

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
            builder.Entity<TelegramBots>().HasOne(p => p.TelegramBotType).WithMany(p=> p.TelegramBots);
            builder.Entity<XOrgUser>();
            builder.Entity<TelegramBotInChats>();
            builder.Entity<OpenWeatherMap>();
            builder.Entity<TelegramBotTypes>();
            
            builder.Entity<WeatherSubscribers>();
            builder.Entity<WeatherCity>();
            
            builder.Entity<CheckData>();
            builder.Entity<CheckCompany>();
            builder.Entity<CheckParsedItems>()
                .HasOne(p => p.CheckData)
                .WithMany(p => p.CheckParsedItems)
                .HasForeignKey(p => p.CheckDataId);
            
            builder.Entity<CheckParsedItems>()
                .HasOne(p => p.CheckCompany)
                .WithMany(p => p.CheckParsedItems)
                .HasForeignKey(p => p.CheckCompanyId);

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
