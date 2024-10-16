using EduCourse.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduCourse.Areas.Admin.Controllers;

[Area("Admin")]
public class OrderController : Controller
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int page = 1, int pageSize = 10)
    {
        var totalOrders = _context.Orders.Count();
        var orders = _context.Orders
            .Include(u => u.User)
            .Include(od => od.OrderDetails)
                .ThenInclude(u => u.Course)
             .OrderByDescending(d => d.OrderDate)
             .Skip((page - 1) * pageSize)
                            .Take(pageSize)
            .ToList();
        ViewData["TotalOrders"] = totalOrders;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        return View(orders);
    }
    public IActionResult Details(int id)
    {
        var order = _context.Orders
            .Include(u => u.User)
            .Include(od => od.OrderDetails)
                .ThenInclude(u => u.Course)
            .FirstOrDefault(o => o.OrderID == id);
        return View(order);
    }
}
