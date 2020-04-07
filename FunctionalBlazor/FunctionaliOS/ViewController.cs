using Foundation;
using FunctionalXamarin;
using GalaSoft.MvvmLight.Helpers;
using System;
using System.Collections.Generic;
using UIKit;
#nullable enable
namespace FunctionaliOS
{
    public partial class ViewController : UIViewController
    {
        CounterViewModel? _viewModel;
        readonly IList<Binding> _bindings = new List<Binding>();

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _viewModel = AppDelegate.RootServiceProvider?.GetService(typeof(CounterViewModel)) as CounterViewModel;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            SetBindings();
            ConnectHandlers();
            _viewModel?.Connect.Execute(null);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DetachBindings();
            _viewModel?.Disconnect.Execute(null);
            DisconnectHandlers();
        }

        
        void SetBindings()
        {
            if (_viewModel != null)
            {
                _bindings.Add(this.SetBinding(() => _viewModel.Title, () => TitleLabel.Text));
                _bindings.Add(this.SetBinding(() => _viewModel.Count, () => CounterLabel.Text));
                _bindings.Add(this.SetBinding(() => _viewModel.Utc, () => TimeLabel.Text));

                _bindings.Add(this.SetBinding<string>(
                    () => UsernameTextField.Text)
                    .ObserveSourceEvent(nameof(UsernameTextField.EditingChanged))
                    .WhenSourceChanges(() =>
                    {
                        _viewModel.SetUsername.Execute(UsernameTextField.Text);
                    }));

                IncrementButton.SetCommand(_viewModel.IncrementCount);
                AlertButton.SetCommand(_viewModel.ShowAlert);
            }
        }

        void DetachBindings()
        {
            foreach (var b in _bindings)
            {
                b.Detach();
            }

            _bindings.Clear();
        }

        void ConnectHandlers()      
        {
            if (_viewModel != null)
            {
                _viewModel.ShowDialog += HandleShowDialog;
                _viewModel.Navigate += HandleNavigate;
                _viewModel.Connect.Execute(null);
                if (NavButton != null) NavButton.TouchUpInside += SelectItem;
            }
        }

        void DisconnectHandlers()
        {
            if (_viewModel != null)
            {
                _viewModel.Disconnect.Execute(null);
                _viewModel.ShowDialog -= HandleShowDialog;
                _viewModel.Navigate -= HandleNavigate;

                if (NavButton != null) NavButton.TouchUpInside -= SelectItem;
            }
        }

        void SelectItem(object sender, EventArgs e)
        {
            _viewModel?.SelectItem.Execute(Guid.NewGuid().ToString());
        }

        void HandleNavigate(object sender, string e)
        {
            var failAlert = UIAlertController.Create("Navigate", $"Destination : {e}", UIAlertControllerStyle.Alert);
            failAlert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            PresentViewController(failAlert, true, null);
        }

        void HandleShowDialog(object sender, string message)
        {
            var failAlert = UIAlertController.Create("Alert", message, UIAlertControllerStyle.Alert);
            failAlert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            PresentViewController(failAlert, true, null);
        }

    }
}                   