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

    public IActionResult Index()
    {
        var orders = _context.Orders
            .Include(u => u.User)
            .Include(od => od.OrderDetails)
                .ThenInclude(u => u.Course)
            .ToList();
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
