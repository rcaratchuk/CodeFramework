using Cirrious.MvvmCross.Touch.Views;
using CodeFramework.iOS.Utils;
using CodeFramework.Core.ViewModels;
using MonoTouch.UIKit;

namespace CodeFramework.iOS.ViewControllers
{
    public class ViewModelDrivenViewController : MvxViewController
    {
        private Hud _hud;

        public override void ViewDidLoad()
        {
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.BackButton, UIBarButtonItemStyle.Plain, (s, e) => NavigationController.PopViewControllerAnimated(true));

            base.ViewDidLoad();

            _hud = new Hud(View);

            var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {

                loadableViewModel.Bind(x => x.IsLoading, x =>
                {
                    if (x)
                    {
                        MonoTouch.Utilities.PushNetworkActive();
                        _hud.Show("Loading...");

                        if (ToolbarItems != null)
                        {
                            foreach (var t in ToolbarItems)
                                t.Enabled = false;
                        }
                    }
                    else
                    {
                        MonoTouch.Utilities.PopNetworkActive();
                        _hud.Hide();

                        if (ToolbarItems != null)
                        {
                            foreach (var t in ToolbarItems)
                                t.Enabled = true;
                        }
                    }
                });
            }
        }

        bool _isLoaded = false;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!_isLoaded)
            {
                var loadableViewModel = ViewModel as LoadableViewModel;
                if (loadableViewModel != null)
                    loadableViewModel.LoadCommand.Execute(false);
                _isLoaded = true;
            }
        }
    }
}

