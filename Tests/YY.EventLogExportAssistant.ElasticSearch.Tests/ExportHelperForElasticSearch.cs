using System;
using System.IO;
using YY.EventLogExportAssistant.ElasticSearch.Tests.Models;

namespace YY.EventLogExportAssistant.ElasticSearch.Tests
{
    public static class ExportHelperForElasticSearch
    {
        public static void ExportToTargetStorage(EventLogExportSettingsForElasticSearch eventLogSettings, IEventLogOnTarget targetStorage)
        {
            if (!Directory.Exists(eventLogSettings.EventLogPath))
                throw new Exception("Каталог данных журнала регистрации не обнаружен.");

            using (EventLogExportMaster exporter = new EventLogExportMaster())
            {
                exporter.SetEventLogPath(eventLogSettings.EventLogPath);
                exporter.SetTarget(targetStorage);

                exporter.BeforeExportData += BeforeExportData;
                exporter.AfterExportData += AfterExportData;
                exporter.OnErrorExportData += OnErrorExportData;

                while (exporter.NewDataAvailable())
                    exporter.SendData();
            }
        }

        private static void BeforeExportData(BeforeExportDataEventArgs e)
        {
        }
        private static void AfterExportData(AfterExportDataEventArgs e)
        {
        }
        private static void OnErrorExportData(OnErrorExportDataEventArgs e)
        {
            throw e.Exception;
        }
    }
}
