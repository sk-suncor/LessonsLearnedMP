using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;
using System.Xml;

namespace Suncor.LessonsLearnedMP.Framework
{
    namespace FluentListExporterColumns
    {

        public static class IEnumerableExtensions
        {
            /// <summary>
            /// Exporter extension method for all IEnumerableOfT
            /// </summary>
            public static FluentExporter<T> GetExporter<T>(
                this IEnumerable<T> source, String seperator = ",") where T : class
            {
                return new FluentExporter<T>(source, seperator);
            }
        }

        /// <summary>
        /// Represents custom exportable column with a expression for the property name
        /// and a custom format string
        /// </summary>
        public class ExportableColumn<T>
        {
            public Expression<Func<T, Object>> Func { get; private set; }
            public String HeaderString { get; private set; }
            public String CustomFormatString { get; private set; }

            public ExportableColumn(
                Expression<Func<T, Object>> func,
                String headerString = "",
                String customFormatString = "")
            {
                this.Func = func;
                this.HeaderString = headerString;
                this.CustomFormatString = customFormatString;
            }
        }

        /// <summary>
        /// Exporter that uses Expression tree parsing to work out what values to export for 
        /// columns, and will use additional data as specified in the List of ExportableColumn
        /// which defines whethere to use custom headers, or formatted output
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class FluentExporter<T> where T : class
        {
            private List<ExportableColumn<T>> columns = new List<ExportableColumn<T>>();
            private Dictionary<Expression<Func<T, Object>>, Func<T, Object>> compiledFuncLookup =
                new Dictionary<Expression<Func<T, Object>>, Func<T, Object>>();
            private List<String> headers = new List<String>();
            private IEnumerable<T> sourceList;
            private String seperator;
            private bool doneHeaders;

            public FluentExporter(IEnumerable<T> sourceList, String seperator = ",")
            {
                this.sourceList = sourceList;
                this.seperator = seperator;
            }

            public FluentExporter<T> AddExportableColumn(
                Expression<Func<T, Object>> func,
                String headerString = "",
                String customFormatString = "")
            {
                columns.Add(new ExportableColumn<T>(func, headerString, customFormatString));
                return this;
            }

            /// <summary>
            /// Export all specified columns as a string, 
            /// using seperator and column data provided
            /// where we may use custom or default headers 
            /// (depending on whether a custom header string was supplied)
            /// where we may use custom fomatted column data or default data 
            /// (depending on whether a custom format string was supplied)
            /// </summary>
            public void AsCSVString(TextWriter writer)
            {
                if (columns.Count == 0)
                    throw new InvalidOperationException(
                        "You need to specify at least one column to export value");

                int i = 0;
                foreach (T item in sourceList)
                {
                    List<String> values = new List<String>();
                    foreach (ExportableColumn<T> exportableColumn in columns)
                    {
                        if (!doneHeaders)
                        {
                            if (String.IsNullOrEmpty(exportableColumn.HeaderString))
                            {
                                headers.Add(exportableColumn.Func == null ? "" : GetPropertyName(exportableColumn.Func));
                            }
                            else
                            {
                                headers.Add(exportableColumn.HeaderString);
                            }

                            if (exportableColumn.Func == null)
                            {
                                values.Add("");
                            }

                            else
                            {
                                Func<T, Object> func = exportableColumn.Func.Compile();
                                compiledFuncLookup.Add(exportableColumn.Func, func);
                                if (!String.IsNullOrEmpty(exportableColumn.CustomFormatString))
                                {
                                    var value = func(item);
                                    values.Add(value != null ?
                                        String.Format(exportableColumn.CustomFormatString, "\"" + value.ToString() + "\"") : "");

                                }
                                else
                                {
                                    var value = func(item);
                                    values.Add(value != null ? "\"" + value.ToString() + "\"" : "");
                                }
                            }
                        }
                        else
                        {
                            if (exportableColumn.Func == null)
                            {
                                values.Add("");
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(exportableColumn.CustomFormatString))
                                {
                                    var value = compiledFuncLookup[exportableColumn.Func](item);
                                    values.Add(value != null ?
                                        String.Format(exportableColumn.CustomFormatString, "\"" + value.ToString()) + "\"" : "");
                                }
                                else
                                {
                                    var value = compiledFuncLookup[exportableColumn.Func](item);
                                    values.Add(value != null ? "\"" + value.ToString() + "\"" : "");
                                }
                            }
                        }
                    }
                    if (!doneHeaders)
                    {
                        writer.WriteLine(headers.Aggregate((start, end) => start + seperator + end));
                        doneHeaders = true;
                    }

                    writer.WriteLine(values.Aggregate((start, end) => start + seperator + end));
                }

            }

