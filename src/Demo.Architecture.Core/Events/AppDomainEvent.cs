using Ardalis.SharedKernel;
using MediatR;

namespace Demo.Architecture.Core.Events;

public abstract class AppDomainEvent : DomainEventBase, INotification
{
}
