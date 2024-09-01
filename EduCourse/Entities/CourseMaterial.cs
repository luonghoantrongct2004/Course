namespace EduCourse.Entities
{
    public class CourseMaterial
    {
        public int CourseMaterialID { get; set; }
        public int CourseID { get; set; }
        public string MaterialName { get; set; }
        public string MaterialType { get; set; } // E.g., PDF, eBook, etc.
        public string MaterialURL { get; set; } // URL to the material

        // Navigation properties
        public Course Course { get; set; }
    }

}
