using System;

namespace Metrics.Utils
{
    public struct TimeMeasuringContext : TimerContext
    {
        private readonly Clock clock;
        private readonly long start;
        private readonly Action<long, bool> action;
        private bool cancelled;
        private bool disposed;

        public TimeMeasuringContext(Clock clock, Action<long, bool> disposeAction)
        {
            this.clock = clock;
            this.start = clock.Nanoseconds;
            this.action = disposeAction;
            this.disposed = false;
            this.cancelled = false;
        }
        public bool Cancelled { get { return cancelled; } set { cancelled = value; } }

        public TimeSpan Elapsed
        {
            get
            {
                var miliseconds = TimeUnit.Nanoseconds.Convert(TimeUnit.Milliseconds, this.clock.Nanoseconds - this.start);
                return TimeSpan.FromMilliseconds(miliseconds);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            this.disposed = true;
            this.action(this.clock.Nanoseconds - this.start, Cancelled);
        }
    }
}
