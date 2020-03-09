using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sitecore.Jobs;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Services;

namespace Sitecore.SharedSource.DataImporter.Processors
{
	public class PostImport
	{
	    protected ILogger Log;
        protected JobService JobService { get; set; }

        public PostImport(ILogger l)
        {
            Log = l;
            JobService = new JobService();
        }

		public void Process()
		{
            JobService.SetJobPriority(ThreadPriority.Highest);

            //IEnumerable<Item> items;

            //int totalLines = items.Count();
            //JobUtil.SetJobTotal(totalLines);

            //int line = 1;
            //foreach (Item a in items)
            //{
            //    try { 
            //    } catch (Exception ex) {
            //        Log.Log(a.Paths.FullPath, ex.ToString(), ProcessStatus.ReferenceError, string.Empty);
            //    }
            //JobUtil.SetJobStatus(line);
            //	  line++;
            //}

            JobService.SetJobState(JobState.Finished);
		}
	}
}
