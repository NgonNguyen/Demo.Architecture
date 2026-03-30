using Ardalis.SharedKernel;
using Demo.Architecture.Core.Base.Interfaces;

namespace Demo.Architecture.Core.Base;

public abstract class AppEntityBase<TId>
    : HasDomainEventsBase, IAuditable, IActivatable
{
    public TId Id { get; protected set; } = default!;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsActive { get; protected set; } = true;

    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
