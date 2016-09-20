using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.Services
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			// TODO: implement this logic!

			// AddTeacherViewModel has the following properties:
			//	-	string SSN
			//	-	TeacherType Type

			// the courseInstanceID represents the course we are working with

			// Teachertype is an enum with the following attributes:
			//	-	MainTeacher (1)
			//	-	AssistantTeacher (2)

			// We need to register a new TeacherRegistration to a course
			// this type holds the following properties: Auto Increment ID
			//	-	string SSN
			//	-	int CourseInstanceID
			//	-	TeacherType Type

			// Steps:

			//	1.	Check if a course with the given courseInstanceID exists
			var course = (
				from c in _courseInstances.All()
				where c.ID == courseInstanceID
				select c
			).SingleOrDefault();

			if(course == null)
				throw new AppObjectNotFoundException();
			
			//	2.	Check if a teacher with the given SSN exists then fetch if true
			var teacher = (
				from p in _persons.All()
				where p.SSN == model.SSN
				select p
			).SingleOrDefault();

			if(teacher == null)
				throw new AppObjectNotFoundException();

			//	3.	If model.Type is MainTeacher check if a MainTeacher is already registerd for the course
			if(model.Type == TeacherType.MainTeacher){
				var mainTeacherRegistration = (
					from r in _teacherRegistrations.All()
					where r.CourseInstanceID == courseInstanceID
					&& r.Type == TeacherType.MainTeacher
					select r
				).SingleOrDefault();
				if(mainTeacherRegistration != null)
					throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
			}
			
			// 4.	Check if the teacher with the given ssn is already teaching this course
			var registration = (
				from r in _teacherRegistrations.All()
				where r.CourseInstanceID == courseInstanceID
				&& r.SSN == model.SSN
				select r
			).SingleOrDefault();

			if(registration != null)
				throw new AppValidationException("PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
				
			//	5.  Actually create the damn thing!
			TeacherRegistration tr = new TeacherRegistration{
				SSN = model.SSN,
				Type = model.Type,
				CourseInstanceID = courseInstanceID				
			};

			_teacherRegistrations.Add(tr);
			_uow.Save();

			return new PersonDTO{
				SSN = teacher.SSN,
				Name = teacher.Name
			};
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

			var courses = ( // I'll never understand why people place code inline after the (
				from c in _courseInstances.All()
				join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID				
				where c.SemesterID == semester
				select new CourseInstanceDTO 
				{
					Name               = ct.Name,
					TemplateID         = ct.CourseID,
					CourseInstanceID   = c.ID					
			}).ToList();

			// Applying MainTeacher Names			
			foreach(var course in courses){				 
				course.MainTeacher = (
					from p in _persons.All()
					join tr in _teacherRegistrations.All() on p.SSN equals tr.SSN
					where tr.CourseInstanceID == course.CourseInstanceID
					select p.Name
				).SingleOrDefault() ?? "";
			}
			
			return courses;
		}
	}
}
