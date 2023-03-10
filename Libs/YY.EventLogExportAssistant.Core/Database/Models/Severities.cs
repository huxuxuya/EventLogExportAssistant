using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YY.EventLogExportAssistant.Database.Models
{
    public class Severities : ReferenceObject
    {
        #region Private Static Members

        private static readonly Dictionary<string, string> _mapPresentation = new Dictionary<string, string>()
        {
            { "Unknown", "Неизвестно" },
            { "Error", "Ошибка" },
            { "Information", "Информация" },
            { "Note", "Примечание" },
            { "Warning", "Предупреждение" }
        };

        #endregion

        #region Public Static Methods

        public static string GetPresentationByName(string name)
        {
            return _mapPresentation.TryGetValue(name, out string output) ? output : name;
        }

        #endregion

        #region Public Members

        [MaxLength(500)]
        public string Presentation
        {
            get => GetPresentationByName(Name);
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        #endregion
    }
}
