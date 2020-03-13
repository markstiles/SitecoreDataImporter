using System;

namespace Sitecore.SharedSource.DataImporter.Providers
{
    public enum LogType
    {
        Info,
        Error,
        Warning,
        ConnectionError,
        NewItemError,
        FieldError,
        DateParseError,
        NotFoundError,
        ImportDefinitionError,
        GetImportDataError,
        VideoInfo,
        MultilistToComponent,
        PresentationService,
        FieldToComponent
    }

    public class ImportRow
    {
        public virtual string ErrorMessage { get; set; }
        public virtual string AffectedItem { get; set; }
        public virtual string FieldName { get; set; }
        public virtual string FieldValue { get; set; }
    }
}