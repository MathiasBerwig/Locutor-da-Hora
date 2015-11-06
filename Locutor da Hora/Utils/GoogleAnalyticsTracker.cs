using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Locutor_da_Hora.Utils
{
    /// <summary>
    /// Utiliza o Google Measurement Protocol para registrar dados anônimos de uso do aplicativo.
    /// </summary>
    /// <remarks>
    /// Inspirado no artigo Google Analytics Measurement Protocol – Track Events C# (http://www.spyriadis.net/2014/07/google-analytics-measurement-protocol-track-events-c/).
    /// </remarks>
    class GoogleAnalyticsTracker
    {
        #region Membros Privados                
        private const string googleUrl = "http://www.google-analytics.com/collect";
        private const string googleUrlDebug = "https://www.google-analytics.com/debug/collect";
        private const string GoogleVersion = "1";
        private const string GoogleTrackingId = "UA-62310092-2";
        private const string ApplicationName = "Locutor da Hora";
        private const string ApplicationVersion = "1.0";
        private const string ApplicationId = "br.edu.unijui.locutordahora";
        private readonly string SystemResolution;
        private readonly string SystemCulture;
        private readonly string ScreenBitDepth;
        private readonly string GoogleClientId;
        private static GoogleAnalyticsTracker instance;
        #endregion

        #region Construtores Privados
        private GoogleAnalyticsTracker()
        {
            SystemResolution = SystemInfo.PrimaryScreenWidth + "x" + SystemInfo.PrimaryScreenHeight;
            ScreenBitDepth = SystemInfo.ScreenBitDepth;
            SystemCulture = SystemInfo.CurrentCulture;
            GoogleClientId = SystemInfo.UniqueID;
        }
        #endregion

        #region Membros Públicos
        public static GoogleAnalyticsTracker Instance => instance ?? (instance = new GoogleAnalyticsTracker());
        #endregion

        #region Métodos Privados
        private Hashtable BaseValues()
        {
            Hashtable ht = new Hashtable
            {
                {"v", GoogleVersion},       // Protocol Version.
                {"tid", GoogleTrackingId},  // Tracking ID / Web property / Property ID.
                {"cid", GoogleClientId},    // Anonymous Client ID.
                {"an", ApplicationName},    // Application Name.
                {"aid", ApplicationId},     // Application ID.
                {"av", ApplicationVersion}, // Application Version.
                {"sr", SystemResolution},   // System Resolution.
                {"sd" , ScreenBitDepth},    // Screen Colors.
                {"ul", SystemCulture }      // User Language.
            };

            return ht;
        }

        private void PostData(Hashtable values)
        {
            ServicePointManager.Expect100Continue = false;

            var data = "";
            foreach (var key in values.Keys)
            {
                if (data != "") data += "&";
                if (values[key] != null) data += key + "=" + HttpUtility.UrlEncode(values[key].ToString());
            }

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers["User-Agent"] = "user_agent_string";
                    var url = App.DebugMode ? googleUrlDebug : googleUrl;
                    var response = client.UploadString(url, "POST", data);

                    //if (App.DebugMode)
                    {
                        System.Diagnostics.Debug.WriteLine(url + "?" + data);
                        System.Diagnostics.Debug.WriteLine(response);
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }
        }
        #endregion

        #region Métodos Públicos        
        public void TrackEvent(string category, string action, string label, int? value)
        {
            Hashtable ht = BaseValues();

            ht.Add("t", "event");                   // Event hit type
            ht.Add("ec", category);                 // Event Category. Required.
            ht.Add("ea", action);                   // Event Action. Required.
            if (label != null) ht.Add("el", label); // Event label.
            if (value != null) ht.Add("ev", value); // Event value.

            Task.Factory.StartNew(() => { PostData(ht); });
        }

        public void TrackTime(string category, string action, long time, string label)
        {

            Hashtable ht = BaseValues();

            ht.Add("t", "timing");                  // Timing hit type.
            ht.Add("utc", category);                // Timing category.
            ht.Add("utv", action);                  // Timing variable.
            ht.Add("utt", time);                    // Timing time.
            ht.Add("utl", label);                   // Timing label.

            Task.Factory.StartNew(() => { PostData(ht); });
        }

        public void TrackScreen(string screenName)
        {
            Hashtable ht = BaseValues();

            ht.Add("t", "screenview");  // Screenview hit type.
            ht.Add("cd", screenName);   // Document screenName. Required.

            Task.Factory.StartNew(() => { PostData(ht); });
        }

        public void EcommerceTransaction(string id, string affiliation, string revenue, string shipping, string tax, string currency)
        {
            Hashtable ht = BaseValues();

            ht.Add("t", "transaction");       // Transaction hit type.
            ht.Add("ti", id);                 // transaction ID.            Required.
            ht.Add("ta", affiliation);        // Transaction affiliation.
            ht.Add("tr", revenue);            // Transaction revenue.
            ht.Add("ts", shipping);           // Transaction shipping.
            ht.Add("tt", tax);                // Transaction tax.
            ht.Add("cu", currency);           // Currency code.

            Task.Factory.StartNew(() => { PostData(ht); });
        }

        public void EcommerceItem(string id, string name, string price, string quantity, string code, string category, string currency)
        {
            Hashtable ht = BaseValues();

            ht.Add("t", "item");              // Item hit type.
            ht.Add("ti", id);                 // transaction ID.            Required.
            ht.Add("in", name);               // Item name.                 Required.
            ht.Add("ip", price);              // Item price.
            ht.Add("iq", quantity);           // Item quantity.
            ht.Add("ic", code);               // Item code / SKU.
            ht.Add("iv", category);           // Item variation / category.
            ht.Add("cu", currency);           // Currency code.

            Task.Factory.StartNew(() => { PostData(ht); });
        }

        public void TrackSocial(string action, string network, string target)
        {
            Hashtable ht = BaseValues();

            ht.Add("t", "social");                // Social hit type.
            ht.Add("dh", action);                 // Social Action.         Required.
            ht.Add("dp", network);                // Social Network.        Required.
            ht.Add("dt", target);                 // Social Target.         Required.

            Task.Factory.StartNew(() => { PostData(ht); });
        }

        public void TrackException(string description, bool fatal)
        {
            Hashtable ht = BaseValues();

            ht.Add("t", "exception");             // Exception hit type.
            ht.Add("dh", description);            // Exception description.         Required.
            ht.Add("dp", fatal ? "1" : "0");      // Exception is fatal?            Required.

            Task.Factory.StartNew(() => { PostData(ht); });
        }
        #endregion        
    }
}
