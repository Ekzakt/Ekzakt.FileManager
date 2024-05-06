using System.Text.Json;

namespace Ekzakt.FileManager.Core.Models.EventArgs;

public class ProgressEventArgs
{
    public string FileName { get; set; } = string.Empty;

    public long BytesSent { get; set; } = 0;

    public long FileSize { get; set; } = 0;

    public double PercentageDone => (double)BytesSent / FileSize * 100;


    public override string ToString()
    {
        var result = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        return result.ToString();
    }
}
