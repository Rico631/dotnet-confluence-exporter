using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp4.Controllers.Lk;

/// <summary>
/// Controller for user personal area (LK) related endpoints.
/// </summary>
[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = "lk")]
public class LkController : ControllerBase
{

    /// <summary>
    /// Uploads a file to the server for the current user.
    /// </summary>
    /// <param name="file">The file being uploaded.</param>
    [HttpPost("save-file")]
    public async Task SaveFile(IFormFile file)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Returns a stream containing diagnostic data based on provided parameters.
    /// </summary>
    /// <param name="r1">String parameter used to filter or label the data.</param>
    /// <param name="r2">Numeric parameter used for selection or paging.</param>
    /// <param name="dt">Date/time parameter used to scope the returned data.</param>
    /// <returns>A <see cref="Stream"/> with the requested data. The caller is responsible for disposing the stream.</returns>
    [HttpGet("get-data")]
    public async Task<Stream> GetData(string r1, int r2, DateTimeOffset dt)
    {

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteLineAsync($"Received parameters: r1={r1}, r2={r2}, dt={dt}");
        await writer.FlushAsync();
        stream.Position = 0; // Reset the stream position to the beginning
        return stream;
    }
}
