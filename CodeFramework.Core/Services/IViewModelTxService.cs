using System;

namespace CodeFramework.Core.Services
{
    public interface IViewModelTxService
    {
		void Add(object obj);

		object Get();
    }
}
