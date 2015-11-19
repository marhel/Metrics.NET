using System;
using System.Threading;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public sealed class ScheduledReporter : IDisposable
    {
        private readonly Scheduler scheduler;
        private readonly MetricsReport report;
        private readonly MetricsDataProvider metricsDataProvider;
        private readonly Func<HealthStatus> healthStatus;
        private readonly bool runAtDispose;

        public ScheduledReporter(MetricsReport reporter, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, TimeSpan interval, bool runAtDispose)
            : this(reporter, metricsDataProvider, healthStatus, interval, runAtDispose, new ActionScheduler()) { }

        public ScheduledReporter(MetricsReport report, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, TimeSpan interval, bool runAtDispose, Scheduler scheduler)
        {
            this.report = report;
            this.metricsDataProvider = metricsDataProvider;
            this.healthStatus = healthStatus;
            this.scheduler = scheduler;
            this.runAtDispose = runAtDispose;
            this.scheduler.Start(interval, t => RunReport(t));
        }

        private void RunReport(CancellationToken token)
        {
            report.RunReport(this.metricsDataProvider.CurrentMetricsData, this.healthStatus, token);
        }

        public void Dispose()
        {
            // stop scheduler
            using (this.scheduler) { }
            // run report one last time, if requested
            if(runAtDispose)
            {
                var ct = new CancellationToken();
                this.RunReport(ct);
            }
            using (this.report as IDisposable) { }
        }
    }
}
