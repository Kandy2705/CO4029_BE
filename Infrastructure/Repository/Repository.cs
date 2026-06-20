using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;
using Supabase.Postgrest.Models;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IRepository<T> where T : BaseModel, new()
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(string id);
        Task<T> CreateAsync(T entity);
        Task UpdateAsync(string id, T entity);
        Task DeleteAsync(string id);
    }

    public class Repository<T> : IRepository<T> where T : BaseModel, new()
    {
        protected readonly Supabase.Client Client;
        protected readonly IPostgrestTable<T> Table;

        public Repository(Supabase.Client client)
        {
            Client = client;
            Table = Client.Postgrest.Table<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var response = await Table.Get();
            return response.Models;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var model = await Table
                .Filter("id", Constants.Operator.Equals, id)
                .Get();

            return model.Models.FirstOrDefault();
        }

        public async Task<T> CreateAsync(T entity)
        {
            var response = await Table.Insert(entity);
            return response.Model!;
        }

        public async Task UpdateAsync(string id, T entity)
        {
            await Table
                .Filter("id", Constants.Operator.Equals, id)
                .Update(entity);
        }

        public async Task DeleteAsync(string id)
        {
            await Table
                .Filter("id", Constants.Operator.Equals, id)
                .Delete();
        }
    }
}