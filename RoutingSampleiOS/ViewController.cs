﻿using CoreGraphics;
using System;
using System.IO;
using ThinkGeo.MapSuite;
using ThinkGeo.MapSuite.Drawing;
using ThinkGeo.MapSuite.iOS;
using ThinkGeo.MapSuite.Layers;
using ThinkGeo.MapSuite.Routing;
using ThinkGeo.MapSuite.Shapes;
using ThinkGeo.MapSuite.Styles;
using UIKit;

namespace RoutingSample_iOS
{
    public partial class ViewController : UIViewController
    {
        private MapView mapView;
        private LayerOverlay layerOverlay;
        private RoutingLayer routingLayer;
        private RoutingEngine routingEngine;
        private bool firstClick = true;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            mapView = new MapView(View.Frame);
            View.Add(mapView);

            mapView.MapUnit = GeographyUnit.Meter;
            mapView.MapSingleTap += MapViewMapSingleTap;

            LayerOverlay backgroundOverlay = new LayerOverlay();
            string shapeFilePath = Path.Combine("AppData", "DallasCounty-3857.shp");
            ShapeFileFeatureLayer shapeFileFeatureLayer = new ShapeFileFeatureLayer(shapeFilePath);
            shapeFileFeatureLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle = WorldStreetsLineStyles.MotorwayFill(2.0f);
            shapeFileFeatureLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            backgroundOverlay.Layers.Add(shapeFileFeatureLayer);
            mapView.Overlays.Add(backgroundOverlay);

            layerOverlay = new LayerOverlay();
            routingLayer = new RoutingLayer();
            layerOverlay.Layers.Add(routingLayer);
            mapView.Overlays.Add(layerOverlay);

            string rtgPath = Path.Combine("AppData", "DallasCounty-3857.rtg");
            var routingSource = new RtgRoutingSource(rtgPath);
            var featureSource = new ShapeFileFeatureSource(shapeFilePath);

            routingEngine = new RoutingEngine(routingSource, featureSource);
            routingEngine.GeographyUnit = GeographyUnit.Meter;
            routingEngine.SearchRadiusInMeters = 200;

            mapView.CurrentExtent = new RectangleShape(-10781100.2970769, 3875007.18710502, -10767407.8727504, 3854947.78546675);
            mapView.Refresh();
        }

        private void MapViewMapSingleTap(object sender, UIGestureRecognizer e)
        {
            CGPoint location = e.LocationInView(View);
            PointShape position = ExtentHelper.ToWorldCoordinate(mapView.CurrentExtent, (float)location.X, (float)location.Y, (float)View.Frame.Width, (float)View.Frame.Height);

            routingLayer.Routes.Clear();
            if (firstClick)
            {
                routingLayer.StartPoint = position;
                firstClick = false;
            }
            else
            {
                routingLayer.EndPoint = position;
                firstClick = true;
            }

            routingLayer.Routes.Clear();
            if (routingLayer.StartPoint != null && routingLayer.EndPoint != null)
            {
                RoutingResult routingResult = routingEngine.GetRoute(routingLayer.StartPoint, routingLayer.EndPoint);
                routingLayer.Routes.Add(routingResult.Route);
            }

            layerOverlay.Refresh();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}