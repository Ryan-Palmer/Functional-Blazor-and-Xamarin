using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using FunctionalXamarin;
using GalaSoft.MvvmLight.Helpers;
using Android.App;
#nullable enable

namespace FunctionalAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class CounterActivity : AndroidX.AppCompat.App.AppCompatActivity
    {
        CounterViewModel? _viewModel;
        readonly IList<Binding> _bindings = new List<Binding>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_counter);
            // resolve view model
        }

        void SetBindings()
        {
            if (_viewModel != null)
            {
                // bind view model
            }
        }

        void DetachBindings()
        {
            foreach (var b in _bindings)
            {
                b.Detach();
            }
        }

        void FindSubViews(View view)
        {
        }

        void ConnectHandlers()
        {
            if (_viewModel != null)
            {
                _viewModel.ShowDialog += HandleShowDialog;
                _viewModel.Navigate += HandleNavigate;
                _viewModel.Connect.Execute(null);
            }
        }

        void DisconnectHandlers()
        {
            if (_viewModel != null)
            {
                _viewModel.Disconnect.Execute(null);
                _viewModel.ShowDialog -= HandleShowDialog;
                _viewModel.Navigate -= HandleNavigate;
            }
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
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}
