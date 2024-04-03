namespace Ekzakt.FileManager.Core.Models.Requests;

public abstract class AbstractFileRequest
{
    /// <summary>
    /// This property represents or the Azure blob container name when
    /// using FileManager.AzureBlob or the base folder when using 
    /// FileManager.Io.
    /// </summary>
    public string BaseLocation { get; set; } = string.Empty;


    /// <summary>
    /// This property represents a list of subfolders that should 
    /// be accessed when doing file operations.
    /// </summary>
    public List<string> Paths { get; set; } = new();


    /// <summary>
    /// This property gets the combined path for all values
    /// in the Paths property.
    /// </summary>
    /// <param name="separator"></param>
    public string GetPathsString(string? separator = null) => string.Join(separator ?? "/", Paths);
}
