﻿namespace Ekzakt.FileManager.Core.Models;

[Obsolete("Use FileInformation instead. This class will be removed in a future version.")]
public class FileProperties
{
    public string Name { get; set; } = string.Empty;

    public long Size { get; set; } = 0;

    public DateTime? CreatedOn { get; set; }
}
