namespace EduCourse.Entities;

public class Order
{
    public int OrderID { get; set; } // Khoá chính của bảng Order
    public string UserID { get; set; } // Khoá ngoại trỏ đến bảng User
    public DateTime OrderDate { get; set; } // Ngày đặt hàng
    public string PaymentStatus { get; set; } // Trạng thái thanh toán, ví dụ Pending, Completed
    public decimal TotalAmount { get; set; } // Tổng số tiền của đơn hàng
    public string PaymentMethod { get; set; } = "Card";

    // Navigation properties
    public User? User { get; set; } // Thuộc tính điều hướng trỏ đến entity User
    public ICollection<OrderDetail>? OrderDetails { get; set; } // Thuộc tính điều hướng cho các chi tiết đơn hàng
}
