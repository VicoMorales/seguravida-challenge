using MediatR;

namespace SeguraVida.Claims.Application.Policies;

public sealed record GetPolicyByNumberQuery(string PolicyNumber) : IRequest<PolicyLookupDto>;
