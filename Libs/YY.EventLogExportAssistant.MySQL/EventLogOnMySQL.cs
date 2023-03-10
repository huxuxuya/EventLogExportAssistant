using System.Collections.Generic;
using System.IO;
using YY.EventLogReaderAssistant;
using RowData = YY.EventLogReaderAssistant.Models.RowData;
using Microsoft.EntityFrameworkCore;
using System;
using YY.EventLogExportAssistant.Database;

namespace YY.EventLogExportAssistant.MySQL
{
    public class EventLogOnMySQL : EventLogOnTarget
    {
        #region Private Member Variables

        private readonly DbContextOptions<EventLogContext> _databaseOptions;
        private readonly IEventLogContextExtensionActions _databaseActions;
        private ReferencesDataCache _referencesCache;

        #endregion

        #region Constructor

        public EventLogOnMySQL() : this(null, _defaultPortion)
        {
            
        }
        public EventLogOnMySQL(int portion) : this(null, portion)
        {
            _portion = portion;
        }
        public EventLogOnMySQL(DbContextOptions<EventLogContext> databaseOptions, int portion)
        {
            _databaseActions = new EventLogMySQLActions();
            _maxPeriodRowData = DateTime.MinValue;
            _portion = portion;
            if (databaseOptions == null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<EventLogContext>();
                _databaseActions.OnConfiguring(optionsBuilder);
                _databaseOptions = optionsBuilder.Options;
            }
            else
                _databaseOptions = databaseOptions;
        }

        #endregion

        #region Public Methods

        public override EventLogPosition GetLastPosition()
        {
            if (_lastEventLogFilePosition != null)
                return _lastEventLogFilePosition;

            EventLogPosition position;
            using (EventLogContext _context = EventLogContext.Create(_databaseOptions, _databaseActions))
                position = _context.GetLastPosition(_system);
            _lastEventLogFilePosition = position;

            return position;
        }
        public override void SaveLogPosition(FileInfo logFileInfo, EventLogPosition position)
        {
            using (EventLogContext _context = EventLogContext.Create(_databaseOptions, _databaseActions))
                _context.SaveLogPosition(_system, logFileInfo, position);

            _lastEventLogFilePosition = position;
        }
        public override int GetPortionSize()
        {
            return _portion;
        }
        public override void Save(RowData rowData)
        {
            Save(new List<RowData>
            {
                rowData
            });
        }
        public override void Save(IList<RowData> rowsData)
        {
            using (EventLogContext _context = EventLogContext.Create(_databaseOptions, _databaseActions))
            {
                if (_maxPeriodRowData == DateTime.MinValue)
                    _maxPeriodRowData = _context.GetRowsDataMaxPeriod(_system);

                List<Database.Models.RowData> newEntities = new List<Database.Models.RowData>();
                foreach (var itemRow in rowsData)
                {
                    if (itemRow == null)
                        continue;
                    if (_maxPeriodRowData != DateTime.MinValue && itemRow.Period <= _maxPeriodRowData)
                        if (_context.RowDataExistOnDatabase(_system, itemRow))
                            continue;

                    newEntities.Add(new Database.Models.RowData(_system, itemRow, _referencesCache));
                }
                
                _context.AddRange(newEntities);
                _context.SaveChanges();
            }
        }
        public override void SetInformationSystem(InformationSystemsBase system)
        {
            using (EventLogContext _context = EventLogContext.Create(_databaseOptions, _databaseActions))
                _system = _context.CreateOrUpdateInformationSystem(system);
        }
        public override void UpdateReferences(ReferencesData data)
        {
            using (EventLogContext _context = EventLogContext.Create(_databaseOptions, _databaseActions))
            {
                _context.FillReferencesToSave(_system, data);
                _context.SaveChanges();

                if (_referencesCache == null)
                    _referencesCache = new ReferencesDataCache(_system);
                _referencesCache.FillByDatabaseContext(_context);
            }
        }

        #endregion
    }
}
