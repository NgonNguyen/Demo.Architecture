using Demo.Architecture.Core.Entities.Orders;
using Demo.Architecture.Core.Entities.Products;
using Demo.Architecture.Core.ValueObjects;
using Vogen;

namespace Demo.Architecture.Infrastructure.Data.Config;

[EfCoreConverter<OrderId>]
[EfCoreConverter<ProductId>]
[EfCoreConverter<OrderItemId>]
[EfCoreConverter<Quantity>]
[EfCoreConverter<Money>]
internal partial class VogenEfCoreConverters;
