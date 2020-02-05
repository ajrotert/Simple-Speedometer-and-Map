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
            // return a view for the polygon
            /*MKPolygon polygon = overlay as MKPolygon;
            MKPolygonView polygonView = new MKPolygonView(polygon);
            polygonView.FillColor = UIColor.Clear;
            polygonView.StrokeColor = color;
            return polygonView;*/
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
