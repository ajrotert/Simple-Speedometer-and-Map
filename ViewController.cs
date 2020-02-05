using System;
using CoreLocation;
using UIKit;
using MapKit;
using Foundation;
using CoreGraphics;
using AVFoundation;
using System.Collections.Generic;
using System.Threading;

namespace AR.Speedometer
{
    public partial class ViewController : UIViewController
    {
        public static LocationSetup LocationFinderManager;
        public ViewController(IntPtr handle) : base(handle)
        {
            LocationFinderManager = new LocationSetup();
            LocationAllowed = LocationFinderManager.LocationServeciesStarted(true);
            LocationAllowed = LocationFinderManager.StartLocationUpdates();
        }

        private bool LocationAllowed;
        private MKMapView MapView;
        MapDelegate mapDelegate;
        private readonly CLLocationManager locationManager = new CLLocationManager();
        UIAlertController alertController;
        Thread TraceThread;
        Thread SpeedThread;
        Thread Locations;
        CircleGraph circleGraph;
        CircleGraph LowerGraph;
        private int lastSpeed = 0;
        private int maxSpeed = 0;
        private static Queue<int> speeds = new Queue<int>();
        CGRect frame;
        bool showSpeed = true;
        private const double MPH = 2.23694;

        public void HandleLocationChanged(object sender, LocationUpdatedEventArgs e)
        {
            CLLocation loc = e.Location;

            int speed = (int)(Math.Round(loc.Speed* MPH));
            if (speed >= 0)
            {

                if(speed>maxSpeed)
                {
                    maxSpeed = speed;
                    Console.WriteLine("New max speed: " + speed);
                }
                if (speed != lastSpeed)
                {
                    Console.WriteLine("Speed Added to queue: " + speed);
                    speeds.Enqueue(speed);
                }
                lastSpeed = speed;
            }

        }

        private void CreateSpeedometerRound(float speed, float MaxSpeed)
        {
            if(circleGraph!=null)
            {
                circleGraph.RemoveFromSuperview();
                LowerGraph.RemoveFromSuperview();
            }

            if (speed > 100)
            {
                speed = 100;
            }
            else if(maxSpeed > 100)
            {
                MaxSpeed = 100;
            }

            speed = 1f - (speed / 100f);
            MaxSpeed = 1f - (MaxSpeed / 100f) * -1f;

            circleGraph = new CircleGraph(frame, 15, speed, true);
            circleGraph.Bounds = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);
            circleGraph.Center = new CGPoint(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2 - (int)ConstMinSpeed.Bounds.Height / 2);

            LowerGraph = new CircleGraph(frame, 15, MaxSpeed, false);
            LowerGraph.Bounds = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);
            LowerGraph.Center = new CGPoint(UIScreen.MainScreen.Bounds.Width / 2, UIScreen.MainScreen.Bounds.Height / 2 + (int)ConstMinSpeed.Bounds.Height / 2);

