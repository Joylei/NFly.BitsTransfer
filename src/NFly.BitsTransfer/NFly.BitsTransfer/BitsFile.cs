using System;

namespace NFly.BitsTransfer
{
    /// <summary>
    /// a wrapper for IBackgroundCopyFile
    /// </summary>
    public class BitsFile
    {
        private readonly BitsJob _job;
        private readonly BitsJob.IBackgroundCopyFile _file;

        internal BitsFile(BitsJob job, BitsJob.IBackgroundCopyFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            _job = job;
            _file = file;
        }

        public string RemoteFile
        {

            get
            {
                string name;
                _file.GetRemoteName(out name);
                return name;
            }
        }

        public string LocalName
        {
            get
            {
                string name;
                _file.GetLocalName(out name);
                return name;
            }
        }

        public ulong BytesTotal
        {
            get
            {
                return _job.CheckError(delegate
                {
                    BitsJob._BG_FILE_PROGRESS progress;

                    _file.GetProgress(out progress);
                    return progress.BytesTotal;
                });
            }
        }

        public ulong BytesTransferred
        {
            get
            {
                return _job.CheckError(delegate
                {
                    BitsJob._BG_FILE_PROGRESS progress;

                    _file.GetProgress(out progress);

                    return progress.BytesTransferred;
                });
            }
        }

        public bool Completed
        {
            get
            {
                return _job.CheckError(delegate
                {
                    BitsJob._BG_FILE_PROGRESS progress;
                    _file.GetProgress(out progress);
                    return progress.Completed > 0;
                });
            }
        }
    }
}