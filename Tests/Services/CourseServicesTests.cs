using System;
using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using CoursesAPI.Services.Services;
using CoursesAPI.Tests.MockObjects;
using Xunit;

namespace CoursesAPI.Tests.Services
{
	public class CourseServicesTests
	{
		private MockUnitOfWork<MockDataContext> _mockUnitOfWork;
		private CoursesServiceProvider _service;
		private List<TeacherRegistration> _teacherRegistrations;

		private const string SSN_DABS    = "1203735289";
		private const string SSN_GUNNA   = "1234567890";
		private const string INVALID_SSN = "9876543210";

		private const string NAME_DABS = "Daníel B. Sigurgeirsson";
		private const string NAME_GUNNA  = "Guðrún Guðmundsdóttir";

		private const int COURSEID_VEFT_20153 = 1337;
		private const int COURSEID_VEFT_20163 = 1338;
		private const int INVALID_COURSEID    = 9999;

		CourseInstance VEFT_20153 = new CourseInstance
		{
			ID         = COURSEID_VEFT_20153,
			CourseID   = "T-514-VEFT",
			SemesterID = "20153"
		};

		CourseInstance VEFT_20163 = new CourseInstance
		{
			ID         = COURSEID_VEFT_20163,
			CourseID   = "T-514-VEFT",
			SemesterID = "20163"
		};
		
		public CourseServicesTests()
		{
			_mockUnitOfWork = new MockUnitOfWork<MockDataContext>();

			#region Persons
			var persons = new List<Person>
			{
				// Of course I'm the first person,
				// did you expect anything else?
				new Person
				{
					ID    = 1,
					Name  = NAME_DABS,
					SSN   = SSN_DABS,
					Email = "dabs@ru.is"
				},
				new Person
				{
					ID    = 2,
					Name  = NAME_GUNNA,
					SSN   = SSN_GUNNA,
					Email = "gunna@ru.is"
				}
			};
			#endregion

			#region Course templates

			var courseTemplates = new List<CourseTemplate>
			{
				new CourseTemplate
				{
					CourseID    = "T-514-VEFT",
					Description = "Í þessum áfanga verður fjallað um vefþj...",
					Name        = "Vefþjónustur"
				}
			};
			#endregion

			#region Courses
			var courses = new List<CourseInstance>
			{
				VEFT_20153,
				VEFT_20163
			};
			#endregion

			#region Teacher registrations
			_teacherRegistrations = new List<TeacherRegistration>
			{
				new TeacherRegistration
				{
					ID               = 101,
					CourseInstanceID = COURSEID_VEFT_20153,
					SSN              = SSN_DABS,
					Type             = TeacherType.MainTeacher
				}
			};
			#endregion

			_mockUnitOfWork.SetRepositoryData(persons);
			_mockUnitOfWork.SetRepositoryData(courseTemplates);
			_mockUnitOfWork.SetRepositoryData(courses);
			_mockUnitOfWork.SetRepositoryData(_teacherRegistrations);

			// TODO: this would be the correct place to add 
			// more mock data to the mockUnitOfWork!

			_service = new CoursesServiceProvider(_mockUnitOfWork);
		}
		
		#region GetCoursesBySemester
		/// <summary>
		/// Grabs all courses from an empty list of courses
		/// Result is an empty list (surprise!)
		/// </summary>
		[Fact]
		public void GetCoursesBySemester_ReturnsEmptyListWhenNoDataDefined()
		{
			// Arrange:			
			_mockUnitOfWork.SetRepositoryData(new List<CourseInstance>{});

			// Act:
			var result = _service.GetCourseInstancesBySemester();

			// Assert:
			Assert.Empty(result);
		}

		//	(10%) 	The function should return all courses on a given semester (no more, no less!)
		/// <summary>
		///	Grabs all courses with semester = "20163"
		/// This should return a single course with ID = COURSEID_VEFT_20163
		/// MainTeacher should be unassigned (empty string)
		/// </summary>
		[Fact]
		public void GetCoursesBySemester_ReturnsAllCoursesOnGivenSemester()
		{
			// Arrange: N/A		

			// Act:
			var result = _service.GetCourseInstancesBySemester("20163");

			// Assert:
			Assert.Single(result); 
			Assert.Equal(VEFT_20163.ID, result[0].CourseInstanceID);			
			Assert.Equal("", result[0].MainTeacher);
		}

		//	(10%) 	If no semester is defined, it should return all courses for the semester 20153
		/// <summary>
		///	Grabs all courses with semester = "20153"
		/// This should return a single course with ID = COURSEID_VEFT_20153		
		/// </summary>
		[Fact]
		public void GetCoursesBySemester_ReturnsAllCoursesOnDefaultSemester()
		{
			// Arrange: N/A		

			// Act:
			var result = _service.GetCourseInstancesBySemester();

			// Assert:
			Assert.Single(result); 
			Assert.Equal(VEFT_20153.ID, result[0].CourseInstanceID);
		}

