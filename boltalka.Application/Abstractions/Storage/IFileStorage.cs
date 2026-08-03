namespace boltalka.Application.Abstractions.Storage;

public interface IFileStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<string> SaveAsync(string fileName, Stream content, string contentType, CancellationToken ct = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="storagePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}