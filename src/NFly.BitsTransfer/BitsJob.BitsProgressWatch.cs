// MIT License 2017 joylei(leingliu@gmail.com)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NFly.BitsTransfer
{
    public partial class BitsJob
    {
        private class BitsProgressWatch : IDisposable
        {
            const int QUEUE_SIZE_LIMIT = 5;
            readonly List<long> _bytesDeltaQueue = new List<long>();
            private readonly Timer _timer;
            private long _lastBytesTransferred = 0;


            public BitsProgressWatch(BitsTransfer.BitsJob job)
            {
                if (job == null)
                {
                    throw new ArgumentNullException("job");
                }

                this._timer = new Timer(OnTimerTick, job, Timeout.Infinite, Timeout.Infinite);
            }

            public void Start()
            {
                lock (_bytesDeltaQueue)
                {
                    _bytesDeltaQueue.Clear();
                    this._timer.Change(0, 1000);
                }
            }

            public void Stop()
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            private void OnTimerTick(object state)
            {
                var job = (BitsTransfer.BitsJob) state;
                var status = job.State;

                BitsProgress progress = new BitsProgress();
                progress.Job = job;

                if (status == BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED)
                {
                    progress.BytesTotal = (long) job.BytesTotal;
                    progress.BytesTransferred = (long) job.BytesTransferred;
                    progress.BytesRemaining = progress.BytesTotal - progress.BytesTransferred;
                    progress.EstimatedSpeed = 0;
                    progress.EstimatedTimeRemaining = -1;
                    progress.Percentage = 1;
                    job.ReportProgress(progress);

                    return;
                }
                if (status != BG_JOB_STATE.BG_JOB_STATE_TRANSFERRING)
                {
                    lock (_bytesDeltaQueue)
                    {
                        _bytesDeltaQueue.Clear();
                    }
                }


                progress.BytesTotal = (long) job.BytesTotal;
                progress.BytesTransferred = (long) job.BytesTransferred;
                progress.BytesRemaining = progress.BytesTotal - progress.BytesTransferred;

                var totalSeconds = -1L;
                var lastBytes = Interlocked.Exchange(ref _lastBytesTransferred, progress.BytesTransferred);
                var delta = progress.BytesTransferred - lastBytes;
                if (delta > 0)
                {
                    this.BytesDelta = delta;
                }
                var avgDelta = this.BytesDelta;
                if (lastBytes >= 0 && avgDelta > 0)
                {
                    totalSeconds = progress.BytesRemaining/avgDelta;
                }

                progress.EstimatedSpeed = avgDelta;
                progress.EstimatedTimeRemaining = totalSeconds;
                progress.Percentage = progress.BytesTotal > 0
                    ? progress.BytesTransferred/(float) progress.BytesTotal
                    : 0;
                job.ReportProgress(progress);
            }


            protected long BytesDelta
            {
                get
                {
                    lock (_bytesDeltaQueue)
                    {
                        return _bytesDeltaQueue.Count == 0
                            ? 0
                            : _bytesDeltaQueue.Sum()/_bytesDeltaQueue.Count;
                    }
                }

                set
                {
                    lock (_bytesDeltaQueue)
                    {
                        if (_bytesDeltaQueue.Count > QUEUE_SIZE_LIMIT)
                        {
                            _bytesDeltaQueue.RemoveAt(0);
                        }
                        _bytesDeltaQueue.Add(value);
                    }
                }
            }

            public void Dispose()
            {
                _timer.Dispose();
            }
        }
    }
}