﻿
using System;
using System.Collections.Generic;
using Metrics.MetricData;
using Metrics.Reporters;
namespace Metrics.Reports
{
    public sealed class MetricsReports : Utils.IHideObjectMembers, IDisposable
    {
        private readonly MetricsDataProvider metricsDataProvider;
        private readonly Func<HealthStatus> healthStatus;

        private readonly List<ScheduledReporter> reports = new List<ScheduledReporter>();

        public MetricsReports(MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus)
        {
            this.metricsDataProvider = metricsDataProvider;
            this.healthStatus = healthStatus;
        }

        /// <summary>
        /// Schedule a generic reporter to be executed at a fixed <paramref name="interval"/>
        /// </summary>
        /// <param name="report">Function that returns an instance of a reporter</param>
        /// <param name="interval">Interval at which to run the report.</param>
        /// <param name="runAtDispose">If report should be allowed a final run during Dispose</param>
        public MetricsReports WithReport(MetricsReport report, TimeSpan interval, bool runAtDispose = false)
        {
            var newReport = new ScheduledReporter(report, this.metricsDataProvider, this.healthStatus, interval, runAtDispose);
            this.reports.Add(newReport);
            return this;
        }

        /// <summary>
        /// Schedule a Console Report to be executed and displayed on the console at a fixed <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">Interval at which to display the report on the Console.</param>
        public MetricsReports WithConsoleReport(TimeSpan interval)
        {
            return WithReport(new ConsoleReport(), interval);
        }

        /// <summary>
        /// Configure Metrics to append a line for each metric to a CSV file in the <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory where to store the CSV files.</param>
        /// <param name="interval">Interval at which to append a line to the files.</param>
        /// <param name="delimiter">CSV delimiter to use</param>
        public MetricsReports WithCSVReports(string directory, TimeSpan interval, string delimiter = CSVAppender.CommaDelimiter)
        {
            return WithReport(new CSVReport(new CSVFileAppender(directory, delimiter)), interval);
        }

        /// <summary>
        /// Schedule a Human Readable report to be executed and appended to a text file.
        /// </summary>
        /// <param name="filePath">File where to append the report.</param>
        /// <param name="interval">Interval at which to run the report.</param>
        public MetricsReports WithTextFileReport(string filePath, TimeSpan interval)
        {
            return WithReport(new TextFileReport(filePath), interval);
        }

        /// <summary>
        /// Stop all registered reports and clear the registrations.
        /// </summary>
        public void StopAndClearAllReports()
        {
            this.reports.ForEach(r => r.Dispose());
            this.reports.Clear();
        }

        public void Dispose()
        {
            StopAndClearAllReports();
        }
    }
}
