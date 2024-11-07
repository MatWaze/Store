namespace Store.Models;

public interface IOrderRepository
{
    IQueryable<Order> Orders { get; }
    void SaveOrder(Order order);

    void DeleteOrder(Order order);
}
