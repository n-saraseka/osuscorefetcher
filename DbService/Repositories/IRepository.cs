using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuscorefetcher.DbService
{
    public interface IRepository<T> : IDisposable
        where T : class
    {
        IEnumerable<T> GetAll();
        T Get(int id);
        void Create(T item);
        void CreateBulk(IEnumerable<T> items);
        void Update(T item);
        void UpdateBulk(IEnumerable<T> items);
        void Delete(int id);
        void DeleteBulk(IEnumerable<int> items);
        void Save();
    }
}
