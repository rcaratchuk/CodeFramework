using System;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeFramework.iOS.Views;
using CodeFramework.ViewControllers;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.CrossCore;
using CodeFramework.Core.Messages;

namespace CodeFramework.iOS.ViewControllers
{
	public abstract class ViewModelDrivenDialogViewController : BaseDialogViewController, IMvxTouchView, IMvxEventSourceViewController
    {
		private UIRefreshControl _refreshControl;
		private bool _manualRefresh;
		private MvxSubscriptionToken _errorToken;
		
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			ViewDidLoadCalled.Raise(this);

			var loadableViewModel = ViewModel as LoadableViewModel;
			if (loadableViewModel != null)
			{
				_refreshControl = new UIRefreshControl();
				RefreshControl = _refreshControl;
				_refreshControl.ValueChanged += HandleRefreshRequested;
				loadableViewModel.Bind(x => x.IsLoading, x =>
				{
						if (x)
						{
							MonoTouch.Utilities.PushNetworkActive();
							_refreshControl.BeginRefreshing();
							TableView.SetContentOffset(new System.Drawing.PointF(0, -TableView.ContentInset.Top), true);

							if (ToolbarItems != null)
							{
								foreach (var t in ToolbarItems)
									t.Enabled = false;
							}
						}
						else
						{
							MonoTouch.Utilities.PopNetworkActive();
							_refreshControl.EndRefreshing();

							if (_manualRefresh)
								TableView.SetContentOffset(new System.Drawing.PointF(0, 0), true);
							_manualRefresh = false;

							if (ToolbarItems != null)
							{
								foreach (var t in ToolbarItems)
									t.Enabled = true;
							}

//							var hideSearch = EnableSearch && AutoHideSearch;
//							var newY = hideSearch ? 44 : 0;
//							_refreshControl.EndRefreshing();
//							if (TableView.ContentOffset.Y != newY)
//							{
//							}
						}
				});
			}
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name='push'>True if navigation controller should push, false if otherwise</param>
        /// <param name='refresh'>True if the data can be refreshed, false if otherwise</param>
		protected ViewModelDrivenDialogViewController(bool push = true)
            : base(push)
        {
			this.AdaptForBinding();
        }

        private void HandleRefreshRequested(object sender, EventArgs e)
        {
			var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {
				_manualRefresh = true;
                loadableViewModel.LoadCommand.Execute(true);
            }
        }

		bool _isLoaded = false;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
			ViewWillAppearCalled.Raise(this, animated);
			_errorToken = Mvx.Resolve<IMvxMessenger>().SubscribeOnMainThread<ErrorMessage>(OnErrorMessage);

			if (!_isLoaded)
			{
				var loadableViewModel = ViewModel as LoadableViewModel;
				if (loadableViewModel != null)
					loadableViewModel.LoadCommand.Execute(false);
				_isLoaded = true;
			}
        }

		private void OnErrorMessage(ErrorMessage msg)
		{
			if (msg.Sender != ViewModel)
				return;
			MonoTouch.Utilities.ShowAlert("Error", msg.Error.Message);
		}

		public override float GetHeightForFooter(MonoTouch.UIKit.UITableView tableView, int section)
		{
			if (tableView.Style == MonoTouch.UIKit.UITableViewStyle.Grouped)
				return 2;
			return base.GetHeightForFooter(tableView, section);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			ViewWillDisappearCalled.Raise(this, animated);

			if (_errorToken != null)
			{
				_errorToken.Dispose();
				_errorToken = null;
			}
		}

		public object DataContext
		{
			get { return BindingContext.DataContext; }
			set { BindingContext.DataContext = value; }
		}

		public IMvxViewModel ViewModel
		{
			get { return DataContext as IMvxViewModel;  }
			set { DataContext = value; }
		}

		public IMvxBindingContext BindingContext { get; set; }

		public MvxViewModelRequest Request { get; set; }

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			ViewDidDisappearCalled.Raise(this, animated);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			ViewDidAppearCalled.Raise(this, animated);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeCalled.Raise(this);
			}
			base.Dispose(disposing);
		}

		public event EventHandler DisposeCalled;
		public event EventHandler ViewDidLoadCalled;
		public event EventHandler<MvxValueEventArgs<bool>> ViewWillAppearCalled;
		public event EventHandler<MvxValueEventArgs<bool>> ViewDidAppearCalled;
		public event EventHandler<MvxValueEventArgs<bool>> ViewDidDisappearCalled;
		public event EventHandler<MvxValueEventArgs<bool>> ViewWillDisappearCalled;
    }
}

