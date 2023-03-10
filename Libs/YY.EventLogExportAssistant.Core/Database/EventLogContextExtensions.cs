using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YY.EventLogExportAssistant.Database.Models;
using YY.EventLogReaderAssistant;

namespace YY.EventLogExportAssistant.Database
{
    public static class EventLogContextExtensions
    {
        #region Public Methods
        
        public static void FillReferencesToSave(this EventLogContext context, InformationSystemsBase system, ReferencesData data)
        {
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<Applications>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<Computers>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<Events>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<Metadata>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<PrimaryPorts>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<SecondaryPorts>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<Severities>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<TransactionStatuses>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<Users>(system));
            FillReferencesToSave(context, system, data.GetReferencesListForDatabaseType<WorkServers>(system));
        }
        public static void AddReferenceToSaveInDB<T>(this EventLogContext context, T item) where T : ReferenceObject
        {
            context.Set<T>().Add(item);
        }
        public static bool ReferenceExistInDB<T>(this EventLogContext context, InformationSystemsBase system, T item) where T : ReferenceObject
        {
            T foundItem = context.Set<T>()
                .FirstOrDefault(e => e.InformationSystemId == system.Id && e.Name == item.Name);

            return (foundItem != null);
        }
        public static DateTime GetRowsDataMaxPeriod(this EventLogContext context, InformationSystemsBase system)
        {
            DateTime maxPeriodRowData = DateTime.MinValue;
            RowData firstRow = context.RowsData.FirstOrDefault();
            if (firstRow != null)
            {
                var _maxPeriodData = context.RowsData
                    .Where(p => p.InformationSystemId == system.Id);
                if (_maxPeriodData.Any())
                {
                    DateTimeOffset _maxPeriodRowDataTimeOffset = _maxPeriodData.Max(m => m.Period);
                    maxPeriodRowData = _maxPeriodRowDataTimeOffset.DateTime;
                }
            }

            return maxPeriodRowData;
        }
        public static InformationSystems CreateOrUpdateInformationSystem(this EventLogContext context, InformationSystemsBase system)
        {
            InformationSystems existSystem = context.InformationSystems.FirstOrDefault(e => e.Name == system.Name);
            if (existSystem == null)
            {
                context.InformationSystems.Add(new InformationSystems()
                {
                    Name = system.Name,
                    Description = system.Description
                });
                context.SaveChanges();
                existSystem = context.InformationSystems.FirstOrDefault(e => e.Name == system.Name);
            }
            else
            {
                if (existSystem.Description != system.Description)
                {
                    existSystem.Description = system.Description;
                    context.Update(system);
                    context.SaveChanges();
                }
            }

            return existSystem;
        }
        public static void SaveLogPosition(this EventLogContext context, InformationSystemsBase system, FileInfo logFileInfo, EventLogPosition position)
        {
            LogFiles foundLogFile = context.LogFiles
                .FirstOrDefault(l => l.InformationSystemId == system.Id && l.FileName == logFileInfo.Name && l.CreateDate == logFileInfo.CreationTimeUtc);

            if (foundLogFile == null)
            {
                context.LogFiles.Add(new LogFiles(system, logFileInfo, position));
            }
            else
            {
                foundLogFile.ModificationDate = logFileInfo.LastWriteTimeUtc;
                foundLogFile.LastCurrentFileData = position.CurrentFileData;
                foundLogFile.LastCurrentFileReferences = position.CurrentFileReferences;
                foundLogFile.LastEventNumber = position.EventNumber;
                foundLogFile.LastStreamPosition = position.StreamPosition;
                context.Entry(foundLogFile).State = EntityState.Modified;
            }

            context.SaveChanges();
        }
        public static bool RowDataExistOnDatabase(this EventLogContext context, InformationSystemsBase system, YY.EventLogReaderAssistant.Models.RowData itemRow)
        {
            var checkExist = context.RowsData
                .FirstOrDefault(e => e.InformationSystemId == system.Id && e.Period == itemRow.Period && e.Id == itemRow.RowId);

            return (checkExist != null);
        }
        public static EventLogPosition GetLastPosition(this EventLogContext context, InformationSystemsBase system)
        {
            var lastLogFile = context.LogFiles
                .SingleOrDefault(e => e.InformationSystemId == system.Id
                                      && e.Id == context.LogFiles.Where(i => i.InformationSystemId == system.Id).Max(m => m.Id));

            if (lastLogFile == null)
                return null;
            else
                return new EventLogPosition(
                    lastLogFile.LastEventNumber,
                    lastLogFile.LastCurrentFileReferences,
                    lastLogFile.LastCurrentFileData,
                    lastLogFile.LastStreamPosition);
        }

        #endregion

        #region Private Methods

        private static void FillReferencesToSave<T>(EventLogContext context, InformationSystemsBase system, IReadOnlyList<T> sourceReferences)
            where T : ReferenceObject
        {
            if (sourceReferences == null)
                return;

            foreach (var itemReference in sourceReferences)
            {
                if (!context.ReferenceExistInDB(system, itemReference))
                {
                    context.AddReferenceToSaveInDB(itemReference);
                }
            }
        }

        #endregion
    }
}
