using MediatR;
using SeguraVida.Claims.Application.Abstractions;
using SeguraVida.Claims.Application.Common;

namespace SeguraVida.Claims.Application.Policies;

public sealed class GetPolicyByNumberQueryHandler : IRequestHandler<GetPolicyByNumberQuery, PolicyLookupDto>
{
    private readonly IPolicyReadRepository _policies;

    public GetPolicyByNumberQueryHandler(IPolicyReadRepository policies)
    {
        _policies = policies;
    }

    public async Task<PolicyLookupDto> Handle(GetPolicyByNumberQuery request, CancellationToken cancellationToken)
    {
        return await _policies.GetByNumberAsync(request.PolicyNumber.Trim(), cancellationToken)
            ?? throw new NotFoundException("Policy was not found.");
    }
}
