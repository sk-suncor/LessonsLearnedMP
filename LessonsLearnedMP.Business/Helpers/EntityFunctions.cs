using System;

namespace System.Data.Objects.Core
{
	public class EntityFunctions
    {

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method return the given date with the time portion cleared.
        /// </summary>
        /// <param name="dateValue"></param>
        /// <returns></returns>
        public static System.DateTimeOffset? TruncateTime(System.DateTimeOffset? dateValue)
        {
            if (dateValue.HasValue == false)
                return default;
            return new DateTimeOffset(dateValue.Value.Date, TimeSpan.Zero);
        }

        /// <summary>
        /// When used as part of a LINQ to Entities query, this method return the given date with the time portion cleared.
        /// </summary>
        /// <param name="dateValue"></param>
        /// <returns></returns>
        public static System.DateTime? TruncateTime(System.DateTime? dateValue)
        {
            if (dateValue.HasValue == false)
                return default;

            return dateValue.Value.Date;
        }

        public static System.Int32? DiffDays(System.DateTimeOffset? dateValue1, System.DateTimeOffset? dateValue2)
        {
            if (!dateValue1.HasValue || !dateValue2.HasValue)
                return default;

            TimeSpan tm = dateValue2.Value.Subtract(dateValue1.Value);

            return System.Convert.ToInt32(Fix(tm.TotalDays));
        }

        public static System.Int32? DiffDays(DateTime? dateValue1, DateTime? dateValue2)
        {
            if (!dateValue1.HasValue || !dateValue2.HasValue)
                return default;

            TimeSpan tm = dateValue2.Value.Subtract(dateValue1.Value);

            return System.Convert.ToInt32(Fix(tm.TotalDays));

        }
        private static double Fix(double Number)
        {
            if (Number >= 0)
                return Math.Floor(Number);
            else
                return -System.Math.Floor(-Number);
        }
    }
}
