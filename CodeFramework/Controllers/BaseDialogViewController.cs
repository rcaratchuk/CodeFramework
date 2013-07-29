using CodeFramework.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Drawing;

namespace CodeFramework.Controllers
{
    public class BaseDialogViewController : DialogViewController
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (NavigationController != null && IsSearching)
                NavigationController.SetNavigationBarHidden(true, true);
            if (IsSearching)
            {
                //This needs to be in the begin invoke because there's logic in the base class that
                //moves the scroll around. So by doing this we move this logic to execute after it.
                BeginInvokeOnMainThread(() => {
                    TableView.ScrollRectToVisible(new RectangleF(0, 0, 1, 1), false);
                });
            }
        }
        
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (NavigationController != null && NavigationController.NavigationBarHidden)
                NavigationController.SetNavigationBarHidden(false, true);
            
            if (IsSearching)
            {
                View.EndEditing(true);
                var searchBar = TableView.TableHeaderView as UISearchBar;
                if (searchBar != null)
                {
                    //Enable the cancel button again....
                    foreach (var s in searchBar.Subviews)
                    {
                        var x = s as UIButton;
                        if (x != null)
                            x.Enabled = true;
                    }
                }
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseDialogViewController"/> class.
		/// </summary>
		/// <param name="push">If set to <c>true</c> push.</param>
        public BaseDialogViewController(bool push)
			: this(push, "Back")
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseDialogViewController"/> class.
		/// </summary>
		/// <param name="push">If set to <c>true</c> push.</param>
		/// <param name="backButtonText">Back button text.</param>
		public BaseDialogViewController(bool push, string backButtonText)
			: base(new RootElement(""), push)
		{
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Images.Buttons.Back, () => NavigationController.PopViewControllerAnimated(true)));
			SearchPlaceholder = "Search";
			Autorotate = true;
            AutoHideSearch = true;
		}
        
        /// <summary>
        /// Makes the refresh table header view.
        /// </summary>
        /// <returns>
        /// The refresh table header view.
        /// </returns>
        /// <param name='rect'>
        /// Rect.
        /// </param>
        public override RefreshTableHeaderView MakeRefreshTableHeaderView(RectangleF rect)
        {
            //Replace it with our own
            return new RefreshView(rect);
        }
        
        public override void ViewDidLoad()
        {
            if (Title != null && Root != null)
                Root.Caption = Title;

            TableView.BackgroundColor = UIColor.Clear;
            TableView.BackgroundView = null;
            if (Style != UITableViewStyle.Grouped)
            {
                TableView.TableFooterView = new DropbarView(View.Bounds.Width) {Hidden = true};
            }

            var backgroundView = new UIView { BackgroundColor = UIColor.FromPatternImage(Images.Views.Background) };
            backgroundView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            this.TableView.BackgroundView = backgroundView;
            base.ViewDidLoad();
        }

        sealed class RefreshView : RefreshTableHeaderView
        {
            public RefreshView(RectangleF rect)
                : base(rect)
            {
                BackgroundColor = UIColor.Clear;
                StatusLabel.BackgroundColor = UIColor.Clear;
                LastUpdateLabel.BackgroundColor = UIColor.Clear;
            }
        }
    }
}

