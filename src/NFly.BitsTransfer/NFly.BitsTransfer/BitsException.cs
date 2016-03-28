using System;

namespace NFly.BitsTransfer
{
    public class BitsException : Exception
    {
        public BitsException(BitsError err, Exception ex)
            : base(ex.Message, ex)
        {
            this.Data["_info"] = err;
        }
    }
}