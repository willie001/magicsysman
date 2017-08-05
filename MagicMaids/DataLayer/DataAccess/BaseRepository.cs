#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using MagicMaids.EntityModels;

using NLog;
#endregion

namespace MagicMaids.DataAccess 
{
	//public class BaseRepository<T> : IRepository<T> where T : class, IDataModel, new()
	//{
	//	#region Fields
	//	private bool _disposed;
 //       private readonly IUnitOfWork _unitOfWork;
 //       #endregion

 //       #region Constructors
 //       public BaseRepository(IUnitOfWork unitOfWork)
	//	{
 //           _unitOfWork = unitOfWork;
	//	}

	//	protected virtual void Dispose(bool disposing)
	//	{
	//		if (!_disposed)
	//		{
	//			if (disposing)
	//			{
	//				Context.Dispose();
	//			}
	//		}
	//		_disposed = true;
	//	}

	//	public void Dispose()
	//	{
	//		Dispose(true);
	//		GC.SuppressFinalize(this);
	//	}
	//	#endregion

	//	#region Properties, Protected
	//	internal Logger Logging { get;  set; }
	//	protected  IUnitOfWork UnitOfWork { get;  set; }
	//	#endregion

	//	#region Methods, Public
	//	public virtual List<T> GetAll(bool includeDisabled)
	//	{
	//		//try
	//	//    {
	//	//        return !Context.ChangeTracker.HasChanges() || Context.SaveChanges() > 0; // only return true if at least one row was changed
	//	//    }
	//	//    catch (Exception ex)
	//	//    {
	//	//        Logging.Log(LogLevel.Error, ex, $"Error from {nameof(SaveAll)}");
	//	//        return false;
	//	//    }
	//		throw new NotImplementedException();
	//	}

	//	public virtual T GetById(Guid guid)
	//	{
	//		//return dbSet.Find(guid);
	//		throw new NotImplementedException();
	//	}

	//	//public virtual IEnumerable<TEntity> Get(
	//	//	Expression<Func<TEntity, bool>> filter = null,
	//	//	Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
	//	//	string includeProperties = "")
	//	//{
	//	//	IQueryable<TEntity> query = dbSet;

	//	//	if (filter != null)
	//	//	{
	//	//		query = query.Where(filter);
	//	//	}

	//	//	foreach (var includeProperty in includeProperties.Split
	//	//		(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
	//	//	{
	//	//		query = query.Include(includeProperty);
	//	//	}

	//	//	if (orderBy != null)
	//	//	{
	//	//		return orderBy(query).ToList();
	//	//	}
	//	//	else
	//	//	{
	//	//		return query.ToList();
	//	//	}
	//	//}

	//	public virtual void Insert(T entity)
	//	{
	//	//	dbSet.Add(entity);
	//		throw new NotImplementedException();
	//	}

	//	public virtual void Update(T entityToUpdate)
	//	{
	//	//	dbSet.Attach(entityToUpdate);
	//	//	context.Entry(entityToUpdate).State = EntityState.Modified;
	//		throw new NotImplementedException();
	//	}
			

	//	public virtual void Delete(Guid guid)
	//	{
	//		//T entityToDelete = dbSet.Find(id);
	//		//Delete(entityToDelete);
	//		throw new NotImplementedException();
	//	}

	//	public virtual void Delete(T entityToDelete)
	//	{
	//		//if (context.Entry(entityToDelete).State == EntityState.Detached)
	//		//{
	//		//	dbSet.Attach(entityToDelete);
	//		//}
	//		//dbSet.Remove(entityToDelete);
	//		throw new NotImplementedException();
	//	}

	//	#endregion


	//}
}
