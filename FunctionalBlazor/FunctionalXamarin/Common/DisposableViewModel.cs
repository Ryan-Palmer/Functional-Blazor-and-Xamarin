using GalaSoft.MvvmLight;
using System;

namespace FunctionalXamarin.Common
{
    [Preserve(AllMembers = true)]
    public abstract class DisposableViewModel : ObservableObject, IDisposable
    {
        protected bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        // Example for derived VMs
        //protected override void Dispose(bool disposing)
        //{
        //    if (_disposed)
        //        return;

        //    // Free unmanaged resporces here using safe handle (release connections, close files etc)
        //    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose

        //    if (disposing)
        //    {
        //        // Free managed resources here (dispose child objects like Observable tokens etc)
        //    }

        //    _disposed = true;
        //}
    }
}
