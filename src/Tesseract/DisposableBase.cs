﻿
using System;
using System.Diagnostics;

namespace Tesseract
{
    public abstract class DisposableBase : IDisposable
    {
        static readonly TraceSource trace = new("Tesseract");

        protected DisposableBase() => IsDisposed = false;

        ~DisposableBase()
        {
            Dispose(false);
            trace.TraceEvent(TraceEventType.Warning, 0, "{0} was not disposed off.", this);
        }


        public void Dispose()
        {
            Dispose(true);

            IsDisposed = true;
            GC.SuppressFinalize(this);

            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public bool IsDisposed { get; private set; }

        public event EventHandler<EventArgs> Disposed;


        protected virtual void VerifyNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        protected abstract void Dispose(bool disposing);
    }
}
