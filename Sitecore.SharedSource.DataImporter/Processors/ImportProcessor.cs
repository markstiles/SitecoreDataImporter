using Sitecore.Data.Items;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sitecore.Data;
using Sitecore.Jobs;
using Sitecore.SecurityModel;
using CsvHelper;
using System.Web;

namespace Sitecore.SharedSource.DataImporter
{
	public class ImportProcessor
	{

		protected ILogger Logger { get; set; }

		protected IDataMap DataMap { get; set; }

		public ImportProcessor(IDataMap dm, ILogger l)
		{
			if (dm == null)
				throw new ArgumentNullException("The provided Data Map was null");
			if (l == null)
				throw new ArgumentNullException("The provided Logger was null");

			DataMap = dm;
			Logger = l;
		}
        
        /// <summary>
        /// processes each field against the data provided by subclasses
        /// </summary>
        public void Process()
		{

			Logger.Log($"Import Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", "N/A");

			if (Sitecore.Context.Job != null)
				Sitecore.Context.Job.Options.Priority = ThreadPriority.Highest;

			IEnumerable<object> importItems;
			try
			{
				importItems = DataMap.GetImportData();
			}
			catch (Exception ex)
			{
				Logger.Log($"GetImportData Failed: {ex.Message}", "N/A", Providers.ProcessStatus.Error);
				Logger.Log($"Import Finished at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", "N/A");

				if (Sitecore.Context.Job != null)
					Sitecore.Context.Job.Status.State = JobState.Finished;

				return;
			}

			int totalLines = importItems.Count();
			if (Sitecore.Context.Job != null)
				Sitecore.Context.Job.Status.Total = totalLines;

			long line = 0;

            var newItemList = new List<Item>();

			using (new SecurityDisabler())
			using (new BulkUpdateContext())
			{ // try to eliminate some of the extra pipeline work
				foreach (object importRow in importItems)
				{
					//import each row of data
					line++;
					try
					{
                        string newItemName = DataMap.BuildNewItemName(importRow);
                        
						if (string.IsNullOrEmpty(newItemName))
						{
							Logger.Log($"BuildNewItemName failed on import row {line} because the new item name was empty", "N/A", Providers.ProcessStatus.NewItemError);
							continue;
						}

						Item thisParent = DataMap.GetParentNode(importRow, newItemName);
						if (thisParent.IsNull())
						{
							Logger.Log($"Get parent failed on import row {line} because the new item's parent is null", "N/A", Providers.ProcessStatus.NewItemError);
							continue;
						}

						var newItem = DataMap.CreateNewItem(thisParent, importRow, newItemName);
                        if (newItem != null)
                            newItemList.Add(newItem);
                    }
					catch (Exception ex)
					{
						Logger.Log($"Exception thrown on import row {line} : {ex.Message}", "N/A", Providers.ProcessStatus.NewItemError);
					}

					if (Sitecore.Context.Job != null)
					{
						Sitecore.Context.Job.Status.Processed = line;
						Sitecore.Context.Job.Status.Messages.Add($"Processed item {line} of {totalLines}");
					}
				}
			}

            DataMap.PostProcess(newItemList);

            //if no messages then you're good
            if (!Logger.LoggedError)
				Logger.Log("The import completed successfully", "N/A");

			Logger.Log($"Import Finished at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", "N/A");

			if (Sitecore.Context.Job != null)
				Sitecore.Context.Job.Status.State = JobState.Finished;

			Sitecore.Caching.CacheManager.ClearAllCaches();

            foreach (KeyValuePair<string, List<ImportRow>> kvp in Logger.GetLogRecords())
            {
                string logPath = $"{HttpRuntime.AppDomainAppPath}Areas\\DataImport\\Logs\\{DataMap.ImportItem.DisplayName.Replace(" ", " - ")}.{DateTime.Now.ToString("yyyy.MM.dd.H.mm.ss")}.{kvp.Key}.csv";
                using (var file = System.IO.File.CreateText(logPath))
                {
                    var csvFile = new CsvWriter(file, false);
                    csvFile.WriteHeader<ImportRow>();
                    csvFile.NextRecord();
                    csvFile.WriteRecords(kvp.Value);
                }
            }
        }
	}
}
