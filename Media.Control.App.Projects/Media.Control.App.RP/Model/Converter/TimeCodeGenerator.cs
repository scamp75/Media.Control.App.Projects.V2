using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Control.App.RP.Model.Converter
{
    public class TimeCodeGenerator
    {
        /// <summary>
        /// Generates the current time in ISO 8601 format with UTC.
        /// </summary>
        /// <returns>A string in the format "yyyy-MM-ddTHH:mm:ssZ".</returns>
        public static string GenerateStartTime()
        {
            // Get the current UTC time
            DateTime utcNow = DateTime.UtcNow;

            // Format the time as "yyyy-MM-ddTHH:mm:ssZ"
            return utcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        /// <summary>
        /// Converts a specific DateTime to ISO 8601 format with UTC.
        /// </summary>
        /// <param name="dateTime">The DateTime to convert.</param>
        /// <returns>A string in the format "yyyy-MM-ddTHH:mm:ssZ".</returns>
        public static string ConvertToStartTime(DateTime dateTime)
        {
            // Ensure the time is in UTC
            DateTime utcTime = dateTime.ToUniversalTime();

            // Format the time as "yyyy-MM-ddTHH:mm:ssZ"
            return utcTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }

}
