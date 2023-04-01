using System;
using Android.AccessibilityServices;
using Android.App;
using Android.Content.PM;
using Android.Views.Accessibility;

namespace ConsumerJourney.Services
{
    [Service(Permission = Android.Manifest.Permission.BindAccessibilityService)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/accessibility_service_config")]
    public class MyAccessibilityService : AccessibilityService
    {
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

                switch(e.EventType)
                {
                    case EventTypes.ViewClicked:
                        //GetAppName(e);
                        break;
                    case EventTypes.WindowStateChanged:
                        GetAppDetails(e);
                        break;
                }

            }
            catch (Exception)
            {

            }
        }

        private void GetAppDetails(AccessibilityEvent e)
        {
            /*if(e.PackageName == "com.sec.android.app.launcher")
            {
                var contentDescription = e.ContentDescription.Split(',')[0];
                MainActivity.appRecords.Add(new Entities.ConsumerAppRecord
                {
                    AppName = contentDescription,
                    AppStartTime = DateTime.Now,
                });
            }*/
            var appName = GetAppName(e.PackageName);
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

        public void performViewClick(AccessibilityNodeInfo nodeInfo)
        {
            if (nodeInfo == null)
            {
                return;
            }
            while (nodeInfo != null)
            {
                if (nodeInfo.Clickable)
                {
                    nodeInfo.PerformAction(Android.Views.Accessibility.Action.Click);
                    break;
                }
                nodeInfo = nodeInfo.Parent;
            }
        }

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
        }
    }
}