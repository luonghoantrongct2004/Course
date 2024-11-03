namespace EduCourse.Entities
{
    public class UserProgress
    {
        public int UserProgressID { get; set; }
        public string UserID { get; set; } // ID người dùng
        public int LessonID { get; set; } // ID bài học

        public double WatchedPercentage { get; set; } // Tỷ lệ đã xem (từ 0 đến 100)
        public string CompletionStatus { get; set; } // "In Progress" hoặc "Completed"
        public DateTime? LastWatchedTime { get; set; } // Thời gian cuối cùng học viên xem bài học
        public DateTime? CompletionDate { get; set; } // Ngày hoàn thành

        // Quan hệ với người dùng và bài học
        public User? User { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
