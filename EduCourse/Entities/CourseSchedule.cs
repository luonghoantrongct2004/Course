namespace EduCourse.Entities
{
    public class CourseSchedule
    {
        public int CourseScheduleID { get; set; }
        public int CourseID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ScheduleDetails { get; set; } // E.g., "Every Monday and Wednesday"

        // Navigation properties
        public Course Course { get; set; }
    }

}
