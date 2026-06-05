using MediatR;

namespace SeguraVida.Claims.Application.Claims;

public sealed record ChangeClaimStatusCommand(Guid ClaimId, string ChangedBy) : IRequest<Unit>;

public sealed record ApproveClaimCommand(Guid ClaimId, decimal ApprovedAmount, string PeritajeNotes, string ChangedBy) : IRequest<Unit>;

public sealed record RejectClaimCommand(Guid ClaimId, string PeritajeNotes, string ChangedBy) : IRequest<Unit>;

public sealed record PayClaimCommand(Guid ClaimId, string ChangedBy) : IRequest<Unit>;
