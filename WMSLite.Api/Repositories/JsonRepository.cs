using System.Text.Json;
using WMSLite.Api.Models;

namespace WMSLite.Api.Repositories;

public class JsonRepository<T> : IJsonRepository<T> where T : class, IEntity
{
    private static readonly Dictionary<string, SemaphoreSlim> FileLocks = new();
    private static readonly object FileLocksGuard = new();

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock;

    public JsonRepository(IWebHostEnvironment environment)
    {
        var dataDir = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);

        _filePath = Path.Combine(dataDir, ResolveFileName());
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }

        lock (FileLocksGuard)
        {
            if (!FileLocks.TryGetValue(_filePath, out var semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                FileLocks[_filePath] = semaphore;
            }

            _fileLock = semaphore;
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            return await ReadAllNoLockAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var all = await ReadAllNoLockAsync();
            return all.FirstOrDefault(x => x.Id == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<T> InsertAsync(T entity)
    {
        await _fileLock.WaitAsync();
        try
        {
            var all = await ReadAllNoLockAsync();
            all.Add(entity);
            await WriteAllNoLockAsync(all);
            return entity;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<T?> UpdateAsync(T entity)
    {
        await _fileLock.WaitAsync();
        try
        {
            var all = await ReadAllNoLockAsync();
            var idx = all.FindIndex(x => x.Id == entity.Id);
            if (idx < 0) return null;

            all[idx] = entity;
            await WriteAllNoLockAsync(all);
            return entity;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var all = await ReadAllNoLockAsync();
            var removed = all.RemoveAll(x => x.Id == id);
            if (removed == 0) return false;

            await WriteAllNoLockAsync(all);
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<T>> ReadAllNoLockAsync()
    {
        await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _jsonOptions);
        return data ?? new List<T>();
    }

    private async Task WriteAllNoLockAsync(List<T> entities)
    {
        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, entities, _jsonOptions);
    }

    private static string ResolveFileName()
    {
        return typeof(T).Name switch
        {
            nameof(Tenant) => "tenants.json",
            nameof(User) => "users.json",
            nameof(Item) => "items.json",
            nameof(InventoryRecord) => "inventory.json",
            nameof(Order) => "orders.json",
            nameof(Subscription) => "subscriptions.json",
            nameof(Location) => "locations.json",
            _ => throw new InvalidOperationException($"No JSON file mapping for {typeof(T).Name}")
        };
    }
}
