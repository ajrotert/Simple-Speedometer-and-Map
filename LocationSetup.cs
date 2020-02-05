using System;
using CoreLocation;
using MapKit;
using UIKit;
namespace AR.Speedometer
{
    public class LocationSetup
    {
        private readonly CLLocationManager locationManager;
        internal Data data;
        private bool started = false;
        public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate { };

        public LocationSetup()
        {
            locationManager = new CLLocationManager();
            locationManager.PausesLocationUpdatesAutomatically = false;
        }

        public bool LocationServeciesStarted(bool PrintErrorMessage)
        {
            bool pass = true;

            if (CLLocationManager.LocationServicesEnabled)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    locationManager.RequestAlwaysAuthorization();
                    Console.WriteLine("Requested Always");
                }
                else
                {
                    locationManager.RequestWhenInUseAuthorization();
                    Console.WriteLine("Requested In Use");
                }
            }
            else
            {
                if (PrintErrorMessage)
                {
                    try
                    {
                        var presentRootController = GetRootController();
                        presentRootController?.InvokeOnMainThread(delegate
                        {
                        //****Need to have a navigation controller for the view to be presented before the app loads
                        string TitleM = "Location Services OFF", MessageM = "Please turn on location services in settings." + Environment.NewLine + "Turn on: Privacy->Location Services";
                            UIAlertController alertController = UIAlertController.Create(TitleM, MessageM, UIAlertControllerStyle.ActionSheet);
                        //Add Action
                        alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));


                        // Present Alert


                        presentRootController?.PresentViewController(alertController, true, null);
                        });
                        //Console.WriteLine("Location Services OFF");
                    }
                    catch
                    {
                        Console.WriteLine("Error");
                    }
                }
                pass = false;
            }
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                locationManager.AllowsBackgroundLocationUpdates = true;
            }

            LocationUpdated += UpdateData;
            return pass;
        }

        public bool StartLocationUpdates()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                locationManager.AllowsBackgroundLocationUpdates = true;
            }
            if (CLLocationManager.LocationServicesEnabled)
            {
                locationManager.DesiredAccuracy = 1;
                locationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) =>
                {
                    LocationUpdated(this, new LocationUpdatedEventArgs(e.Locations[e.Locations.Length - 1]));
                };
                locationManager.StartUpdatingLocation();
                return true;
            }
            else
            {
                Console.WriteLine("Location services not turned on");
                return false;
            }
            

        }
        public void StartDataCollection()
        {
            started = true;
        }
        public void EndDataCollection()
        {
            try { data.locs.Clear(); }
            catch { Console.WriteLine("Data Empty"); }
            started = false;
        }
        public void EndLocationUpdates()
        {
            started = false;
            locationManager.StopUpdatingLocation();
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                locationManager.AllowsBackgroundLocationUpdates = false;
            }
        }
        public CLLocationCoordinate2D GetLocation()
        {
            return locationManager.Location.Coordinate;
        }
        public void UpdateData(object sender, LocationUpdatedEventArgs e)
        {
            if (started)
            {
                //Console.WriteLine("::Data::");
                if(data == null)
                {
                    try
                    {
                        data = new Data(locationManager.Location.Coordinate);//Initiallized data with the starting coordinate

                    }
                    catch
                    {
                        data = new Data(new CLLocationCoordinate2D(0, 0));//Initiallized data with the starting coordinate
                    }
                }
                data.Add(e.Location.Coordinate);    //Adds each coordinate to the list in data
                
            }
        }

        public UIViewController GetRootController()
        {
            var root = UIApplication.SharedApplication.KeyWindow.RootViewController;
            while (true)
            {
                switch (root)
                {
                    case UINavigationController navigationController:
                        root = navigationController.VisibleViewController;
                        continue;
                    case UITabBarController uiTabBarController:
                        root = uiTabBarController.SelectedViewController;
                        continue;
                }

                if (root.PresentedViewController == null) return root;
                root = root.PresentedViewController;
            }
        }




    }
}
public class LocationUpdatedEventArgs : EventArgs
{
    CLLocation location;

    public LocationUpdatedEventArgs(CLLocation location)
    {
        this.location = location;
    }

    public CLLocation Location
    {
        get { return location; }
    }
}

