using System;

namespace Fregata
{
    /// <summary>
    /// desc：
    /// author：yjq 2018/11/22 16:46:01
    /// </summary>
    public class Disposable : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeCode()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                DisposeCode();
            }
            _isDisposed = true;
        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}