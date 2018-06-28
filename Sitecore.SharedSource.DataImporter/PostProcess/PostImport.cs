using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sitecore.Jobs;
using Sitecore.SharedSource.DataImporter.Logger;
using Sitecore.SharedSource.DataImporter.Utility;

namespace Sitecore.SharedSource.DataImporter.PostProcess
{
	public class PostImport
	{
	    protected ILogger Log;

        public PostImport(ILogger l)
        {
            Log = l;
        }

		public void Process()
		{
            JobUtil.SetJobPriority(ThreadPriority.Highest);

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

            JobUtil.SetJobState(JobState.Finished);
		}
	}
}
