using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.AccessibilityServices;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Views.Accessibility;
using AndroidX.Core.Content;
using ConsumerJourney.AppParser;
using ConsumerJourney.Entities;
using Xamarin.Essentials;

namespace ConsumerJourney.Services
{
    [Service(Permission = Android.Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
    public class MyAccessibilityService : AccessibilityService
    {
        private const string SYSTEM_PACKAGE_NAME = "com.android.systemui";
        private const string SAMSUNG_PACKAGE_NAME = "com.samsung";
        private const string LAUNCHER_PACKAGE_NAME = "com.sec.android.app.launcher";
        private const string CONSUMER_JOURNEY_PACKAGE_NAME = "com.companyname.consumerjourney";
        private ChromeParser _chromeParser;
        public MyAccessibilityService()
        {
            _chromeParser = new ChromeParser();
        }

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            try
            {
                Console.WriteLine("PackageName: " + e.PackageName);
                Console.WriteLine("EventType: " + e.EventType);
                Console.WriteLine("Source: " + e.Source);
                Console.WriteLine("WindowId: " + e.WindowId);

                Console.WriteLine("ContentDescription: " + e.ContentDescription);

                var Text = e.Text;

                switch (e.EventType)
                {
                    case EventTypes.ViewClicked:
                        //GetAppDetailsFromHomeScreen(e);
                        break;
                    case EventTypes.WindowStateChanged:
                        _ = WindowStateChanged(e);
                        break;
                    case EventTypes.WindowContentChanged:
                        WindowContentChanged(e);
                        break;
                }

            }
            catch (Exception)
            {

            }
        }

        private void GetAppDetailsFromHomeScreen(AccessibilityEvent e)
        {
            if (e.PackageName == "com.sec.android.app.launcher")
            {
                var contentDescription = e.ContentDescription.Split(',')[0];
                MainActivity.appRecords.Add(new Entities.AppRecord
                {
                    AppName = contentDescription,
                    AppStartTime = DateTime.Now,
                });
            }
            //var appName = GetAppName(e.PackageName);
        }

        private void WindowContentChanged(AccessibilityEvent e)
        {
            if (e.PackageName == SYSTEM_PACKAGE_NAME ||
                e.PackageName == CONSUMER_JOURNEY_PACKAGE_NAME ||
                e.PackageName.Contains(SAMSUNG_PACKAGE_NAME))
                return;
            _chromeParser.GetChromeData(e, this);
        }

        private async Task WindowStateChanged(AccessibilityEvent e)
        {
            if (e.PackageName == SYSTEM_PACKAGE_NAME ||
                e.PackageName == CONSUMER_JOURNEY_PACKAGE_NAME ||
                e.PackageName.Contains(SAMSUNG_PACKAGE_NAME))
                return;

            var appName = GetAppName(e.PackageName);
            if (string.IsNullOrEmpty(appName)) return;

            var currentApp = MainActivity.appRecords.FirstOrDefault(x => x.IsCurrentApp);
            if (currentApp != null)
            {
                if (currentApp.PackageName == e.PackageName)
                    return;
                else
                {
                    currentApp.AppEndTime = DateTime.Now;
                    currentApp.IsCurrentApp = false;
                }
            }

            if (e.PackageName != LAUNCHER_PACKAGE_NAME)
            {
                var location = await GetCurrentLocation();
                var lastApp = MainActivity.appRecords != null && MainActivity.appRecords.Any() ?
                    MainActivity.appRecords.Last() : null;
                MainActivity.appRecords.Add(new Entities.AppRecord
                {
                    PackageName = e.PackageName,
                    AppName = appName,
                    AppStartTime = DateTime.Now,
                    IsCurrentApp = true,
                    AppCategory = "",
                    Latitude = location?.Latitude ?? -1,
                    Longitude = location?.Longitude ?? -1,
                    PreviousApp = lastApp?.AppName
                });
            }
        }

        private AccessibilityNodeInfo FindChildNodeById(AccessibilityNodeInfo rootNode, string viewId)
        {
            if (rootNode == null || string.IsNullOrEmpty(viewId))
            {
                return null;
            }

            if (rootNode.ClassName == "android.widget.EditText")
            {
                var text = rootNode.Text;
            }

            for (int i = 0; i < rootNode.ChildCount; i++)
            {
                var childNode = rootNode.GetChild(i);
                if (childNode != null && viewId.Equals(childNode.ViewIdResourceName))
                {
                    return childNode;
                }

                var result = FindChildNodeById(childNode, viewId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private async Task<Android.Locations.Location> GetCurrentLocation()
        {
            FusedLocationProviderClient fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted && ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                // Request location permission
            }
            else
            {
                // Permission already granted, get the location
            }
            Task<Android.Locations.Location> locationTask = fusedLocationProviderClient.GetLastLocationAsync();
            var location = await locationTask;
            return location;
        }

        public string GetAppName(string packageName)
        {
            PackageManager pm = PackageManager;
            string appName = "";
            try
            {
                ApplicationInfo appInfo = pm.GetApplicationInfo(packageName, 0);
                appName = pm.GetApplicationLabel(appInfo).ToString();
            }
            catch (PackageManager.NameNotFoundException e)
            {
                // App is not installed
            }
            return appName;
        }

        public override void OnInterrupt()
        {
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
        }
    }
}