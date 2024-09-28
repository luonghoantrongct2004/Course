namespace EduCourse.Entities
{
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; } // Liên kết với đơn hàng
        public int CourseID { get; set; } // Liên kết với khóa học
        public decimal CoursePrice { get; set; } // Giá của khóa học tại thời điểm mua
        public decimal Discount { get; set; } // Mức giảm giá áp dụng

        // Navigation properties
        public Order? Order { get; set; } // Liên kết với bảng Order
        public Course? Course { get; set; } // Liên kết với bảng Course
    }
}
