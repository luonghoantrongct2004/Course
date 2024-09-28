namespace EduCourse.Entities
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public string UserID { get; set; }
        public int CourseID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } // Credit Card, PayPal, etc.

        // Navigation properties
        public User User { get; set; }

        public Course Course { get; set; }
    }
}