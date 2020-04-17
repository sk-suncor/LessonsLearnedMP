using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Suncor.LessonsLearnedMP.Framework.FluentListExporterColumns;

namespace Suncor.LessonsLearnedMP.Framework
{
    public static class Utility
    {
        public const string DisplayDateFormat = "{0:MM/dd/yyyy h:mm tt}";
        
        public static T SafeGetAppConfigSetting<T>(string configAppSettingsKey, T defaultValueIfNotFound)
        {
            string config = ConfigurationManager.AppSettings[configAppSettingsKey];
            if (!String.IsNullOrEmpty(config))
            {
                T retValue = (T)Convert.ChangeType(config, typeof(T));
                return retValue;
            }

            return defaultValueIfNotFound;
        }

        public static List<string> GetDistinguishedNameFilters()
        {
            List<string> result = new List<string>();
            DistinguishedNameFilterConfigurationSection filterConfigSection = ConfigurationManager.GetSection("distinguishedNameFilters") as DistinguishedNameFilterConfigurationSection;
            if (filterConfigSection != null)
            {
                result.AddRange(filterConfigSection.Filters.Select(filter => filter.DistinguishedName));
            }

            return result;
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// 
        /// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        /// 
        /// Provides a method for performing a deep copy of an object.
        /// Binary Serialization is used to perform the copy.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneObject<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static T Clone<T>(this T source)
        {
            return CloneObject(source);
        }

        public static string Serialize(this Dictionary<string, string> dictionary)
        {
            var xdoc = new XDocument(new XElement("root", dictionary.Select(entry => new XElement(entry.Key, entry.Value))));
            return xdoc.ToString();
        }

        public static bool IsNumeric(string expression)
        {
            Regex regexNumber = new Regex(@"^\d+$");
            return regexNumber.Match(expression).Success;
        }

        public static bool IsAlpha(string expression)
        {
            Regex regexAlpha = new Regex("^[A-Za-z]$");
            return regexAlpha.Match(expression).Success;
        }

        public static string StringValue(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }

            return value.ToString();
        }

