﻿using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SharedSource.DataImporter.Extensions;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Mappings.Fields;
using Sitecore.SharedSource.DataImporter.Providers;
using Sitecore.SharedSource.DataImporter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Jobs;

namespace Sitecore.SharedSource.DataImporter {
    public class ImportProcessor {

        protected ILogger Logger { get; set; }

        protected IDataMap DataMap { get; set; }

        public ImportProcessor(IDataMap dm, ILogger l) {
            if (dm == null)
                throw new ArgumentNullException("The provided Data Map was null");
            if(l == null)
                throw new ArgumentNullException("The provided Logger was null");

            DataMap = dm;
            Logger = l;
        }

        /// <summary>
        /// processes each field against the data provided by subclasses
        /// </summary>
        public void Process() {

            if (Sitecore.Context.Job != null)
                Sitecore.Context.Job.Options.Priority = ThreadPriority.Highest;

            IEnumerable<object> importItems;
            try {
                importItems = DataMap.GetImportData();
            } catch (Exception ex) {
                Logger.LogError("Import Error", ex.Message);
                return;
            }

            int totalLines = importItems.Count();
            if (Sitecore.Context.Job != null)
                Sitecore.Context.Job.Status.Total = totalLines;

            long line = 0;

            Logger.Log("Import Started", string.Format("start time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            using (new BulkUpdateContext()) { // try to eliminate some of the extra pipeline work
                foreach (object importRow in importItems) {
                    //import each row of data
                    line++;
                    try {
                        string newItemName = DataMap.BuildNewItemName(importRow);
                        if (string.IsNullOrEmpty(newItemName)) {
                            Logger.LogError(string.Format("Get Name Error (import row: {0})", line),
                                "Item wasn't created because the new item name was empty");
                            continue;
                        }

                        Item thisParent = DataMap.GetParentNode(importRow, newItemName);
                        if (thisParent.IsNull()) {
                            Logger.LogError(string.Format("Get Parent Error (import row: {0})", line),
                                "The new item's parent is null");
                            continue;
                        }

                        DataMap.CreateNewItem(thisParent, importRow, newItemName);
                    } catch (Exception ex) {
                        Logger.LogError(string.Format("Exception thrown (import row: {0})", line), ex.Message);
                    }

                    if (Sitecore.Context.Job != null)
                    {
                        Sitecore.Context.Job.Status.Processed = line;
                        Sitecore.Context.Job.Status.Messages.Add(string.Format("Processed item {0} of {1}", line, totalLines));
                    }
                }
            }

            //if no messages then you're good
            if (!Logger.LoggedError)
                Logger.Log("Success", "the import completed successfully");

            Logger.Log("Import Finished", string.Format("end time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            if (Sitecore.Context.Job != null)
                Sitecore.Context.Job.Status.State = JobState.Finished;
        }
    }
}