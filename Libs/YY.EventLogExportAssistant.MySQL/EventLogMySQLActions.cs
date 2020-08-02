﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using YY.EventLogExportAssistant.Database;

namespace YY.EventLogExportAssistant.MySQL
{
    public sealed class EventLogMySQLActions : IEventLogContextExtensionActions
    {
        #region Public Methods

        public void AdditionalInitializationActions(DatabaseFacade database)
        {
            database.ExecuteSqlRaw(Resources.Query_CreateView_vw_EventLog);
        }
        public void OnModelCreating(ModelBuilder modelBuilder, out bool standardBehaviorChanged)
        {
            standardBehaviorChanged = false;
        }
        public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration Configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                string connectinString = Configuration.GetConnectionString("EventLogDatabase");
                optionsBuilder.UseMySql(connectinString);
            }
        }
        public bool UseExplicitKeyIndicesInitialization()
        {
            return true;
        }

        #endregion
    }
}
