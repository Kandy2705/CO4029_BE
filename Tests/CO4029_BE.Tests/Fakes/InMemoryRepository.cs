using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;
using Supabase.Postgrest.Models;

namespace CO4029_BE.Tests.Fakes;

/// <summary>Dictionary-backed repository; ids are generated like the database default.</summary>
public class InMemoryRepository<T> : IRepository<T> where T : BaseModel, new()
{
    private readonly Func<T, string?> _getId;
    private readonly Action<T, string> _setId;
    public Dictionary<string, T> Items { get; } = new();

    public InMemoryRepository(Func<T, string?> getId, Action<T, string> setId)
    {
        _getId = getId;
        _setId = setId;
    }

    public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(Items.Values.ToList());

    public Task<T?> GetByIdAsync(string id) => Task.FromResult(Items.GetValueOrDefault(id));

    public virtual Task<T> CreateAsync(T entity)
    {
        var id = _getId(entity);
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            _setId(entity, id);
        }
        Items[id] = entity;
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(string id, T entity)
    {
        if (Items.ContainsKey(id))
            Items[id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        Items.Remove(id);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync() => Task.FromResult(Items.Count);
}

public class InMemoryOtpRepository : InMemoryRepository<OtpCode>, IOtpCodeRepository
{
    public InMemoryOtpRepository() : base(o => o.id, (o, id) => o.id = id) { }

    public Task<IEnumerable<OtpCode>> GetByKeyAsync(string key)
        => Task.FromResult<IEnumerable<OtpCode>>(Items.Values.Where(o => o.email == key).ToList());
}

public class InMemoryUserRepository : InMemoryRepository<User>, IUserRepository
{
    public InMemoryUserRepository() : base(u => u.id, (u, id) => u.id = id) { }

    public Task<User?> GetByEmailAsync(string email)
        => Task.FromResult(Items.Values.FirstOrDefault(u => u.email == email));

    public Task<(IEnumerable<User> Items, int TotalItems)> GetPagedAsync(
        int offset, int limit, string? search, string? role, bool? isActive)
    {
        var all = Items.Values.ToList();
        return Task.FromResult<(IEnumerable<User>, int)>((all.Skip(offset).Take(limit), all.Count));
    }

    public Task<int> CountByActiveAsync(bool isActive)
        => Task.FromResult(Items.Values.Count(u => u.is_active == isActive));
}

public class InMemoryBuildingRepository : InMemoryRepository<Building>, IBuildingRepository
{
    public InMemoryBuildingRepository() : base(b => b.ID, (b, id) => b.ID = id) { }

    public Task<IEnumerable<Building>> GetBuildingsByUserId(string userID)
        => Task.FromResult<IEnumerable<Building>>(Items.Values.Where(b => b.UserId == userID).ToList());
}

public class InMemoryFloorRepository : InMemoryRepository<Floor>, IFloorRepository
{
    public InMemoryFloorRepository() : base(f => f.Id, (f, id) => f.Id = id) { }

    public Task<IEnumerable<Floor>> GetByBuildingIdAsync(string buildingId)
        => Task.FromResult<IEnumerable<Floor>>(Items.Values.Where(f => f.BuildingId == buildingId).ToList());
}

public class InMemoryRoomRepository : InMemoryRepository<Room>, IRoomRepository
{
    public InMemoryRoomRepository() : base(r => r.Id, (r, id) => r.Id = id) { }

    public Task<IEnumerable<Room>> GetByFloorIdAsync(string floorId)
        => Task.FromResult<IEnumerable<Room>>(Items.Values.Where(r => r.FloorId == floorId).ToList());
}
