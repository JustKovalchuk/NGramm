using System;
using System.Threading.Tasks;
using System.Threading;

namespace NGramm
{
    public sealed class ProgressReporter
    {
        private int progress;

        public event EventHandler<string> OperationNameChanged;
        public event EventHandler<int> ProgressChanged;
        public event EventHandler TimerStopRequest;
        public event EventHandler TimerStartRequest;

        public void StartNewOperation(string name)
        {
            Reset();
            OperationNameChanged?.Invoke(this, name);
            StartTimer();
        }

        public void MoveProgress(int inc = 1)
        {
            if (progress >= 100 || inc < 1) return;

            if (progress + inc >= 100)
            {
                Interlocked.Exchange(ref progress, 100);
            }
            else
            {
                Interlocked.Add(ref progress, inc);
            }

            ProgressChanged?.Invoke(this, progress);
        }

        public void Reset()
        {
            Interlocked.Exchange(ref progress, 0);
            ProgressChanged?.Invoke(this, 0);
            OperationNameChanged?.Invoke(this, string.Empty);
        }

        public void Finish()
        {
            StopTimer();
            Interlocked.Exchange(ref progress, 100);
            ProgressChanged?.Invoke(this, 100);

            Task.Delay(200).ContinueWith(t => Reset());
        }

        public void StopTimer() =>
            TimerStopRequest.Invoke(this, EventArgs.Empty);

        public void StartTimer() =>
            TimerStartRequest.Invoke(this, EventArgs.Empty);
    }
}
