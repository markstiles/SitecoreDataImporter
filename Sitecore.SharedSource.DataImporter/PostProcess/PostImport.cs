using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sitecore.Jobs;
using Sitecore.SharedSource.DataImporter.Logger;

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
			if (Sitecore.Context.Job != null)
				Sitecore.Context.Job.Options.Priority = ThreadPriority.Highest;

            //IEnumerable<Item> items;

            //int totalLines = items.Count();
            //if (Sitecore.Context.Job != null)
            //	  Sitecore.Context.Job.Status.Total = totalLines;

            //int line = 1;
            //foreach (Item a in items)
		    //{
		    //    try { 
            //    } catch (Exception ex) {
            //        Log.Log(a.Paths.FullPath, ex.ToString(), ProcessStatus.ReferenceError, string.Empty);
            //    }
            //    if (Sitecore.Context.Job != null)
            //	  {
            //		  Sitecore.Context.Job.Status.Processed = line;
            //		  Sitecore.Context.Job.Status.Messages.Add(string.Format("Processed item {0} of {1}", line, totalLines));
            //	  }
            //	  line++;
            //}

            if (Sitecore.Context.Job != null)
				Sitecore.Context.Job.Status.State = JobState.Finished;
		}
	}
}
