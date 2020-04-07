using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Widget;
using FunctionalXamarin;
using GalaSoft.MvvmLight.Helpers;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
#nullable enable

namespace FunctionalAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class CounterActivity : AndroidX.AppCompat.App.AppCompatActivity
    {
        CounterViewModel? _viewModel;
        readonly IList<Binding> _bindings = new List<Binding>();

        TextView? _tvTitle;
        TextView? _tvCounter;
        TextView? _tvTime;
        Button? _btnIncrement;
        Button? _btnAlert;
        Button? _btnNavigate;
        TextInputEditText? _etUsername;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_counter);
            _viewModel = MainApplication.RootServiceProvider?.GetService(typeof(CounterViewModel)) as CounterViewModel;
            FindSubViews();
            SetBindings();
        }

        protected override void OnStart()
        {
            base.OnStart();
            ConnectHandlers();
        }

        protected override void OnStop()
        {
            base.OnStop();
            DisconnectHandlers();
        }

        protected override void OnDestroy()
        {
            DetachBindings();
            base.OnDestroy();
        }

        void FindSubViews()
        {
            _tvTitle = FindViewById<TextView>(Resource.Id.tvTitle);
            _tvCounter = FindViewById<TextView>(Resource.Id.tvCounter);
            _tvTime = FindViewById<TextView>(Resource.Id.tvTime);
            _btnIncrement = FindViewById<Button>(Resource.Id.btnIncrement);
            _btnAlert = FindViewById<Button>(Resource.Id.btnAlert);
            _btnNavigate = FindViewById<Button>(Resource.Id.btnNav);
            _etUsername = FindViewById<TextInputEditText>(Resource.Id.etUsername);
        }

        void SetBindings()
        {
            if (_viewModel != null)
            {
                if (_tvTitle != null) _bindings.Add((this).SetBinding<string, string>(() => _viewModel.Title, () => _tvTitle.Text));
                if (_tvCounter != null) _bindings.Add((this).SetBinding(() => _viewModel.Count, () => _tvCounter.Text));
                if (_tvTime != null) _bindings.Add((this).SetBinding(() => _viewModel.Utc, () => _tvTime.Text));
                if (_etUsername != null)
                    _bindings.Add(this.SetBinding<string>(
                        () => _etUsername.Text)
                        .ObserveSourceEvent<TextChangedEventArgs>(nameof(_etUsername.TextChanged))
                        .WhenSourceChanges(() => _viewModel.SetUsername.Execute(_etUsername.Text)));
                _btnIncrement?.SetCommand("Click", _viewModel.IncrementCount);
                _btnAlert?.SetCommand("Click", _viewModel.ShowAlert);
            }
        }

        void DetachBindings()
        {
            foreach (var b in _bindings)
            {
                b.Detach();
            }
        }

        void ConnectHandlers()
        {
            if (_viewModel != null)
            {
                _viewModel.ShowDialog += HandleShowDialog;
                _viewModel.Navigate += HandleNavigate;
                _viewModel.Connect.Execute(null);

                if (_btnNavigate != null) _btnNavigate.Click += SelectItem;
            }
        }

        void DisconnectHandlers()
        {
            if (_viewModel != null)
            {
                _viewModel.Disconnect.Execute(null);
                _viewModel.ShowDialog -= HandleShowDialog;
                _viewModel.Navigate -= HandleNavigate;

                if (_btnNavigate != null) _btnNavigate.Click -= SelectItem;
            }
        }

        void SelectItem(object sender, EventArgs e)
        {
            _viewModel?.SelectItem.Execute(Guid.NewGuid().ToString());
        }

        void HandleShowDialog(object sender, string message)
        {
#nullable disable warnings
            new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle("Alert")
                .SetMessage(message)
                .SetPositiveButton("Ok", (IDialogInterfaceOnClickListener)null)
                .Create()
                .Show();
#nullable enable warnings
        }

        void HandleNavigate(object sender, string e)
        {
#nullable disable warnings
            new AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle("Navigate")
                .SetMessage($"Destination : {e}")
                .SetPositiveButton("Ok", (IDialogInterfaceOnClickListener)null)
                .Create()
                .Show();
#nullable enable warnings
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
