using System;
using Cirrious.MvvmCross.Touch.Views;
using CodeFramework.iOS.Views;
using MonoTouch.UIKit;
using CodeFramework.Core;
using MonoTouch.Foundation;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.CrossCore;
using CodeFramework.Core.Messages;

namespace CodeFramework.iOS.Views
{
    public class WebView : MvxViewController
    {
        protected UIBarButtonItem BackButton;
        protected UIBarButtonItem RefreshButton;
        protected UIBarButtonItem ForwardButton;
		private MvxSubscriptionToken _errorToken;

        public UIWebView Web { get; private set; }
        private readonly bool _navigationToolbar;

        protected virtual void GoBack()
        {
            Web.GoBack();
        }

        protected virtual void Refresh()
        {
            Web.Reload();
        }

        protected virtual void GoForward()
        {
            Web.GoForward();
        }
         
		public WebView()
			: this(true, true)
        {
        }

		public WebView(bool navigationToolbar, bool showPageAsTitle = false)
        {
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Theme.CurrentTheme.BackButton, () => NavigationController.PopViewControllerAnimated(true)));

            Web = new UIWebView {ScalesPageToFit = true};
            Web.LoadFinished += OnLoadFinished;
            Web.LoadStarted += OnLoadStarted;
            Web.LoadError += OnLoadError;
            Web.ShouldStartLoad = (w, r, n) => ShouldStartLoad(r, n);

			if (showPageAsTitle)
			{
				Web.LoadFinished += (sender, e) =>
				{
					Title = Web.EvaluateJavascript("document.title");
				};
			}

            _navigationToolbar = navigationToolbar;

            if (_navigationToolbar)
            {
                ToolbarItems = new [] { 
                    (BackButton = new UIBarButtonItem(Theme.CurrentTheme.WebBackButton, UIBarButtonItemStyle.Plain, (s, e) => GoBack()) { Enabled = false }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 40f },
                    (ForwardButton = new UIBarButtonItem(Theme.CurrentTheme.WebFowardButton, UIBarButtonItemStyle.Plain, (s, e) => GoForward()) { Enabled = false }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    (RefreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh, (s, e) => Refresh()))
                };

                BackButton.TintColor = Theme.CurrentTheme.WebButtonTint;
                ForwardButton.TintColor = Theme.CurrentTheme.WebButtonTint;
                RefreshButton.TintColor = Theme.CurrentTheme.WebButtonTint;

                BackButton.Enabled = false;
                ForwardButton.Enabled = false;
                RefreshButton.Enabled = false;
            }

			EdgesForExtendedLayout = UIRectEdge.None;
        }

        protected virtual bool ShouldStartLoad (MonoTouch.Foundation.NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            return true;
        }

        protected virtual void OnLoadError (object sender, UIWebErrorArgs e)
        {
            MonoTouch.Utilities.PopNetworkActive();
            if (RefreshButton != null)
                RefreshButton.Enabled = true;
        }

        protected virtual void OnLoadStarted (object sender, EventArgs e)
        {
            MonoTouch.Utilities.PushNetworkActive();
            if (RefreshButton != null)
                RefreshButton.Enabled = false;
        }

        protected virtual void OnLoadFinished(object sender, EventArgs e)
        {
            MonoTouch.Utilities.PopNetworkActive();
            if (BackButton != null)
            {
                BackButton.Enabled = Web.CanGoBack;
                ForwardButton.Enabled = Web.CanGoForward;
                RefreshButton.Enabled = true;
            }
        }
        
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
			_errorToken.Dispose();
			_errorToken = null;
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Add(Web);
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            Web.Frame = View.Bounds;
        }

		protected static string JavaScriptStringEncode(string data)
		{
			return System.Web.HttpUtility.JavaScriptStringEncode(data);
		}

		protected static string UrlDecode(string data)
		{
			return System.Web.HttpUtility.UrlDecode(data);
		}

		protected string LoadFile(string path)
        {
			if (path == null)
				return string.Empty;

            var uri = Uri.EscapeUriString("file://" + path) + "#" + Environment.TickCount;
            InvokeOnMainThread(() => Web.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(uri))));
            return uri;
        }

		protected void LoadContent(string content, string contextPath)
		{
			contextPath = contextPath.Replace("/", "//").Replace(" ", "%20");
			Web.LoadHtmlString(content, NSUrl.FromString("file:/" + contextPath + "//"));
		}
        
        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
            var bounds = View.Bounds;
            if (_navigationToolbar)
                bounds.Height -= NavigationController.Toolbar.Frame.Height;
            Web.Frame = bounds;
			_errorToken = Mvx.Resolve<IMvxMessenger>().SubscribeOnMainThread<ErrorMessage>(OnErrorMessage);
        }

		private void OnErrorMessage(ErrorMessage msg)
		{
			if (msg.Sender != ViewModel)
				return;
			MonoTouch.Utilities.ShowAlert("Error", msg.Error.Message);
		}
        
        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
            Web.Frame = View.Bounds;
        }
    }
}

