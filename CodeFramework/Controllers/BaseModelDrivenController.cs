using System;
using CodeFramework.Elements;
using CodeFramework.Views;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch;
using CodeFramework.Filters.Controllers;
using CodeFramework.Filters.Models;

namespace CodeFramework.Controllers
{
    /// <summary>
    /// I screwed up big time. The Controller class used generics to store the Model. However, since the last MonoTouch
    /// update, generic classes that inheirt from NSObject caused the app to crash so I needed to remake it. Unfortunately,
    /// this was at a time where the app I had submitted to the app store got rejected so I needed to whip up a temp class
    /// to make everything work reasonably well so this is basically a dupe of Controller.cs
    /// </summary>
    public abstract class BaseModelDrivenController : BaseDialogViewController
    {
        protected ErrorView CurrentError;
        private bool _firstSeen;
        private bool _enableFilter;
        private FilterModel _filterModel;

        public bool EnableFilter
        {
            get { return _enableFilter; }
            set 
            {
                if (value == true)
                    EnableSearch = true;
                _enableFilter = value;
            }
        }

        // This must be a generic type because I can't template this controller
        protected object Model { get; set; }

        public bool Loaded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name='push'>True if navigation controller should push, false if otherwise</param>
        /// <param name='refresh'>True if the data can be refreshed, false if otherwise</param>
        protected BaseModelDrivenController(bool push = true, bool refresh = true)
            : base(push)
        {
            if (refresh)
                RefreshRequested += (sender, e) => UpdateAndRender(false);
        }

        //Called when the UI needs to be updated with the model data            
        protected abstract void OnRender();

        //Called when the controller needs to request the model from the server
        protected abstract object OnUpdateModel(bool forced);

        protected T GetFilterModel<T>() where T : FilterModel, new()
        {
            if (_filterModel == null)
                return default(T);
            return (T)_filterModel;
        }

        protected void SetFilterModel<T>(T model) where T : FilterModel, new()
        {
            if (model == null)
                model = default(T);
            _filterModel = model;
        }

        /// <summary>
        /// Code called to render the model
        /// </summary>
        public void Render()
        {
            try
            {
                OnRender();
            }
            catch (Exception ex)
            {
                MonoTouch.Utilities.LogException("Error when refreshing view", ex);
            }

            if (TableView.TableFooterView != null)
                TableView.TableFooterView.Hidden = Root.Count == 0;
        }

        public void UpdateAndRender(bool showLoading = true)
        {
            if (CurrentError != null)
                CurrentError.RemoveFromSuperview();
            CurrentError = null;

            if (!showLoading)
            {
                this.DoWorkNoHud(() => {
                    Model = OnUpdateModel(true);
                    InvokeOnMainThread(Render);
                }, ex => Utilities.ShowAlert("Unable to refresh!".t(), "There was an issue while attempting to refresh. ".t() + ex.Message), ReloadComplete);
            }
            else
            {
                this.DoWork(() => {
                    Model = OnUpdateModel(false);
                    InvokeOnMainThread(Render);
                }, ex => {
                    CurrentError = ErrorView.Show(View.Superview, ex.Message);
                }, ReloadComplete);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            //We only want to run this code once, when teh view is first seen...
            if (!_firstSeen)
            {
                //Check if the model is pre-loaded
                if (Model != null)
                {
                    Render();
                    ReloadComplete(); 
                }
                else
                {
                    UpdateAndRender();
                }

                _firstSeen = true;
            }
        }

        protected override void SearchStart()
        {
            base.StartSearch();

            var searchBar = SearchBar as SearchFilterBar;
            if (searchBar != null)
                searchBar.FilterButtonVisible = false;
        }

        protected override void SearchEnd()
        {
            var searchBar = SearchBar as SearchFilterBar;
            if (searchBar != null)
                searchBar.FilterButtonVisible = true;
        }

        protected override UISearchBar CreateSearchBar()
        {
            if (EnableFilter)
            {
                var searchBar = new SearchFilterBar { Delegate = new CustomSearchDelegate(this) };
                searchBar.FilterButton.TouchUpInside += FilterButtonTouched;
                return searchBar;
            }
            else
            {
                return base.CreateSearchBar();
            }
        }

        void FilterButtonTouched (object sender, EventArgs e)
        {
            var filter = CreateFilterController();
            if (filter != null)
                ShowFilterController(filter);
        }

        protected virtual FilterController CreateFilterController()
        {
            return null;
        }

        protected virtual void ApplyFilter(FilterModel model)
        {
            Render();
        }

        protected virtual void SaveFilterAsDefault(FilterModel model)
        {
        }

        private void ShowFilterController(FilterController filter)
        {
            filter.SetCurrentFilterModel(_filterModel);

            filter.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Images.Buttons.Cancel, () => { 
                filter.DismissViewController(true, null);
            }));
            filter.NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(Images.Buttons.Save, () => {
                filter.DismissViewController(true, null); 
                _filterModel = filter.CreateFilterModel();
                ApplyFilter(_filterModel);
            }));

            filter.SaveFilter = (model) => {
                filter.DismissViewController(true, null); 
                _filterModel = model;
                SaveFilterAsDefault(model);
                ApplyFilter(model);
            };

            var nav = new UINavigationController(filter);
            PresentViewController(nav, true, null);
        }
    }
}

