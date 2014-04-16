using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.IO;
using System.Globalization;

namespace MonoTouch
{
    public static class Utilities
    {
        /// <summary>
        ///   A shortcut to the main application
        /// </summary>
        public static UIApplication MainApp = UIApplication.SharedApplication;

        public readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");

        //
        // Since we are a multithreaded application and we could have many
        // different outgoing network connections (api.twitter, images,
        // searches) we need a centralized API to keep the network visibility
        // indicator state
        //
        static readonly object NetworkLock = new object ();
        static int _active;

        public static void PushNetworkActive ()
        {
            lock (NetworkLock){
                _active++;
                MainApp.NetworkActivityIndicatorVisible = true;
            }
        }

        public static void PopNetworkActive ()
        {
            lock (NetworkLock){
                if (_active == 0)
                    return;

                _active--;
                if (_active == 0)
                    MainApp.NetworkActivityIndicatorVisible = false;
            }
        }

        public static NSUserDefaults Defaults = NSUserDefaults.StandardUserDefaults;

//        public static void SetupAnalytics(string trackerId, string appName)
//        {
//            var result = GoogleAnalytics.GAI.SharedInstance;
//            result.GetTracker(trackerId);
//            result.TrackUncaughtExceptions = true;
//            result.DefaultTracker.AppName = appName;
//            result.DispatchInterval = 30;
//            result.DefaultTracker.AppVersion = NSBundle.MainBundle.InfoDictionary[new NSString("CFBundleVersion")].ToString();
//        }
//
//        public static bool AnalyticsEnabled
//        {
//            get { return !GoogleAnalytics.GAI.SharedInstance.OptOut; }
//            set { GoogleAnalytics.GAI.SharedInstance.OptOut = !value; }
//        }
//
//        public static GAITracker Analytics
//        {
//            get { return GoogleAnalytics.GAI.SharedInstance.DefaultTracker; }
//        }

        public static void LogException (string text, Exception e)
        {
            Console.WriteLine (String.Format ("On {0}, message: {1}\nException:\n{2}", DateTime.Now, text, e));
			//Analytics.TrackException(false, e.Message + " - " + e.StackTrace);
        }

        public static void LogException (Exception e)
        {
            Console.WriteLine (String.Format ("On {0} Exception:\n{1}", DateTime.Now, e));
			//Analytics.TrackException(false, e.Message + " - " + e.StackTrace);
        }

        static UIActionSheet _sheet;
        public static UIActionSheet GetSheet (string title)
        {
            _sheet = new UIActionSheet (title);
            return _sheet;
        }

        static CultureInfo _americanCulture;
        public static CultureInfo AmericanCulture {
            get { return _americanCulture ?? (_americanCulture = new CultureInfo("en-US")); }
        }


        public static void ShowAlert(string title, string message, Action dismissed = null)
        {
            var alert = new UIAlertView {Title = title, Message = message};
            alert.DismissWithClickedButtonIndex(alert.AddButton("Ok".t()), true);
            if (dismissed != null)
                alert.Dismissed += (sender, e) => dismissed();
            alert.Show();
        }


        public static bool IsTall
        {
            get 
            { 
                return UIDevice.CurrentDevice.UserInterfaceIdiom 
                    == UIUserInterfaceIdiom.Phone 
                        && UIScreen.MainScreen.Bounds.Height 
                        * UIScreen.MainScreen.Scale >= 1136;
            }     
        }

        public static Tuple<int, int> iOSVersion
        {
            get
            {
                try
                {
                    var version = UIDevice.CurrentDevice.SystemVersion.Split('.');
                    var major = Int32.Parse(version[0]);
                    var minor = Int32.Parse(version[1]);
                    return new Tuple<int, int>(major, minor);
                }
                catch (Exception e)
                {
                    LogException("Unable to get iOS version", e);
                    return new Tuple<int, int>(5, 0);
                }
            }
        }

        public static UIColor CreateRepeatingBackground()
        {
            UIImage bgImage = null;
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
            {
                bgImage = UIImageHelper.FromFileAuto(MonoTouch.Utilities.IsTall ? "Default-568h" : "Default");
            }
            else
            {
                if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.Portrait || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                    bgImage = UIImageHelper.FromFileAuto("Default-Portrait");
                else
                    bgImage = UIImageHelper.FromFileAuto("Default-Landscape");
            }

            if (bgImage == null)
                return null;

            var size = new System.Drawing.SizeF(40f, bgImage.Size.Height);
            UIGraphics.BeginImageContext(size);
            var ctx = UIGraphics.GetCurrentContext();
            ctx.TranslateCTM (0, bgImage.Size.Height);
            ctx.ScaleCTM (1f, -1f);

            ctx.DrawImage(new System.Drawing.RectangleF(-10, 0, bgImage.Size.Width, bgImage.Size.Height), bgImage.CGImage);
            ctx.ClipToRect(new System.Drawing.RectangleF(0, 0, size.Width, size.Height));

            var img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            bgImage.Dispose();
            bgImage = null;

            if (img == null)
                return null;

            var ret = UIColor.FromPatternImage(img);
            img.Dispose();
            return ret;
        }
    }
}

