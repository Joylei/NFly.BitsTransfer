// MIT License 2016 joylei(leingliu@126.com)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NFly.BitsTransfer
{
    [Serializable]
    public class BitsError
    {
        public int Code;
        public BitsJob.BG_ERROR_CONTEXT Context;

        public BitsError()
        {
            Code = 0;
        }

        public string Message;
    }
}