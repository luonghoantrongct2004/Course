using EduCourse.Data;
using EduCourse.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Security.Claims;

namespace EduCourse.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }
       var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var purchasedCourses = _context.Orders
        .Where(o => o.UserID == userId && o.PaymentStatus == "Completed")
         .SelectMany(o => o.OrderDetails.Select(od => od.CourseID))
         .ToList();


        ViewBag.PurchasedCourses = purchasedCourses;

        Guid.TryParse(userId, out Guid id);

        return View(_context.Carts
            .Include(p => p.Course)
                .ThenInclude(c => c.Chapters)
                    .ThenInclude(l => l.Lessons)
            .Include(u => u.User)
            .Where(u => u.UserId == id )
            .ToList());
    }
    [HttpPost]
    public async Task<IActionResult> AddToCart(int courseId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }
        try
        {
           var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Guid.TryParse(userId, out Guid id);

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == id);

            if (existingCartItem == null)
            {
                var cartItem = new Cart
                {
                    CourseId = courseId,
                    UserId = id
                };

                _context.Carts.Add(cartItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
            }

            return Json(new { success = true, message = "Khóa học đã có trong giỏ hàng" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int courseId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Auth/Login");
        }

        try
        {
           var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userId, out Guid id);

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == id);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa khỏi giỏ hàng" });
            }

            return Json(new { success = false, message = "Không tìm thấy khóa học trong giỏ hàng" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult Checkout([FromBody] List<int> courseIds)
    {
        if (courseIds == null || !courseIds.Any())
        {
            return BadRequest("Không có khóa học nào được chọn để thanh toán.");
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var selectedCourses = _context.Courses.Where(c => courseIds.Contains(c.CourseID)).ToList();

        if (!selectedCourses.Any())
        {
            return BadRequest("Không tìm thấy khóa học nào để thanh toán.");
        }

        // Tính tổng số tiền
        decimal totalAmount = 0;
        var orderDetails = new List<OrderDetail>();

        foreach (var course in selectedCourses)
        {
            var price = course.Price;
            totalAmount += price;

            orderDetails.Add(new OrderDetail
            {
                CourseID = course.CourseID,
                CoursePrice = price
            });
        }

        var order = new Order
        {
            UserID = userId,
            OrderDate = DateTime.Now,
            PaymentStatus = "Pending",
            TotalAmount = totalAmount,
            OrderDetails = orderDetails
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        // Thêm các khóa học đã mua vào bảng UserCourse
        foreach (var course in selectedCourses)
        {
            var userCourse = new UserCourse
            {
                UserID = userId,
                CourseID = course.CourseID,
                EnrollDate = DateTime.Now,
                Status = "Active"
            };

            _context.UserCourses.Add(userCourse);
        }

        _context.SaveChanges(); // Lưu lại tất cả các khóa học đã thêm vào UserCourse

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = Url.Action("PaymentSuccess", "Cart", new { orderId = order.OrderID }, Request.Scheme),
            CancelUrl = Url.Action("Index", "Cart", null, Request.Scheme) + "?paymentStatus=cancelled",
        };

        foreach (var course in selectedCourses)
        {
            var price = course.Price * (1 - course.Discount / 100.0M);

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = price, // Stripe yêu cầu số tiền ở đơn vị nhỏ nhất (cent), nên nhân với 100
                    Currency = "vnd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = course.Title,
                    },
                },
                Quantity = 1,
            });
        }

        var service = new SessionService();
        Session session = service.Create(options);

        return Json(new { sessionId = session.Id });
    }

    [HttpPost]
    public IActionResult CheckoutSingleCourse([FromBody] int courseId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized("Bạn cần đăng nhập để thực hiện thanh toán.");
        }

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var course = _context.Courses.FirstOrDefault(c => c.CourseID == courseId);
        if (course == null)
        {
            return BadRequest("Khóa học không tồn tại.");
        }

        var price = course.Price * (1 - course.Discount / 100.0M);
        var roundedPrice = (long)Math.Round(price);

        // Tạo session thanh toán với Stripe
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = roundedPrice, // Đơn vị tiền là cent
                    Currency = "vnd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = course.Title,
                    },
                },
                Quantity = 1,
            }
        },
            Mode = "payment",
            SuccessUrl = Url.Action("PaymentSuccessSingle", "Cart", null, Request.Scheme) + "?courseId=" + course.CourseID,
            CancelUrl = Url.Action("PaymentCancelled", "Cart", null, Request.Scheme),
        };

        var service = new SessionService();
        var session = service.Create(options);

        return Json(new { sessionId = session.Id });
    }

    public IActionResult PaymentSuccessSingle(int courseId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var order = new Order
        {
            UserID = userId,
            OrderDate = DateTime.Now,
            PaymentStatus = "Completed",
            TotalAmount = _context.Courses.First(c => c.CourseID == courseId).Price,
            OrderDetails = new List<OrderDetail>
        {
            new OrderDetail
            {
                CourseID = courseId,
                CoursePrice = _context.Courses.First(c => c.CourseID == courseId).Price
            }
        }
        };

        _context.Orders.Add(order);

        var userCourse = new UserCourse
        {
            UserID = userId,
            CourseID = courseId,
            EnrollDate = DateTime.Now,
            Status = "Active"
        };

        _context.UserCourses.Add(userCourse);

        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }


    public IActionResult PaymentSuccess(int orderId)
    {
        var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);

        if (order != null)
        {
            order.PaymentStatus = "Completed";
            _context.SaveChanges();
        }

        ViewData["PaymentMessage"] = "Lotus Acedemy cảm ơn bạn đã mua khóa học.";

        return RedirectToAction("Index", "Home", new { paymentMessage = ViewData["PaymentMessage"] });
    }

}
