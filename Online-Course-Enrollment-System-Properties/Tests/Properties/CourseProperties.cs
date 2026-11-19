using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Online_Course_Enrollment_System_Properties.Domain;
using Online_Course_Enrollment_System_Properties.Tests.Arbitraries;

namespace Online_Course_Enrollment_System_Properties.Tests.Properties
{
    public class CourseProperties
    {
        // P2 · Інваріант: кількість студентів не перевищує місткість.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Students_never_exceed_capacity(Course course, Student[] students)
        {
            foreach (Student student in students)
            {
                course.Enroll(student);
            }

            course.Students.Count.Should().BeLessThanOrEqualTo(course.Capacity);
            course.FreeSeats.Should().BeGreaterThanOrEqualTo(0);
        }

        // P1 · Туди й назад: запис і скасування повертають початковий стан.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enroll_then_unenroll_restores_course(Course course, Student student)
        {
            int initialSeats = course.FreeSeats;

            course.Enroll(student);
            course.Unenroll(student.Id);

            course.Students.Should().BeEmpty();
            course.FreeSeats.Should().Be(initialSeats);
        }

        // P3 · Ідемпотентність: повторний запис не додає студента вдруге.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enrolling_twice_does_not_duplicate_student(Course course, Student student)
        {
            course.Enroll(student).Should().BeTrue();
            course.Enroll(student).Should().BeFalse();

            course.Students.Should().ContainSingle();
        }

        // P7 · Легко перевірити: успішний запис займає одне місце.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enrollment_uses_one_free_seat(Course course, Student student)
        {
            int initialSeats = course.FreeSeats;

            course.Enroll(student).Should().BeTrue();

            course.FreeSeats.Should().Be(initialSeats - 1);
            course.IsEnrolled(student.Id).Should().BeTrue();
        }

        // P2 · Інваріант: після заповнення курсу вільних місць немає.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Filled_course_has_no_free_seats(Course course)
        {
            for (int i = 0; i < course.Capacity; i++)
            {
                course.Enroll(new Student($"unique-{i}"));
            }

            course.IsFull.Should().BeTrue();
            course.FreeSeats.Should().Be(0);
            course.Enroll(new Student("extra")).Should().BeFalse();
        }

        // P4 · Різні шляхи: порядок запису двох різних студентів не впливає на результат.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public Property Enrollment_order_does_not_matter(Course course, Student student)
        {
            bool applicable = course.Capacity >= 2;
            bool ok = true;

            if (applicable)
            {
                Student other = new Student($"other-{student.Id}");
                Course first = new Course(course.Id, course.Capacity);
                Course second = new Course(course.Id, course.Capacity);

                first.Enroll(student);
                first.Enroll(other);

                second.Enroll(other);
                second.Enroll(student);

                ok = first.Students.Count == second.Students.Count
                    && first.IsEnrolled(student.Id)
                    && first.IsEnrolled(other.Id)
                    && second.IsEnrolled(student.Id)
                    && second.IsEnrolled(other.Id);
            }

            return ok.When(applicable);
        }

        // P2 · Інваріант із передумовою: перед останнім записом лишається одне місце.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public Property One_seat_remains_before_last_student(Course course)
        {
            bool applicable = course.Capacity > 1;
            bool ok = true;

            if (applicable)
            {
                for (int i = 0; i < course.Capacity - 1; i++)
                {
                    course.Enroll(new Student($"unique-{i}"));
                }

                ok = course.FreeSeats == 1 && !course.IsFull;
            }

            return ok.When(applicable);
        }

        // P8 · Без винятків: запис валідного студента на новий курс успішний.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Valid_enrollment_does_not_throw(Course course, Student student)
        {
            Action act = () => course.Enroll(student);

            act.Should().NotThrow();
        }

        // Перевірка винятку: не можна записати null.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enrolling_null_throws(Course course)
        {
            Action act = () => course.Enroll(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        // Перевірка винятку: не можна скасувати неіснуючий запис.
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Unenrolling_missing_student_throws(Course course, Student student)
        {
            Action act = () => course.Unenroll(student.Id);

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
