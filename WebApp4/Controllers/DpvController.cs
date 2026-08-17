using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp4.Shared.Dpv;

namespace WebApp4.Controllers.Dpv;

/// <summary>
/// Controller for DPV (document processing) operations.
/// </summary>
[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = "dpv")]
public class DpvController : ControllerBase
{

    /// <summary>
    /// Saves provided documents and returns the result of the operation.
    /// </summary>
    /// <param name="request">Documents to save.</param>
    /// <returns>A <see cref="AddCartItemResponse"/> containing the outcome.</returns>
    [HttpPost("documents/save")]
    public async Task<AddCartItemResponse> SaveDocuments([FromBody] AddCartItemRequest request)
    {

        return new();
    }
}