            /// <summary>
            /// Export all specified columns as a XML string, using column data provided
            /// and use custom headers depending on whether a custom header string was supplied.
            /// Use custom formatted column data or default data depending on whether a custom format string was supplied.
            /// </summary>
            public void ToXML(XmlTextWriter writer)
            {
                if (columns.Count == 0)
                    throw new InvalidOperationException("You need to specify at least one element to export value");

                foreach (T item in sourceList)
                {
                    List<String> values = new List<String>();
                    foreach (ExportableColumn<T> exportableColumn in columns)
                    {
                        if (!doneHeaders)
                        {
                            if (String.IsNullOrEmpty(exportableColumn.HeaderString))
                            {
                                headers.Add(MakeXMLNameLegal(GetPropertyName(exportableColumn.Func)));
                            }
                            else
                            {
                                headers.Add(MakeXMLNameLegal(exportableColumn.HeaderString));
                            }

                            Func<T, Object> func = exportableColumn.Func.Compile();
                            compiledFuncLookup.Add(exportableColumn.Func, func);
                            if (!String.IsNullOrEmpty(exportableColumn.CustomFormatString))
                            {
                                var value = func(item);
                                values.Add(value != null ?
                                    String.Format(exportableColumn.CustomFormatString, value.ToString()) : "");

                            }
                            else
                            {
                                var value = func(item);
                                values.Add(value != null ? value.ToString() : "");
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(exportableColumn.CustomFormatString))
                            {
                                var value = compiledFuncLookup[exportableColumn.Func](item);
                                values.Add(value != null ?
                                    String.Format(exportableColumn.CustomFormatString, value.ToString()) : "");
                            }
                            else
                            {
                                var value = compiledFuncLookup[exportableColumn.Func](item);
                                values.Add(value != null ? value.ToString() : "");
                            }
                        }
                    }
                    if (!doneHeaders)
                    {
                        writer.Formatting = Formatting.Indented;
                        writer.WriteStartDocument(true);
                        writer.WriteProcessingInstruction("xml-stylesheet", "type='text/xsl' href='dump.xsl'");
                        writer.WriteComment("List Exporter dump");

                        // Write main document node and document properties
                        writer.WriteStartElement("dump");
                        writer.WriteAttributeString("date", DateTime.Now.ToString());

                        doneHeaders = true;
                    }

                    writer.WriteStartElement("item");
                    for (int i = 0; i < values.Count; i++)
                    {
                        writer.WriteStartElement(headers[i]);
                        writer.WriteString(values[i]);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }

            /// <summary>
            /// Export to file, using the AsCSVString() method to supply the exportable data
            /// </summary>
            public void WhichIsExportedToFileLocation(StreamWriter fileWriter)
            {
                AsCSVString(fileWriter);
            }

            /// <summary>
            /// Gets a Name from an expression tree that is assumed to be a
            /// MemberExpression
            /// </summary>
            private static string GetPropertyName<T>(
                Expression<Func<T, Object>> propertyExpression)
            {
                var lambda = propertyExpression as LambdaExpression;
                MemberExpression memberExpression;
                if (lambda.Body is UnaryExpression)
                {
                    var unaryExpression = lambda.Body as UnaryExpression;
                    memberExpression = unaryExpression.Operand as MemberExpression;
                }
                else
                {
                    memberExpression = lambda.Body as MemberExpression;
                }

                var propertyInfo = memberExpression.Member as PropertyInfo;

                return propertyInfo.Name;
            }

            private string MakeXMLNameLegal(string aString)
            {
                StringBuilder newName = new StringBuilder();

                if (!char.IsLetter(aString[0]))
                    newName.Append("_");

                // Must start with a letter or underscore.
                for (int i = 0; i <= aString.Length - 1; i++)
                {
                    if (char.IsLetter(aString[i]) || char.IsNumber(aString[i]))
                    {
                        newName.Append(aString[i]);
                    }
                    else
                    {
                        newName.Append("_");
                    }
                }
                return newName.ToString();
            }
        }
    }
}
