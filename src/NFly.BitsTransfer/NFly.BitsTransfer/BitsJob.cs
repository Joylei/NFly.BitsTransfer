using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NFly.BitsTransfer
{
    /// <summary>
    /// a wrapper for IBackgroundCopyJob
    /// </summary>
    public partial class BitsJob : IDisposable
    {
        private readonly IBackgroundCopyJob _job;

        public event Action<BitsProgress> OnProgress;

        private BitsProgressWatch _progressWatch;

        internal BitsJob(IBackgroundCopyJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            _job = job;

            _progressWatch = new BitsProgressWatch(this);
        }

        protected internal void ReportProgress(BitsProgress progress)
        {
            var ev = OnProgress;
            if (ev != null)
            {
                ev(progress);
            }
        }

        /// <summary>
        /// get job id
        /// </summary>
        public Guid Id
        {
            get
            {
                return CheckError(delegate
                {
                    Guid jobId;
                    _job.GetId(out jobId);
                    return jobId;
                });
            }
        }

        /// <summary>
        /// get or set job name
        /// </summary>
        public string DisplayName
        {
            get
            {
                return CheckError(delegate
                {
                    string name;
                    _job.GetDisplayName(out name);
                    return name;
                });
            }
            set { CheckError(delegate { _job.SetDisplayName(value); }); }
        }

        /// <summary>
        /// get or set job name
        /// </summary>
        public string Description
        {
            get
            {
                return CheckError(delegate
                {
                    string desc;
                    _job.GetDescription(out desc);
                    return desc;
                });
            }
            set { CheckError(delegate { _job.SetDescription(value); }); }
        }


        /// <summary>
        /// get or set priority
        /// </summary>
        public BG_JOB_PRIORITY Priority
        {
            get
            {
                return CheckError(delegate
                {
                    BG_JOB_PRIORITY priority;

                    _job.GetPriority(out priority);
                    return priority;
                });
            }
            set { CheckError(delegate { _job.SetPriority(value); }); }
        }

        /// <summary>
        /// get or set minimum retry delay
        /// </summary>
        public uint MinimumRetryDelay
        {
            get
            {
                return CheckError(delegate
                {
                    uint delay;
                    _job.GetMinimumRetryDelay(out delay);

                    return delay;
                });
            }
            set { CheckError(delegate { _job.SetMinimumRetryDelay(value); }); }
        }

        /// <summary>
        /// owner of job
        /// </summary>
        public string Owner
        {
            get
            {
                return CheckError(delegate
                {
                    string owner;


                    _job.GetOwner(out owner);

                    return owner;
                });
            }
        }

        /// <summary>
        /// total bytes of all files
        /// </summary>
        public ulong BytesTotal
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_PROGRESS progress;
                    _job.GetProgress(out progress);
                    return progress.BytesTotal;
                });
            }
        }

        /// <summary>
        /// bytes already transferred
        /// </summary>
        public ulong BytesTransferred
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_PROGRESS progress;
                    _job.GetProgress(out progress);
                    return progress.BytesTransferred;
                });
            }
        }

        /// <summary>
        /// total files count
        /// </summary>
        public uint FilesTotal
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_PROGRESS progress;
                    _job.GetProgress(out progress);
                    return progress.FilesTotal;
                });
            }
        }

        /// <summary>
        /// transferred file count
        /// </summary>
        public uint FilesTransferred
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_PROGRESS progress;
                    _job.GetProgress(out progress);
                    return progress.FilesTransferred;
                });
            }
        }

        /// <summary>
        /// transfer state
        /// </summary>
        public BG_JOB_STATE State
        {
            get
            {
                return CheckError(delegate
                {
                    BG_JOB_STATE state;
                    _job.GetState(out state);
                    return state;
                });
            }
        }

        /// <summary>
        /// transfer completed?
        /// </summary>
        public bool Transferred
        {
            get { return State == BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED; }
        }

        /// <summary>
        /// job creation time
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_TIMES times;
                    _job.GetTimes(out times);
                    return times.CreationTime;
                });
            }
        }

        /// <summary>
        /// job modification time
        /// </summary>
        public DateTime ModificationTime
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_TIMES times;
                    _job.GetTimes(out times);
                    return times.ModificationTime;
                });
            }
        }

        /// <summary>
        /// job transfer complete time
        /// </summary>
        public DateTime TransferCompletionTime
        {
            get
            {
                return CheckError(delegate
                {
                    _BG_JOB_TIMES times;
                    _job.GetTimes(out times);
                    return times.TransferCompletionTime;
                });
            }
        }

        internal void CheckError(Action action)
        {
            try
            {
                action();
            }
            catch (COMException ex)
            {
                throw new BitsException(this.Error, ex);
            }
        }

        internal TResult CheckError<TResult>(Func<TResult> action)
        {
            try
            {
                return action();
            }
            catch (COMException ex)
            {
                throw new BitsException(this.Error, ex);
            }
        }

        /// <summary>
        /// error if any
        /// </summary>
        private BitsError Error
        {
            get
            {
                BitsError error = new BitsError();

                BG_JOB_STATE jobState;
                this._job.GetState(out jobState);
                if (BG_JOB_STATE.BG_JOB_STATE_ERROR == jobState
                    || BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR == jobState)
                {
                    try
                    {
                        IBackgroundCopyError ppError;
                        this._job.GetError(out ppError);
                        if (null != ppError)
                        {
                            int code;
                            string pErrorDescription;
                            BG_ERROR_CONTEXT context;
                            ppError.GetErrorDescription(Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID),
                                out pErrorDescription);
                            ppError.GetError(out context, out code);
                            error.Code = code;
                            error.Context = context;
                            error.Message = pErrorDescription;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }

                return error;
            }
        }


        /// <summary>
        /// cancel the job
        /// </summary>
        public void Cancel()
        {
            CheckError(delegate
            {
                try
                {
                    _job.Cancel();
                }
                finally
                {
                    _progressWatch.Stop();
                }
            });
        }

        /// <summary>
        /// Suspend the job
        /// </summary>
        public void Suspend()
        {
            CheckError(delegate
            {
                try
                {
                    _job.Suspend();
                }
                finally
                {
                    _progressWatch.Stop();
                }
            });
        }

        /// <summary>
        /// resume the job
        /// </summary>
        public void Resume()
        {
            CheckError(delegate
            {
                _job.Resume();
                _progressWatch.Start();
            });
        }

        /// <summary>
        /// since files has been transferred, mark the job as completed
        /// </summary>
        public void Complete()
        {
            CheckError(delegate
            {
                _progressWatch.Stop();
                _job.Complete();
            });
        }

        /// <summary>
        /// add file to the job
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        public void AddFile(string remoteFile, string localFile)
        {
            CheckError(delegate { _job.AddFile(remoteFile, localFile); });
        }

        /// <summary>
        /// all files
        /// </summary>
        public IEnumerable<BitsFile> Files
        {
            get { return CheckError(() => GetFiles()); }
        }

        private IEnumerable<BitsFile> GetFiles()
        {
            IEnumBackgroundCopyFiles files;
            _job.EnumFiles(out files);
            if (files != null)
            {
                uint fileCount;
                files.GetCount(out fileCount);
                for (uint i = 0; i < fileCount; i++)
                {
                    IBackgroundCopyFile file;
                    uint fetchCount;
                    files.Next(i, out file, out fetchCount);
                    if (fetchCount == 1)
                    {
                        yield return new BitsFile(this, file);
                    }
                }
            }
        }

        public void Dispose()
        {
            _progressWatch.Dispose();
        }


        private static WeakReference _weakBCM = null;

        static IBackgroundCopyManager Manager
        {
            get
            {
                IBackgroundCopyManager bcm;
                if (_weakBCM != null)
                {
                    bcm = _weakBCM.Target as IBackgroundCopyManager;
                    if (bcm != null)
                    {
                        return bcm;
                    }
                }
                // ReSharper disable once SuspiciousTypeConversion.Global
                bcm = (IBackgroundCopyManager) new BackgroundCopyManager();
                _weakBCM = new WeakReference(bcm);
                return bcm;
            }
        }

        public static IEnumerable<BitsJob> AllJobs(JobOwner owner)
        {
            var bcm = Manager;
            uint count;

            IEnumBackgroundCopyJobs currentUserjobs;
            bcm.EnumJobs((uint) owner, out currentUserjobs);

            currentUserjobs.GetCount(out count);
            for (int i = 0; i < count; i++)
            {
                uint fetchedCount = 0;
                BitsJob.IBackgroundCopyJob currentJob;
                currentUserjobs.Next(1, out currentJob, out fetchedCount);
                if (fetchedCount == 1)
                {
                    yield return new BitsJob(currentJob);
                }
            }
        }

        public static BitsJob GetJob(Guid jobId)
        {
            return AllJobs(JobOwner.AllUsers).FirstOrDefault(x => x.Id == jobId);
        }

        public static BitsJob GetJob(string jobName)
        {
            return AllJobs(JobOwner.AllUsers).FirstOrDefault(x => x.DisplayName == jobName);
        }

        public static BitsJob CreateJob(string jobName)
        {
            Guid jobId;
            IBackgroundCopyJob job;
            Manager.CreateJob(jobName, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out jobId, out job);
            return new BitsJob(job);
        }
    }
}