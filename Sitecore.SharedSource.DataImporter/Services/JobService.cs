using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore;
using Sitecore.Jobs;

namespace Sitecore.SharedSource.DataImporter.Services
{
    public class JobService
    {
        public void SetJobPriority(ThreadPriority priority)
        {
            if (Context.Job == null)
                return;

            Context.Job.Options.Priority = priority;
        }

        public void SetJobTotal(int total)
        {
            if (Context.Job == null)
                return;

            Context.Job.Status.Total = total;
        }

        public void SetJobStatus(long currentLine)
        {
            if (Context.Job == null)
                return;

            Context.Job.Status.Processed = currentLine;
            Context.Job.Status.Messages.Add($"Processed item {currentLine} of {Context.Job.Status.Total}");
        }

        public void SetJobState(JobState state)
        {
            if (Context.Job == null)
                return;

            Context.Job.Status.State = state;
        }
    }
}
