
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionalXamarin.Common
{
    /// <summary>
    /// 
    ///  Use this to stop text input spamming IProgram with messages which also
    ///  can cause loops if you use two way bindings (when two changes are sent 
    ///  before  any come back they start fighting to overwrite the property).
    ///  
    ///  Using this, each action waits for a moment before executing,
    ///  and any further actions cancel and replace it, so you only actually
    ///  run the final action.
    /// 
    ///  For a string property in a ViewModel, you can do:
    /// 
    ///  Action<string>? _debouncedTitle;
    ///  void DoUpdateTitle(string title)
    ///  {
    ///      _debounceTitle ??= new Action<string>(s =>
    ///      {
    ///          _program.Post(
    ///              ProgramMsg.NewSubPageMessage(
    ///                  NewSubPageMsg.NewTitleUpdated(s)));
    ///      }).Debounce();
    ///          
    ///      _debouncedTitle?.Invoke(title);
    ///  }
    ///  
    /// </summary>
    public static class Debouncer
    {
        public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
        {
            CancellationTokenSource? cancelTokenSource = null;

            return arg =>
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                Task.Delay(milliseconds, cancelTokenSource.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func(arg);
                        }
                    }, TaskScheduler.Default);
            };
        }
    }
}