﻿namespace Ekzakt.FileManager.Core.Models;

public class FileInformation
{
    public string Name { get; set; } = string.Empty;

    public long Size { get; set; } = 0;

    public DateTime? CreatedOn { get; set; }
}
