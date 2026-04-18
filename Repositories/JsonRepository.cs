using System.Collections.Concurrent;
using System.Text.Json;
using WMSLite.Models;

namespace WMSLite.Repositories;

public class JsonRepository<T> : IJsonRepository<T> where T : class, IEntity
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new();
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock;
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
        _lock = FileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
        EnsureFileExists();
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await ReadAllInternalAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(x => x.Id == id);
    }

    public async Task InsertAsync(T entity)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllInternalAsync();
            all.Add(entity);
            await WriteAllInternalAsync(all);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(T entity)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllInternalAsync();
            var index = all.FindIndex(x => x.Id == entity.Id);
            if (index < 0)
            {
                throw new InvalidOperationException($"Entity with id {entity.Id} not found.");
            }

            all[index] = entity;
            await WriteAllInternalAsync(all);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await ReadAllInternalAsync();
            all.RemoveAll(x => x.Id == id);
            await WriteAllInternalAsync(all);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureFileExists()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private async Task<List<T>> ReadAllInternalAsync()
    {
        await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var data = await JsonSerializer.DeserializeAsync<List<T>>(stream, _serializerOptions);
        return data ?? [];
    }

    private async Task WriteAllInternalAsync(List<T> entities)
    {
        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, entities, _serializerOptions);
    }
}
