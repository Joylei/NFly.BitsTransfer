// MIT License 2017 joylei(leingliu@gmail.com)
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