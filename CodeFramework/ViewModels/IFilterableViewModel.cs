using System;
using CodeFramework.Filters.Models;

namespace CodeFramework.ViewModels
{
    public interface IFilterableViewModel<TFilter> where TFilter : FilterModel<TFilter>, new()
    {
        TFilter Filter { get; }

        void ApplyFilter(TFilter filter, bool saveAsDefault = false);
    }
}