            View.AddSubview(circleGraph);
            View.AddSubview(LowerGraph);
        }
        private void UpdateSpeed()
        {
            while(true)
            {
                if(speeds.Count>0)
                {
                    InvokeOnMainThread(delegate
                    {
                        UIApplicationState state = UIApplication.SharedApplication.ApplicationState;
                        if (state == UIApplicationState.Active)
                        {
                            float speed = speeds.Dequeue();
                            Console.WriteLine("Speed Removed From queue");
                            DigitalSpeed.Text = speed.ToString();
                            if(showSpeed && speed<=100)
                                CreateSpeedometerRound(speed, maxSpeed);
                        }
                    });
                }
                    
                Thread.Sleep(500);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //When the user enters the foreground
            UIApplication.Notifications.ObserveDidBecomeActive((sender, args) =>
            {       //THIS WILL HANDLE UPDATES TO THE USER INTERFACE
                LocationFinderManager.LocationUpdated += HandleLocationChanged;
                Console.WriteLine("Active::");
            });
            //When the user leaves the app
            UIApplication.Notifications.ObserveDidEnterBackground((sender, args) =>
            {       //STOPS UPDATES FROM THE USER INTERFACE
                LocationFinderManager.LocationUpdated -= HandleLocationChanged;
                Console.WriteLine("Deactive::");
            });

            Foundation.NSNotificationCenter.DefaultCenter.AddObserver(new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);
        }

        private void StartFunction()
        {
            StopButton.Enabled = true;

            if (!LocationAllowed)
            {
                Locations = new Thread(AskForLocation);
                Locations.Start();
            }
            else
            {
                LocationFinderManager.StartDataCollection();
            }

            frame = new CGRect(DigitalSpeed.Bounds.X, DigitalSpeed.Bounds.Y, DigitalSpeed.Bounds.Width, DigitalSpeed.Bounds.Height * 1.5);

            CreateSpeedometerRound(0, 0);

            SpeedThread = new Thread(UpdateSpeed);
            SpeedThread.Start();

            DeviceRotated();
        }
        private void StopFunction()
        {
            StopButton.Enabled = false;

            if(TraceThread!=null)
                TraceThread.Abort();
            if(SpeedThread != null)
                SpeedThread.Abort();
            if(Locations!=null)
                Locations.Abort();

            LocationFinderManager.EndDataCollection();
            LocationFinderManager.EndLocationUpdates();

            if(MapView != null)
            {
                MapView.ShowsUserLocation = false;
                MapView.UserTrackingMode = MKUserTrackingMode.None;
            }

            DigitalSpeed.Text = "Ended";
            View.BringSubviewToFront(DigitalSpeed);
            CreateSpeedometerRound(0f, maxSpeed);

        }

        public override void ViewDidAppear(bool animated)
        {
            StartFunction();
        }


        private void DeviceRotated(NSNotification notification=null)
        {
            //if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            //{
                if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeLeft || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.LandscapeRight)
                {
                    LoadMap();
                }
                else if (UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.PortraitUpsideDown || UIDevice.CurrentDevice.Orientation == UIDeviceOrientation.Portrait)
                {
                    LoadSpeed();
                }
            //}

        }

        private void LoadMap()
        {
            Console.WriteLine("Load Map");
            showSpeed = false;
            if (MapView == null)
            {
                Console.WriteLine("Load New Map");
                MapView = new MKMapView(UIScreen.MainScreen.Bounds);
                MapView.ShowsUserLocation = true;
                MapView.MapType = MapKit.MKMapType.Standard;
                MapView.TintColor = UIColor.Black;
                MapView.ShowsCompass = true;
                MapView.ShowsScale = true;
                MapView.ShowsBuildings = true;
                MapView.ShowsTraffic = true;
                MapView.UserTrackingMode = MKUserTrackingMode.FollowWithHeading;

                //Adding are own map delegate for custom overlay views
                mapDelegate = new MapDelegate();
                MapView.Delegate = mapDelegate;

                View.AddSubview(MapView);
            }
            else
            {
                Console.WriteLine("Use existing map");
                MapView.Hidden = false;
            }
            View.BringSubviewToFront(MapView);
            StartTrace();

        }
        private void LoadSpeed()
        {
            if (MapView != null)
            {
                Console.WriteLine("Load Speed");
                MapView.Hidden = true;
                StopTrace();
            }
            showSpeed = true;
        }

        private void AskForLocation()
        {
            int WaitTime = 5000;
            while (!LocationAllowed)
            {
                Thread.Sleep(WaitTime);
                ShowErrorMessage("Location Services Not Allowed", "Please turn on location services in settings.\nTurn on: Privacy->Location Services");
                LocationAllowed = LocationFinderManager.LocationServeciesStarted(false);
                LocationAllowed = LocationFinderManager.StartLocationUpdates();
                WaitTime += WaitTime;
                if (WaitTime > 60000)
                    WaitTime = 60000;
            }
            LocationFinderManager.StartDataCollection();
        }

        public void ShowErrorMessage(string title = "Location Services OFF", string message = "Please turn on location services in settings.\nTurn on: Privacy->Location Services")
        {
            InvokeOnMainThread(delegate
            {
                alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.ActionSheet);
                //Add Action
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                // Present Alert
                PresentViewController(alertController, true, null);
            });
        }

        private void UpdateAnnotation()
        {
            //Removes all current overlays
            
            while (true)
            {

                CLLocationCoordinate2D[] dataArray = LocationFinderManager.data.GetAndRemoveData();
                if (dataArray.Length > 1)
                {
                    InvokeOnMainThread(delegate
                    {
                        UIApplicationState state = UIApplication.SharedApplication.ApplicationState;
                        if (state == UIApplicationState.Active)
                        {
                            MapView.AddOverlay(MKPolyline.FromCoordinates(dataArray));
                        }
                    });
                }
                Thread.Sleep(1000);
            }

        }
        private void StartTrace()
        {
            TraceThread = new Thread(UpdateAnnotation);
            TraceThread.Start();
        }
        private void StopTrace()
        {
            TraceThread.Abort();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void StopButton_TouchUpInside(UIButton sender)
        {
            StopFunction();
        }
    }



    public class CircleGraph : UIView
    {
        //        const float FULL_CIRCLE = 2*(float)Math.PI;

        const float FULL_CIRCLE = -(float)Math.PI; //
        int _radius = 20;
        //int _radius = 10;
        int _lineWidth = 10;
        nfloat _percentComplete = 0.0f;
        UIColor _frontColor = UIColor.LightGray; //UIColor.FromRGB(46, 60, 76);
        UIColor _backColor = UIColor.SystemBlueColor; //UIColor.FromRGB(234, 105, 92);
        bool _direction = true;

        public CircleGraph(CGRect frame, int lineWidth, nfloat percentComplete, bool direction)
        {
            _lineWidth = lineWidth;
            _percentComplete = percentComplete;
            this.Frame = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);
            this.BackgroundColor = UIColor.Clear;
            _direction = direction;
            //this.BackgroundColor = UIColor.White;

        }

        public CircleGraph(CGRect frame, int lineWidth, nfloat percentComplete, UIColor backColor, UIColor frontColor)
        {
            _lineWidth = lineWidth;
            _percentComplete = percentComplete;
            this.Frame = new CGRect(frame.X, frame.Y, frame.Width, frame.Height);
            this.BackgroundColor = UIColor.Clear;
            //this.BackgroundColor = UIColor.White;
            _backColor = backColor;
            _frontColor = frontColor;
        }

        public override void Draw(CoreGraphics.CGRect rect)
        {
            base.Draw(rect);

            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                var diameter = Math.Min(this.Bounds.Width, this.Bounds.Height);
                _radius = (int)(diameter / 2) - _lineWidth;
                //_radius = (int)(diameter / 2);
                DrawGraph(g, this.Bounds.GetMidX(), this.Bounds.GetMidY());
            }
        }

        public void DrawGraph(CGContext g, nfloat x, nfloat y)
        {
            g.SetLineWidth(_lineWidth);

            // Draw background circle
            CGPath path = new CGPath();
            _backColor.SetStroke();
            path.AddArc(x, y, _radius, 0, FULL_CIRCLE, _direction);
            g.AddPath(path);
            g.DrawPath(CGPathDrawingMode.Stroke);
            // Draw overlay circle
            var pathStatus = new CGPath();
            _frontColor.SetStroke();

             //Same Arc params except direction so colors don't overlap
            pathStatus.AddArc(x, y, _radius, 0, _percentComplete * FULL_CIRCLE, _direction);
            g.AddPath(pathStatus);
            g.DrawPath(CGPathDrawingMode.Stroke);
        }
    }
}