		//	(10%) 	For each course returned, the name of the main teacher of the course should be 
		//			included (see the definition of CourseInstanceDTO).
		/// <summary>
		///	Grabs all courses with semester = "20153"
		/// This should return a single course with ID = COURSEID_VEFT_20153
		/// MainTeacher should be equal to NAME_DABS
		/// </summary>
		[Fact]
		public void GetCoursesBySemester_ReturnsCourseWithDefinedMainTeacher()
		{
			// Arrange: N/A

			// Act:
			var result2015 = _service.GetCourseInstancesBySemester("20153");
			var result2016 = _service.GetCourseInstancesBySemester("20163");

			// Assert:
			// Asserting that we have a single result
			Assert.Single(result2015);						 
			// Asserting that we have the right course returned
			Assert.Equal(VEFT_20153.ID, result2015[0].CourseInstanceID);			
			// Asserting that the name of the mainTeacher is equals to NAME_DABS
			Assert.Equal(NAME_DABS, result2015[0].MainTeacher);
			
		}

		//	(10%) 	If the main teacher hasn't been defined, the name of the main teacher should 
		//			be returned as an empty string.
		/// <summary>
		///	Grabs all courses with semester = "20163"
		/// This should return a single course with ID = COURSEID_VEFT_20163
		/// MainTeacher should be an empty string
		/// </summary>
		[Fact]
		public void GetCoursesBySemester_ReturnsCourseWithUndefinedMainTeacher()
		{
			// Arrange: N/A

			// Act:			
			var result2016 = _service.GetCourseInstancesBySemester("20163");

			// Assert:
			// Asserting that we have a single result			
			Assert.Single(result2016);			 
			// Asserting that we have the right course returned			
			Assert.Equal(VEFT_20163.ID, result2016[0].CourseInstanceID);
			// Asserting that we have an empty string returned			
			Assert.Equal("", result2016[0].MainTeacher);
		}
		//	(10%)	Finally, the code is not correct when it comes to returning the name of the main teacher. You should:
		//			Write the unit tests for this functionality (which should fail) Modify the code until the tests pass.
		// DONE AND DONE!

		#endregion

		#region AddTeacher

		/// <summary>
		/// Adds a main teacher to a course which doesn't have a
		/// main teacher defined already (see test data defined above).
		/// </summary>
		[Fact]
		public void AddTeacher_WithValidTeacherAndCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			var prevCount = _teacherRegistrations.Count;			
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);

			// Assert:

			// Check that the dto object is correctly populated:
			Assert.Equal(SSN_GUNNA, dto.SSN);
			Assert.Equal(NAME_GUNNA, dto.Name);

			// Ensure that a new entity object has been created:
			var currentCount = _teacherRegistrations.Count;
			Assert.Equal(prevCount + 1, currentCount);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.Last();
			Assert.Equal(COURSEID_VEFT_20163, newEntity.CourseInstanceID);
			Assert.Equal(SSN_GUNNA, newEntity.SSN);
			Assert.Equal(TeacherType.MainTeacher, newEntity.Type);

			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.True(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

		[Fact]
//		[ExpectedException(typeof(AppObjectNotFoundException))]
		public void AddTeacher_InvalidCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Assert.Throws<AppObjectNotFoundException>( () => _service.AddTeacherToCourse(INVALID_COURSEID, model) );
		}

		/// <summary>
		/// Ensure it is not possible to add a person as a teacher
		/// when that person is not registered in the system.
		/// </summary>
		[Fact]
//		[ExpectedException(typeof (AppObjectNotFoundException))]
		public void AddTeacher_InvalidTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = INVALID_SSN,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Assert.Throws<AppObjectNotFoundException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
		}

		/// <summary>
		/// In this test, we test that it is not possible to
		/// add another main teacher to a course, if one is already
		/// defined.
		/// </summary>
		[Fact]
		//[ExpectedExceptionWithMessage(typeof (AppValidationException), "COURSE_ALREADY_HAS_A_MAIN_TEACHER")]
		public void AddTeacher_AlreadyWithMainTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Exception ex = Assert.Throws<AppValidationException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
			Assert.Equal(ex.Message, "COURSE_ALREADY_HAS_A_MAIN_TEACHER");
		}

		/// <summary>
		/// In this test, we ensure that a person cannot be added as a
		/// teacher in a course, if that person is already registered
		/// as a teacher in the given course.
		/// </summary>
		[Fact]
		// [ExpectedExceptionWithMessage(typeof (AppValidationException), "PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE")]
		public void AddTeacher_PersonAlreadyRegisteredAsTeacherInCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_DABS,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			Exception ex = Assert.Throws<AppValidationException>( () => _service.AddTeacherToCourse(COURSEID_VEFT_20153, model));
			Assert.Equal(ex.Message, "PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
		}

		#endregion
	}
}
