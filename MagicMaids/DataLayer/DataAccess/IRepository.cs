#region Using
using System;
using System.Collections.Generic;

using MagicMaids.EntityModels;
#endregion 

namespace MagicMaids.DataAccess
{
    public interface IRepository<T> : IDisposable //where T: IDataModel, IDisposable
    {
        List<T> GetAll(bool includeDisabled);
		T GetById(Guid guid);
        void Insert(T entity);
		void Update(T entityToUpdate);
		void Delete(Guid guid);
		void Delete(T entityToDelete);
    }
    
}
