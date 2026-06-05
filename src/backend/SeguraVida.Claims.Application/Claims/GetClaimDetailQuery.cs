using MediatR;

namespace SeguraVida.Claims.Application.Claims;

public sealed record GetClaimDetailQuery(Guid ClaimId) : IRequest<ClaimDetailDto>;
