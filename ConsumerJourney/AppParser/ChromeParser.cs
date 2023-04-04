using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views.Accessibility;
using Android.Webkit;
using ConsumerJourney.Entities;

namespace ConsumerJourney.AppParser
{
    public class ChromeParser
    {
        private const string CHROME_PACKAGE_NAME = "com.android.chrome";
        private const string CHROME_URL_NODE = "com.android.chrome:id/url_bar";
        public void GetChromeData(AccessibilityEvent e, Services.MyAccessibilityService myAccessibilityService)
        {
            if (e.PackageName == CHROME_PACKAGE_NAME)
            {
                var findAccessibilityNodeInfosByViewId =
                e.Source.FindAccessibilityNodeInfosByViewId(CHROME_URL_NODE);

                WebView wv = new WebView(myAccessibilityService);

                CustomWebViewClient customWebViewClient = new CustomWebViewClient();
                customWebViewClient.OnPageLoaded += CustomWebViewClient_OnPageLoaded;

                


                if (findAccessibilityNodeInfosByViewId != null && findAccessibilityNodeInfosByViewId.Any())
                {
                    AccessibilityNodeInfo node = findAccessibilityNodeInfosByViewId[0];

                    var currentActiveApp = MainActivity.appRecords.FirstOrDefault(x => x.IsCurrentApp);
                    if (currentActiveApp != null && currentActiveApp.PackageName == CHROME_PACKAGE_NAME)
                    {
                        GetPageTitle(wv, customWebViewClient, node);

                        var appData = new AppData
                        {
                            Url = node.Text,
                            SearchText = GetSearchQuery(node.Text)
                        };
                        if (currentActiveApp.Data == null)
                            currentActiveApp.Data = new List<AppData>();

                        if (currentActiveApp.Data.Count() > 1)
                        {
                            if (currentActiveApp.Data[currentActiveApp.Data.Count - 1].Url != appData.Url)
                                currentActiveApp.Data.Add(appData);
                        }
                        else
                            currentActiveApp.Data.Add(appData);
                    }
                }


            }
        }

        private static void GetPageTitle(WebView wv, CustomWebViewClient customWebViewClient, AccessibilityNodeInfo node)
        {
            var urlText = node.Text;
            if (!urlText.StartsWith("http"))
                urlText = "https://" + urlText;
            wv.SetWebViewClient(customWebViewClient);
            wv.LoadUrl(urlText);
        }

        private void CustomWebViewClient_OnPageLoaded(object sender, string e)
        {
            var title = e;
        }

        private string GetSearchQuery(string urlText)
        {
            string searchText = "";
            try
            {
                if (!urlText.StartsWith("http"))
                    urlText = "https://" + urlText;

                Uri url = new Uri(urlText);

                // Get the query parameter as a string
                string query = url.Query;

                // Remove the leading "?" character from the query string
                if (query.StartsWith("?"))
                {
                    query = query.Substring(1);
                }

                // Split the query string into individual parameters
                string[] parameters = query.Split('&');

                // Loop through each parameter to find the one you are interested in
                foreach (string parameter in parameters)
                {
                    // Split the parameter into its name and value components
                    string[] parts = parameter.Split('=');
                    string name = parts[0];
                    string value = parts.Length > 1 ? parts[1] : "";

                    // Check if this is the parameter you are looking for
                    if (name == "q")
                    {
                        // This is the search query parameter!
                        // The value of the parameter contains the search text
                        searchText = Uri.UnescapeDataString(value);
                        // Do whatever you need to do with the search text here...
                        break;
                    }
                }
            }
            catch { }
            return searchText;
        }
    }

    public class CustomWebViewClient : WebViewClient
    {

        public event EventHandler<string> OnPageLoaded;

        public override void OnPageFinished(WebView view, string url)
        {
            OnPageLoaded?.Invoke(this, view.Title);
        }

    }
}