using boltalka.Application.Abstractions.Storage;

namespace boltalka.Application.Tests.Fakes;

public class FakeFileStorage : IFileStorage
{
    public Task<string> SaveAsync(string fileName, Stream content, string contentType, CancellationToken ct = default)
    {
        return Task.FromResult($"fake/path/{Guid.NewGuid()}");
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
        => Task.CompletedTask;
}