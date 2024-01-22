namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManager
{
    Task<IFileResult> SaveAsync();
}
