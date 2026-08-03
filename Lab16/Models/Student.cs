namespace lab16.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int YearOfStudy { get; set; }
        public double Gpa { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}