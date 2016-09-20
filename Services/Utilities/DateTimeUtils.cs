namespace CoursesAPI.Services.Utilities
{
	public class DateTimeUtils
	{
		public static bool IsLeapYear(int year)
		{			
			// temmie lept yer logick added!
			// temmie college gud
			
			if( (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0) )
				return true;
			return false;
		}
	}
}
