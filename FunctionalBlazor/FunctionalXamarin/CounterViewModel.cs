using FunctionalXamarin.Common;
using GalaSoft.MvvmLight.Command;
using Microsoft.FSharp.Core;
using System;
using Xamarin.Essentials;
using static FunctionalBlazor.Caching.CachingTypes;
using static FunctionalBlazor.Program.ProgramTypes;

namespace FunctionalXamarin
{
    [Preserve(AllMembers = true)]
    public class CounterViewModel : DisposableViewModel
    {
        readonly ICache<ProgramModel> _modelCache;
        readonly IProgram _program;

        public event EventHandler<string>? ShowDialog;
        public event EventHandler<string>? Navigate;

        IDisposable? _modelToken;
        Guid _lastDialogId = Guid.Empty;
        Guid _lastNavId = Guid.Empty;

        public CounterViewModel (
            ICache<ProgramModel> modelCache,
            IProgram program)
        {
            _modelCache = modelCache;
            _program = program;
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            private set
            {
                Set(ref _title, value);
            }
        }

        private string _utc = string.Empty;
        public string Utc
        {
            get => _utc;
            private set
            {
                Set(ref _utc, value);
            }
        }

        private int _count;
        public int Count
        {
            get => _count;
            private set
            {
                Set(ref _count, value);
            }
        }

        private RelayCommand<string>? _setUsername;
        public RelayCommand<string> SetUsername => _setUsername ?? (_setUsername = new RelayCommand<string>(DoSetUsername));
        void DoSetUsername(string username)
        {
            _program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.NewSetUsername(username)));
        }

        private RelayCommand<string>? _selectItem;
        public RelayCommand<string> SelectItem => _selectItem ?? (_selectItem = new RelayCommand<string>(DoSelectItem));
        void DoSelectItem(string itemId)
        {
            _program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.NewItemSelected(itemId)));
        }

        private RelayCommand? _incrementCount;
        public RelayCommand IncrementCount => _incrementCount ?? (_incrementCount = new RelayCommand(DoIncrementCount));
        void DoIncrementCount()
        {
            _program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.IncreaseCount));
        }

        private RelayCommand? _showAlert;
        public RelayCommand ShowAlert => _showAlert ?? (_showAlert = new RelayCommand(DoShowAlert));
        void DoShowAlert()
        {
            _program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.NewCounterPageError(
                        new Exception("Some custom error message!"))));
        }

        private RelayCommand? _connect;
        public RelayCommand Connect => _connect ?? (_connect = new RelayCommand(DoConnect));

        async void DoConnect()
        {
            void HandleModelUpdate(object sender, FSharpOption<ProgramModel> maybeProgramModel)
            {
                void UpdateModel(CounterPageModel model)
                {
                    Title = model.Title;
                    Utc = model.UTC;
                    Count = model.Count;
                }

                void TryShowPendingDialog(CounterPageModel model)
                {
                    try
                    {
                        var (message, id) = model.PendingAlert.Value;
                        if (_lastDialogId != id)
                        {
                            _lastDialogId = id;
                            ShowDialog?.Invoke(this, message);
                            _program?.Post(
                                ProgramMsg.NewCounterPageMessage(
                                    CounterPageMsg.AlertShown));
                        }
                    }
                    catch (NullReferenceException e) { }
                }

                void TryPerformPendingNavigation(CounterPageModel model)
                {
                    try
                    {
                        var (destination, id) = model.PendingNavigation.Value;
                        if (_lastNavId != id)
                        {
                            _lastNavId = id;
                            Navigate?.Invoke(this, destination);
                            _program?.Post(
                                ProgramMsg.NewCounterPageMessage(
                                    CounterPageMsg.NavigationComplete));
                        }
                    }
                    catch (NullReferenceException e) { }
                }

                try
                {
                    var model = maybeProgramModel.Value.CounterPage;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        UpdateModel(model);
                        TryShowPendingDialog(model);
                        TryPerformPendingNavigation(model);
                    });
                }
                catch (NullReferenceException e) { }
            }

            if (_modelToken == null)
            {
                var subscribeMessage = CacheCommand<ProgramModel>.NewSubscribe(
                    FuncConvert.ToFSharpFunc<Tuple<object, FSharpOption<ProgramModel>>>(t =>
                         HandleModelUpdate(t.Item1, t.Item2)));

                switch (await _modelCache.Post(subscribeMessage))
                {
                    case CacheResult<ProgramModel>.Subscribed token:
                        _modelToken = token.Item;
                        break;
                }
            }
        }

        private RelayCommand? _disconnect;
        public RelayCommand Disconnect => _disconnect ?? (_disconnect = new RelayCommand(DoDisconnect));

        void DoDisconnect()
        {
            _modelToken?.Dispose();
            _modelToken = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                DoDisconnect();
            }

            _disposed = true;
        }
    }
}
