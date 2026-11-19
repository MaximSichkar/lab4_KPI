using FsCheck;
using FsCheck.Fluent;
using Online_Course_Enrollment_System_Properties.Domain;

namespace Online_Course_Enrollment_System_Properties.Tests.Arbitraries
{
    public static class CourseArbitraries
    {
        public static Arbitrary<Student> Student()
        {
            var generator = Gen.Choose(0, 100000)
                .Select(number => new Student($"student-{number}"));

            return generator.ToArbitrary();
        }

        public static Arbitrary<Course> Course()
        {
            var generator =
                from number in Gen.Choose(0, 100000)
                from capacity in Gen.OneOf(
                    Gen.Constant(1),
                    Gen.Choose(1, 30))
                select new Course($"course-{number}", capacity);

            return generator.ToArbitrary();
        }
    }
}
