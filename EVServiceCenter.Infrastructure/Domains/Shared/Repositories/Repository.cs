using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.Shared.Interfaces;

namespace EVServiceCenter.Infrastructure.Domains.Shared.Repositories
{
  public class Repository<T> : IRepository<T> where T : class
  {
    protected readonly EVDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(EVDbContext context)
    {
      _context = context ?? throw new ArgumentNullException(nameof(context));
      _dbSet = _context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
      return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
      return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
      if (entity == null) throw new ArgumentNullException(nameof(entity));
      await _dbSet.AddAsync(entity);
      await _context.SaveChangesAsync();
      return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
      if (entity == null) throw new ArgumentNullException(nameof(entity));
      _dbSet.Update(entity);
      await _context.SaveChangesAsync();
      return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
      var entity = await GetByIdAsync(id);
      if (entity == null) return false;
      _dbSet.Remove(entity);
      await _context.SaveChangesAsync();
      return true;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
      return await _dbSet.FindAsync(id) != null;
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));
      return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));
      return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<int> CountAsync()
    {
      return await _dbSet.CountAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));
      return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
    {
      if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
      if (pageSize < 1 || pageSize > SystemConstants.MAX_PAGE_SIZE) throw new ArgumentException($"Page size must be between 1 and {SystemConstants.MAX_PAGE_SIZE}.", nameof(pageSize));
      return await _dbSet
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate)
    {
      if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0.", nameof(pageNumber));
      if (pageSize < 1 || pageSize > SystemConstants.MAX_PAGE_SIZE) throw new ArgumentException($"Page size must be between 1 and {SystemConstants.MAX_PAGE_SIZE}.", nameof(pageSize));
      if (predicate == null) throw new ArgumentNullException(nameof(predicate));
      return await _dbSet
          .Where(predicate)
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();
    }

    public virtual async Task CreateRangeAsync(IEnumerable<T> entities)
    {
      if (entities == null) throw new ArgumentNullException(nameof(entities));
      await _dbSet.AddRangeAsync(entities);
      await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
      if (entities == null) throw new ArgumentNullException(nameof(entities));
      _dbSet.UpdateRange(entities);
      await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
      if (entities == null) throw new ArgumentNullException(nameof(entities));
      _dbSet.RemoveRange(entities);
      await _context.SaveChangesAsync();
    }
  }
}