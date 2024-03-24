namespace Ekzakt.FileManager.Core.Models;

public class ContentFile<T> : FileProperties
    where T : class?
{
    public T? Content { get; set; }
}
