using SeguraVida.Claims.Application.Claims;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Application.Abstractions;

public interface IClaimReadRepository
{
    Task<PagedResult<ClaimListItemDto>> SearchAsync(ClaimFilters filters, CancellationToken cancellationToken);

    Task<ClaimDetailDto?> GetDetailAsync(Guid claimId, CancellationToken cancellationToken);
}
