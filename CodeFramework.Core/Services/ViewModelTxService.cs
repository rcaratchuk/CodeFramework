using System;
using Cirrious.CrossCore;

namespace CodeFramework.Core.Services
{
	public class ViewModelTxService : IViewModelTxService
    {
		private object _data;

		public void Add(object obj)
		{
			_data = obj;
		}

		public object Get()
		{
			return _data;
		}
    }
}

