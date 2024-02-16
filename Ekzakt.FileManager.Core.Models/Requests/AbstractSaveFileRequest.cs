﻿using System.Text.Json.Serialization;

namespace Ekzakt.FileManager.Core.Models.Requests;

public abstract class AbstractSaveFileRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;

    public abstract long InitialFileSize { get; set; }

    public abstract long Length { get; }

    [JsonIgnore]
    public IProgress<ProgressEventArgs>? ProgressHandler;
}
