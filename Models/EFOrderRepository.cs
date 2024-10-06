using Microsoft.EntityFrameworkCore;
using Store.Infrastructure;

namespace Store.Models;

public class EFOrderRepository : IOrderRepository
{
    private DataContext ctx;

    public EFOrderRepository(DataContext context)
    {
        ctx = context;
    }

    public IQueryable<Order> Orders => ctx.Orders
        // .Include(u => u.User)
        .Include(o => o.Lines)
        .ThenInclude(l => l.Product);

    public void SaveOrder(Order order)
    {
        ctx.AttachRange(order.Lines.Select(l => l.Product));
        if (order.OrderID == 0)
        {
            Console.WriteLine("adding new order");
            ctx.Orders.Add(order);
        }
        ctx.SaveChanges();
    }
}