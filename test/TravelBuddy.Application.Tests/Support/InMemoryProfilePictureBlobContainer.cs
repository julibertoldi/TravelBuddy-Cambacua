using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TravelBuddy.Users;
using Volo.Abp.BlobStoring;

namespace TravelBuddy;

public sealed class InMemoryProfilePictureBlobContainer : IBlobContainer<ProfilePictureContainer>
{
    private readonly ConcurrentDictionary<string, byte[]> _blobs = new();

    public async Task SaveAsync(
        string name,
        Stream stream,
        bool overrideExisting = false,
        CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        var content = buffer.ToArray();

        if (overrideExisting)
        {
            _blobs[name] = content;
            return;
        }

        if (!_blobs.TryAdd(name, content))
        {
            throw new InvalidOperationException($"A blob named '{name}' already exists.");
        }
    }

    public Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_blobs.TryRemove(name, out _));

    public Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult(_blobs.ContainsKey(name));

    public Task<Stream> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        if (!_blobs.TryGetValue(name, out var content))
        {
            throw new FileNotFoundException($"Blob '{name}' was not found.");
        }

        return Task.FromResult<Stream>(new MemoryStream((byte[])content.Clone(), writable: false));
    }

    public Task<Stream?> GetOrNullAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream?>(
            _blobs.TryGetValue(name, out var content)
                ? new MemoryStream((byte[])content.Clone(), writable: false)
                : null);
}
