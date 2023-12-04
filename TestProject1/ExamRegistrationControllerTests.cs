using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ooadproject.Controllers;
using ooadproject.Data;
using ooadproject.Models;
using System.Security.Claims;

namespace ProjectTests
{
    [TestClass]
    public class ExamRegistrationControllerTests
    {
        private ExamRegistrationController? _controller;
        public DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private Mock<UserManager<Person>> _mockUserManager;

        [TestInitialize]
        public void TestInitialize()
        {
            // Set up DbContextOptions for in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase").EnableSensitiveDataLogging().Options;

            // Set up Mock UserManager
            _mockUserManager = new Mock<UserManager<Person>>(
                Mock.Of<IUserStore<Person>>(), null, null, null, null, null, null, null, null);

            using (var context = new ApplicationDbContext(_dbContextOptions))
            {
                
                // Add sample data to the in-memory database
                var student = new Student { UserName = "teststudent", FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Index = 12345, Department = "Computer Science", Year = 2 };
                //var student = new Student { Id = _personId++, UserName = "teststudent", FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Index = 12345, Department = "Computer Science", Year = 2 };
                var teacher = new Teacher { UserName = "testteacher", FirstName = "Jane", LastName = "Doe", Email = "jane.doe@example.com", Title = "dr." };
                var course = new Course { Name = "TestCourse", AcademicYear = "2023-2024", ECTS = 6, Semester = 3, TeacherID = teacher.Id, Teacher = teacher };
                var studentCourse = new StudentCourse { CourseID = course.ID, Course = course, StudentID = student.Id, Student = student, Grade = 8, Points = 73.5 };
                var exam = new Exam { CourseID = course.ID, Time = DateTime.Now.AddDays(1), Course = course, MinimumPoints = 10, TotalPoints = 20, Type = ExamType.Final };
                var examRegistration = new ExamRegistration { StudentID = 1, Student = student, ExamID = exam.ID, Exam = exam, RegistrationTime = DateTime.Now };
                context.AddRange(student, course, studentCourse, exam, examRegistration);
                context.SaveChanges();
                
                // Mock a user with a non-null Id
                _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ReturnsAsync(student);
            }

            _controller = new ExamRegistrationController(new ApplicationDbContext(_dbContextOptions), _mockUserManager.Object);
        }

        [TestMethod]
        public async Task Index_ReturnsViewWithModel()
        {
            // Act
            var result = await _controller.Index();

            //Assertations
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Create_WithValidModel_ReturnsRedirectToActionResult()
        {
            // Act
            var examID = 1; // valid id
            var examRegistration = new ExamRegistration { StudentID = 1, ExamID = examID, RegistrationTime = DateTime.Now.AddDays(2) };

            var result = await _controller.Create(examID, examRegistration) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public async Task Create_WithInvalidModel_ReturnsViewResult()
        {
            // Arrange
            var examID = 1; // valid id
            var examRegistration = new ExamRegistration { StudentID = 1, ExamID = examID, RegistrationTime = DateTime.Now.AddDays(-2) };

            // Simulate an invalid model state
            _controller.ModelState.AddModelError("RegistrationTime", "Registration time is not valid!");

            // Act
            var result = await _controller.Create(examID, examRegistration) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(examRegistration, result.Model); // Check if the model is passed back to the view
            //Assert.AreEqual(string.Empty, result.ViewName); // Check if it returns the default view
            Assert.IsFalse(result.ViewData.ModelState.IsValid); // Check if the ModelState is marked as invalid
        }

        [TestMethod]
        public async Task Delete_WithValidId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var examRegistrationId = 1; // Assuming a valid examRegistrationId

            // Act
            var result = await _controller.Delete(examRegistrationId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public async Task Delete_WithNullExamRegistrationSet_ReturnsProblemResult()
        {
            // Arrange     
            var mockContext = new Mock<ApplicationDbContext>(_dbContextOptions);
            mockContext.Setup(c => c.ExamRegistration).Returns((DbSet<ExamRegistration>)null);

            _controller = new ExamRegistrationController(mockContext.Object, _mockUserManager.Object);

            // Act
            var result = await _controller.Delete(1) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Value, typeof(ProblemDetails));
            var problemDetails = (ProblemDetails)result.Value;
            Assert.AreEqual("Entity set 'ApplicationDbContext.ExamRegistration'  is null.", problemDetails.Detail);
        }
    }
}