        public static T ToEnum<T>(this string value)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == value)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == value)
                        return (T)field.GetValue(null);
                }
            }

            return (T)Enum.Parse(typeof(T), value);
        }

        public static DateTime GetCurrentDateTimeAsMST()
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Mountain Standard Time");
        }

        public static string ToSentenceCase(this string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
        }

        public static string ToDisplayDate(this DateTime date, bool includeTimeZoneDesignation = false)
        {
            return string.Format(DisplayDateFormat, date) + (includeTimeZoneDesignation ? " MT" : "");
        }

        public static string ToDisplayDate(this DateTime? date, bool includeTimeZoneDesignation = false)
        {
            if (!date.HasValue)
            {
                return "";
            }

            return date.Value.ToDisplayDate(includeTimeZoneDesignation);
        }

        public static string ContactToDisplayString(string lastName, string firstName)
        {
            return firstName + " " + lastName;
        }

        public static string ContactFirstNameFromDisplayString(this string contact)
        {
            return ContactNameFromDisplayString(contact, ContactName.FirstName);
        }

        public static string ContactLastNameFromDisplayString(this string contact)
        {
            return ContactNameFromDisplayString(contact, ContactName.LastName);
        }

        private static string ContactNameFromDisplayString(string contact, ContactName namePart)
        {
            if (!string.IsNullOrWhiteSpace(contact))
            {
                var contactArray = contact.Trim().Split(' ');

                if (contactArray.Length == 1)
                {
                    switch (namePart)
                    {
                        case ContactName.FirstName:
                            return contactArray[0].Trim();
                        case ContactName.LastName:
                            return "";
                    }
                }
                else if (contactArray.Length > 1)
                {
                    switch (namePart)
                    {
                        case ContactName.FirstName:
                            return contactArray[0].Trim();
                        case ContactName.LastName:
                            return contact.Substring(contactArray[0].Trim().Length).Trim();
                    }
                }

            }

            return "";
        }

        private enum ContactName
        {
            LastName,
            FirstName
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (source == null)
            {
                return false;
            }

            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Exports the supplied dataset to csv with the supplied schema
        /// </summary>
        /// <typeparam name="T">Any Class</typeparam>
        /// <param name="dataset">A List of type T</param>
        /// <param name="schema">Schema defined from the UI</param>
        public static string ExportToCsv<T>(IEnumerable<T> dataset, CsvExportSchema schema) where T : class
        {
            string result = string.Empty;

            if (dataset == null)
            {
                throw new ArgumentNullException("dataset");
            }

            if (schema == null || !schema.IsDefined)
            {
                throw new ArgumentNullException("schema");
            }

            if (dataset.Count() > 0)
            {
                string delimiter = SafeGetAppConfigSetting("CsvDelimiter", ",");
                var csvExporter = dataset.GetExporter(delimiter);

                foreach (var column in schema.Columns)
                {
                    Expression<Func<T, Object>> expression = null;
                    
                    if (!string.IsNullOrEmpty(column.Property))
                    {
                        expression = GetPropertyExpression<T>(column.Property);
                    }

                    csvExporter = csvExporter.AddExportableColumn(expression, column.ColumnHeader);
                }

                using (StringWriter writer = new StringWriter())
                {
                    csvExporter.AsCSVString(writer);
                    result = writer.ToString();
                }

                //using (StreamWriter writer = new StreamWriter(GetCsvExportFilenamePath(schema.FileId)))
                //{
                //    csvExporter.AsCSVString(writer);
                //}
            }

            return result;
        }

        /// <summary>
        /// Returns a linq expression similar to x => x.[propertyName]
        /// </summary>
        /// <typeparam name="T">Any Class</typeparam>
        /// <param name="propertyName">String name of property</param>
        /// <returns>Linq Expression</returns>
        public static Expression<Func<T, Object>> GetPropertyExpression<T>(string propertyName) where T : class
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(T), "x");
            MemberExpression memberExpression = null;

            if (propertyName.Contains("."))
            {
                var infoParts = propertyName.Split('.');
                var classPropInfo = typeof(T).GetProperty(infoParts[0]);
                memberExpression = Expression.MakeMemberAccess(paramExpression, classPropInfo);

                var propInfo = classPropInfo.PropertyType.GetProperty(infoParts[1]);
                memberExpression = Expression.MakeMemberAccess(memberExpression, propInfo);
            }
            else
            {
                PropertyInfo propInfo = typeof(T).GetProperty(propertyName);
                memberExpression = Expression.MakeMemberAccess(paramExpression, propInfo);
            }

            if (memberExpression.Type.IsValueType)
            {
                Expression conversion = Expression.Convert(memberExpression, typeof(Object));
                return Expression.Lambda<Func<T, Object>>(conversion, paramExpression);
            }

            return Expression.Lambda<Func<T, Object>>(memberExpression, paramExpression);
        }

        public static string FormatUserNameForDisplay(string lastName, string firstName, string email, bool enabled, bool showEmail = true)
        {
            if (lastName == null)
            {
                lastName = "";
            }

            if (firstName == null)
            {
                firstName = "";
            }

            return string.Format("{0}{1}{2}{3}",
                firstName.Trim(),
                string.IsNullOrEmpty(lastName) ? "" : " " + lastName.Trim(),
                !string.IsNullOrWhiteSpace(email) && showEmail ? " (" + email + ")" : "",
                enabled ? "" : " [Disabled]");
        }

        public static string FormatReferenceValueNameForDisplay(string name, bool enabled, bool formatAmpersand = true)
        {
            return string.Format("{0}{1}",
                (formatAmpersand ? name.Replace("&", "&amp;") : name),
                enabled ? "" : " [Disabled]");
        }
    }
}
