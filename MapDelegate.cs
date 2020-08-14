using System;
using CoreLocation;
using UIKit;
using MapKit;
using Foundation;
using CoreGraphics;
using AVFoundation;

namespace AR.Speedometer
{
    public class MapDelegate : MKMapViewDelegate
    {
        public MapDelegate() { }
        public override MKOverlayView GetViewForOverlay(MKMapView mapView, IMKOverlay overlay)
        {
            MKPolyline polyline = overlay as MKPolyline;
            try
            {
                MKPolylineView polylineView = new MKPolylineView(polyline);

                polylineView.StrokeColor = UIColor.Black;
                polylineView.LineWidth = 8;
                return polylineView;
            }
            catch { return null; }

        }
    }
}
