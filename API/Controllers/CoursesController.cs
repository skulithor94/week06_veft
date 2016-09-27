using Microsoft.AspNetCore.Mvc;

using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Services;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CoursesAPI.Controllers
{
	[Route("api/courses")]
	public class CoursesController : Controller
	{
		private readonly CoursesServiceProvider _service;

		public CoursesController(IUnitOfWork uow)
		{
			_service = new CoursesServiceProvider(uow);
		}

		[HttpGet]
		public IActionResult GetCoursesBySemester( string semester = null, int page = 1)
		{
			// TODO: figure out the requested language (if any!)
			// and pass it to the service provider!
			string language = Request.Headers["Accept-Language"]; 
			return Ok(_service.GetCourseInstancesBySemester(language, semester, page));
		}

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("{id}/teachers")]
		public IActionResult AddTeacher(int id, AddTeacherViewModel model)
		{
			var result = _service.AddTeacherToCourse(id, model);
			return Created("TODO", result);
		}
	}
}
