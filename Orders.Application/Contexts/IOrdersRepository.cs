using Orders.Domain.Entities;

using Sol9.Core.Abstractions;

namespace Orders.Application.Contexts;

public interface IOrdersRepository : IRepository<Order>;
