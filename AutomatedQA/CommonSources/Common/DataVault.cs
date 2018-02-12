using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedQA.CommonSources.Common
{
    public static class DataVault
    {
        private static ConcurrentStack<string> _voucherStack = new ConcurrentStack<string>();
        private static readonly object syncObject = new object();

        public static string StoredVoucher
        {
            get
            {
                string returnMessage = string.Empty;
                if (_voucherStack.Count >0)
                {
                    lock (syncObject)
                    {
                        _voucherStack.TryPop(out returnMessage);
                    }
                }
                return returnMessage;
            }
            set
            {
                lock (syncObject)
                {
                    _voucherStack.Push(value);
                }
            }
        }
                
    }
}
