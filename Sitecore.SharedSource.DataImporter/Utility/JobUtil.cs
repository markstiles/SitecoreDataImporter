using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sitecore.Jobs;

namespace Sitecore.SharedSource.DataImporter.Utility
{
    public static class JobUtil
    {
        public static void SetJobPriority(ThreadPriority priority)
        {
            if (Context.Job == null)
                return;

            Context.Job.Options.Priority = priority;
        }

        public static void SetJobTotal(int total)
        {
            if (Context.Job == null)
                return;

            Context.Job.Status.Total = total;
        }

        public static void SetJobStatus(long currentLine)
        {
            if (Context.Job == null)
                return;

            Context.Job.Status.Processed = currentLine;
            Context.Job.Status.Messages.Add($"Processed item {currentLine} of {Context.Job.Status.Total}");
        }

        public static void SetJobState(JobState state)
        {
            if (Context.Job == null)
                return;

            Context.Job.Status.State = state;
        }
    }
}
