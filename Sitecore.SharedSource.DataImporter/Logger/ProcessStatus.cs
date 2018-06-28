using System;

namespace Sitecore.SharedSource.DataImporter.Providers
{
    public enum ProcessStatus
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
        PostProcessorError,
        GetImportDataError
    }

    public class ImportRow
    {
        //Should have properties which correspond to the Column Names in the file   
        public virtual string AffectedItem { get; set; }
        public virtual string ErrorMessage { get; set; }  
        public virtual string FieldName { get; set; }
        public virtual string FieldValue { get; set; }
    }
}