using MediatR;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Application.Claims;

public sealed class GetClaimDetailQueryHandler : IRequestHandler<GetClaimDetailQuery, ClaimDetailDto>
{
    private readonly IClaimReadRepository _claims;

    public GetClaimDetailQueryHandler(IClaimReadRepository claims)
    {
        _claims = claims;
    }

    public async Task<ClaimDetailDto> Handle(GetClaimDetailQuery request, CancellationToken cancellationToken)
    {
        return await _claims.GetDetailAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException("Claim was not found.");
    }
}
