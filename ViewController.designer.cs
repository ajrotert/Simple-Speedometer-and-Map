// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace AR.Speedometer
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ConstMaxSpeed { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ConstMinSpeed { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DigitalSpeed { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView MainView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton PlayButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton StopButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TopSpeedDigitalLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TopSpeedLabel { get; set; }

        [Action ("Start_Clicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void Start_Clicked (UIKit.UIButton sender);

        [Action ("StopButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void StopButton_TouchUpInside (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (ConstMaxSpeed != null) {
                ConstMaxSpeed.Dispose ();
                ConstMaxSpeed = null;
            }

            if (ConstMinSpeed != null) {
                ConstMinSpeed.Dispose ();
                ConstMinSpeed = null;
            }

            if (DigitalSpeed != null) {
                DigitalSpeed.Dispose ();
                DigitalSpeed = null;
            }

            if (MainView != null) {
                MainView.Dispose ();
                MainView = null;
            }

            if (PlayButton != null) {
                PlayButton.Dispose ();
                PlayButton = null;
            }

            if (StopButton != null) {
                StopButton.Dispose ();
                StopButton = null;
            }

            if (TopSpeedDigitalLabel != null) {
                TopSpeedDigitalLabel.Dispose ();
                TopSpeedDigitalLabel = null;
            }

            if (TopSpeedLabel != null) {
                TopSpeedLabel.Dispose ();
                TopSpeedLabel = null;
            }
        }
    }
}