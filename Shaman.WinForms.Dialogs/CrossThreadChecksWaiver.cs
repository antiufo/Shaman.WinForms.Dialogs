using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Shaman.WinForms
{
    internal class CrossThreadChecksWaiver : IDisposable
    {
        private static int count;
        private bool disposed;

        public CrossThreadChecksWaiver()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            Interlocked.Increment(ref count);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (Interlocked.Decrement(ref count) == 0)
                Control.CheckForIllegalCrossThreadCalls = true;
        }

    }
}
