namespace Ekzakt.FileManager.Core.Models;

public class ContentFile<T> : FileInformation
    where T : class?
{
    public T? Content { get; set; }
}
