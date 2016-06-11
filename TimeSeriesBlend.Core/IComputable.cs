using System;

namespace TimeSeriesBlend.Core
{
    public class ComputationArgs : EventArgs
    {
        public int NumberOfAttempts { get; internal set; }
    }

    public class ProgressArgs : ComputationArgs
    {
        public bool Cancel { get; set; }

        public string VariableName { get; internal set; }
        public string GroupName { get; internal set; }
        public int UpperBound { get; internal set; }
        public int Current { get; internal set; }
    }

    public class ComputationErrorArgs : ComputationArgs
    {
        public Exception Exception { get; internal set; }
    }

    public interface IComputable
    {
        void Compute(ComputationParameters parameters);

        event EventHandler<ProgressArgs> OnProgress;
        event EventHandler<ComputationArgs> OnComputationStart;
        event EventHandler<ComputationArgs> OnComputationFinish;
        event EventHandler<ComputationErrorArgs> OnComputationError;
    }
}
