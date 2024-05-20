using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task Save();
        Task Insert(TEntity entity);
        Task<IEnumerable<TEntity>> GetAll();
        Task Delete(object id);
        Task Delete(TEntity entityToDelete);
        Task Update(TEntity ententityToUpdate);
        Task<TEntity?> GetItemBySpec(ISpecification<TEntity> specification);
        Task<IEnumerable<TEntity>> GetListBySpec(ISpecification<TEntity> specification);
        Task<TEntity?> GetByID(object id);

    }
}
