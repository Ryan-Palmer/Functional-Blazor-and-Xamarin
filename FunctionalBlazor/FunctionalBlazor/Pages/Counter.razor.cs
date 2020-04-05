using Microsoft.AspNetCore.Components;
using Microsoft.FSharp.Core;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FunctionalBlazor.Caching.CachingTypes;
using static FunctionalBlazor.Program.ProgramTypes;

namespace FunctionalBlazor.Web.Pages
{
    public partial class Counter : ComponentBase, IDisposable
    {
        [Inject] // Might have to inject these through a normal contructor so that AutoData works in tests
        IJSRuntime? JSRuntime { get; set; }
        [Inject]
        IProgram? Program { get; set; }
        [Inject]
        ICache<ProgramModel>? ModelCache { get; set; }
        [Inject]
        NavigationManager? NavigationManager { get; set; }

        IDisposable? modelToken;
        CounterPageModel model = FunctionalBlazor.Program.Pages.Counter.init;
        Guid _lastNavId = Guid.Empty;
        Guid _lastAlertId = Guid.Empty;

        protected async override Task OnInitializedAsync()
        {
            await BindModel();
            base.OnInitialized();
        }

        async Task BindModel()
        {
            async void HandleModelUpdate(object sender, FSharpOption<ProgramModel> maybeProgramModel)
            {
                void TryPerformPendingNavigation(CounterPageModel model)
                {
                    try
                    {
                        var (destination, id) = model.PendingNavigation.Value;
                        if (_lastNavId != id)
                        {
                            _lastNavId = id;
                            NavigationManager?.NavigateTo(destination);
                            Program?.Post(
                                ProgramMsg.NewCounterPageMessage(
                                    CounterPageMsg.NavigationComplete));
                        }
                    }
                    catch (NullReferenceException e) { }
                }

                async Task TryShowPendingAlert(CounterPageModel model)
                {
                    try
                    {
                        var (message, id) = model.PendingAlert.Value;
                        if (_lastAlertId != id)
                        {
                            _lastAlertId = id;
                            if (JSRuntime != null)
                                await JSRuntime.InvokeAsync<object>("alert", message);
                            Program?.Post(
                                ProgramMsg.NewCounterPageMessage(
                                    CounterPageMsg.AlertShown));
                        }
                    }
                    catch (NullReferenceException e) { }
                }

                try
                {
                    model = maybeProgramModel.Value.CounterPage;
                    await InvokeAsync(async () => // This makes sure all updates happens on UI thread
                    {
                        StateHasChanged();
                        await TryShowPendingAlert(model);
                        TryPerformPendingNavigation(model);
                    });
                }
                catch (Exception e) { }
            }

            if (modelToken == null && ModelCache != null)
            {
                var subscribeMessage = CacheCommand<ProgramModel>.NewSubscribe(
                    FuncConvert.ToFSharpFunc<Tuple<object, FSharpOption<ProgramModel>>>(t =>
                         HandleModelUpdate(t.Item1, t.Item2)));

                switch (await ModelCache.Post(subscribeMessage))
                {
                    case CacheResult<ProgramModel>.Subscribed token:
                        modelToken = token.Item;
                        break;
                }
            }
        }

        void SelectItem(string itemId)
        {
            Program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.NewItemSelected(itemId)));
        }

        void IncrementCount()
        {
            Program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.IncreaseCount));
        }

        void ShowAlert()
        {
            Program?.Post(
                ProgramMsg.NewCounterPageMessage(
                    CounterPageMsg.NewCounterPageError(
                        new Exception("Some custom error message!"))));
        }

        public void Dispose()
        {
            if (modelToken != null)
            {
                modelToken.Dispose();
                modelToken = null;
            }
        }
    }
}