using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CoursesAPI.Services.Services
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;
		private int objectsPerPage;
		private int pageCount;
		private int pageNumber;
		private string language;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
			objectsPerPage = 10;
			pageCount = 0;
			pageNumber = 1;
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
			return null;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public dynamic GetCourseInstancesBySemester(string request, string semester = null, int page = 1)
		{
			List<CourseInstanceDTO> courses;
			Console.Write("BLAAAAA");
			//string AcceptLanguage = request.Headers.AcceptLanguage.ToString();
			//Console.Write(AcceptLanguage);
			if(request.Contains("en"))
				language = "en";
			else 
				language = "is";

			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}
			if(language == "en"){
				courses = ( // I'll never understand why people place code inline after the (
					from c in _courseInstances.All()
					join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID				
					where c.SemesterID == semester
					select new CourseInstanceDTO 
					{
						Name               = ct.NameEN,
						TemplateID         = ct.CourseID,
						CourseInstanceID   = c.ID					
					}).ToList();
			}else{//language is icelandic
				courses = ( 
					from c in _courseInstances.All()
					join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID				
					where c.SemesterID == semester
					select new CourseInstanceDTO 
					{
						Name               = ct.Name,
						TemplateID         = ct.CourseID,
						CourseInstanceID   = c.ID					
					}).ToList();
			}

			// Applying MainTeacher Names			
			foreach(var course in courses){				 
				course.MainTeacher = (
					from p in _persons.All()
					join tr in _teacherRegistrations.All() on p.SSN equals tr.SSN
					where tr.CourseInstanceID == course.CourseInstanceID
					select p.Name
				).SingleOrDefault() ?? "";
			}

			//If you input an invalid page number you always get the default page
			if(page <= 0)
				pageNumber = 1;
			else
				pageNumber = page;

			var items = courses
				.Skip(objectsPerPage * (pageNumber - 1))
				.Take(objectsPerPage)
				.ToList();

			int itemSize = courses.Count;
			pageCount = itemSize / objectsPerPage;
			
			//Very sloppy way of doing things I know.
			if(itemSize % 10 != 0)
				pageCount++;

			//If you input an invalid page number you always get the default page
			if(page > pageCount){
				pageNumber = 1;
				items = courses
				.Skip(objectsPerPage * (pageNumber - 1))
				.Take(objectsPerPage)
				.ToList();
			}

			var paging = new {	
						PageCount          = pageCount,
						PageSize           = objectsPerPage,
						PageNumber         = pageNumber,
						TotalNumberOfItems = itemSize
			};

			var envelope = new {
					Items    = items,
					Paging   = paging
			};

			return envelope;
		}
	}
}
