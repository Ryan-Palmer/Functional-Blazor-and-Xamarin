// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace FunctionaliOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIButton AlertButton { get; set; }

		[Outlet]
		UIKit.UILabel CounterLabel { get; set; }

		[Outlet]
		UIKit.UIButton IncrementButton { get; set; }

		[Outlet]
		UIKit.UIButton NavButton { get; set; }

		[Outlet]
		UIKit.UILabel TimeLabel { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }

		[Outlet]
		UIKit.UITextField UsernameTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CounterLabel != null) {
				CounterLabel.Dispose ();
				CounterLabel = null;
			}

			if (IncrementButton != null) {
				IncrementButton.Dispose ();
				IncrementButton = null;
			}

			if (NavButton != null) {
				NavButton.Dispose ();
				NavButton = null;
			}

			if (TimeLabel != null) {
				TimeLabel.Dispose ();
				TimeLabel = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (UsernameTextField != null) {
				UsernameTextField.Dispose ();
				UsernameTextField = null;
			}

			if (AlertButton != null) {
				AlertButton.Dispose ();
				AlertButton = null;
			}
		}
	}
}
