using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Suncor.LessonsLearnedMP.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace Suncor.LessonsLearnedMP.Web.Helpers
{
    public static class HtmlHelpers
    {
        public static Dictionary<Enumerations.JavaScript, string> DebugScripts = new Dictionary<Enumerations.JavaScript, string>()
        {
            { Enumerations.JavaScript.Jquery, "jquery-1.7.1.js" },
            { Enumerations.JavaScript.JqueryUi, "jquery-ui-1.8.20.custom.min.js" },
            { Enumerations.JavaScript.JqueryBgiframe, "jquery.bgiframe-2.1.2.js" },
            { Enumerations.JavaScript.JqueryForm, "jquery.form.yui.js" },
            { Enumerations.JavaScript.JqueryLivequery, "jquery.livequery.js" },
            { Enumerations.JavaScript.JqueryValidate, "jquery.validate.js" },
            { Enumerations.JavaScript.Json2, "json2.js" },
            { Enumerations.JavaScript.LessonsLearnedCommon, "lessons-learned-common.yui.js" },
            { Enumerations.JavaScript.LessonsLearnedSearch, "lessons-learned-search.yui.js" },
            { Enumerations.JavaScript.LessonsLearnedEdit, "lessons-learned-edit.yui.js" },
            { Enumerations.JavaScript.LessonsLearnedLessonIndex, "lessons-learned-lesson-index.yui.js" },
            { Enumerations.JavaScript.LessonsLearnedAdmin, "lessons-learned-admin.yui.js" },
            { Enumerations.JavaScript.LessonsLearnedDashboard, "lessons-learned-dashboard.yui.js" },
            { Enumerations.JavaScript.JqueryDataTables, "jquery.dataTables.yui.js" },
            { Enumerations.JavaScript.Date, "date.min.js" },
            { Enumerations.JavaScript.JqueryContextMenu, "jquery.contextMenu.yui.js" },
            { Enumerations.JavaScript.Bootstrap, "bootstrap.js" },
            { Enumerations.JavaScript.JquerySelectMenu, "jquery.ui.selectmenu.yui.js" },
            { Enumerations.JavaScript.Pie, "PIE.js" },
            { Enumerations.JavaScript.JqueryJqplot, "jquery.jqplot.yui.js" },
            { Enumerations.JavaScript.JqueryJqplot_PieRenderer, "jqplot.plugins/jqplot.pieRenderer.js" },
            { Enumerations.JavaScript.JqueryJqplot_BarRenderer, "jqplot.plugins/jqplot.barRenderer.js" },
            { Enumerations.JavaScript.JqueryJqplot_CategoryAxisRenderer, "jqplot.plugins/jqplot.categoryAxisRenderer.js" },
            { Enumerations.JavaScript.JqueryJqplot_PointLabels, "jqplot.plugins/jqplot.pointLabels.js" },
            { Enumerations.JavaScript.JqueryJqplot_Highlighter, "jqplot.plugins/jqplot.highlighter.js" },
            { Enumerations.JavaScript.JqueryJqplot_DateAxisRenderer, "jqplot.plugins/jqplot.dateAxisRenderer.js" },
            { Enumerations.JavaScript.JqueryJqplot_Cursor, "jqplot.plugins/jqplot.cursor.js" },
            { Enumerations.JavaScript.Excanvas, "excanvas.min.js" },
            { Enumerations.JavaScript.JqueryMultiselect, "jquery.multiselect.yui.js" },
            { Enumerations.JavaScript.JqueryTooltip, "jquery.tooltip.js" }
        };

        public static Dictionary<Enumerations.JavaScript, string> ReleaseScripts = new Dictionary<Enumerations.JavaScript, string>()
        {
            { Enumerations.JavaScript.Jquery, "jquery-1.7.1.min.js" },
            { Enumerations.JavaScript.JqueryUi, "jquery-ui-1.8.20.custom.min.js" },
            { Enumerations.JavaScript.JqueryBgiframe, "jquery.bgiframe-2.1.2.min.js" },
            { Enumerations.JavaScript.JqueryForm, "jquery.form.min.js" },
            { Enumerations.JavaScript.JqueryLivequery, "jquery.livequery.min.js" },
            { Enumerations.JavaScript.JqueryValidate, "jquery.validate.min.js" },
            { Enumerations.JavaScript.Json2, "json2.min.js" },
            { Enumerations.JavaScript.LessonsLearnedCommon, "lessons-learned-common.min.js" },
            { Enumerations.JavaScript.LessonsLearnedSearch, "lessons-learned-search.min.js" },
            { Enumerations.JavaScript.LessonsLearnedEdit, "lessons-learned-edit.min.js" },
            { Enumerations.JavaScript.LessonsLearnedLessonIndex, "lessons-learned-lesson-index.min.js" },
            { Enumerations.JavaScript.LessonsLearnedAdmin, "lessons-learned-admin.min.js" },
            { Enumerations.JavaScript.LessonsLearnedDashboard, "lessons-learned-dashboard.min.js" },
            { Enumerations.JavaScript.JqueryDataTables, "jquery.dataTables.min.js" },
            { Enumerations.JavaScript.Date, "date.min.js" },
            { Enumerations.JavaScript.JqueryContextMenu, "jquery.contextMenu.min.js" },
            { Enumerations.JavaScript.Bootstrap, "bootstrap.min.js" },
            { Enumerations.JavaScript.JquerySelectMenu, "jquery.ui.selectmenu.min.js" },
            { Enumerations.JavaScript.Pie, "PIE.js" },
            { Enumerations.JavaScript.JqueryJqplot, "jquery.jqplot.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_PieRenderer, "jqplot.plugins/jqplot.pieRenderer.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_BarRenderer, "jqplot.plugins/jqplot.barRenderer.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_CategoryAxisRenderer, "jqplot.plugins/jqplot.categoryAxisRenderer.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_PointLabels, "jqplot.plugins/jqplot.pointLabels.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_Highlighter, "jqplot.plugins/jqplot.highlighter.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_DateAxisRenderer, "jqplot.plugins/jqplot.dateAxisRenderer.min.js" },
            { Enumerations.JavaScript.JqueryJqplot_Cursor, "jqplot.plugins/jqplot.cursor.min.js" },
            { Enumerations.JavaScript.Excanvas, "excanvas.min.js" },
            { Enumerations.JavaScript.JqueryMultiselect, "jquery.multiselect.min.js" },
            { Enumerations.JavaScript.JqueryTooltip, "jquery.tooltip.min.js" }
        };

        public static Dictionary<Enumerations.Css, string> DebugCss = new Dictionary<Enumerations.Css, string>()
        {
            { Enumerations.Css.JqueryUi, "themes/cupertino/jquery-ui-1.8.20.custom.chirp.css" },
            { Enumerations.Css.JqueryUiInverse, "themes/inverse-theme/jquery-ui-1.8.20.custom.chirp.css" },
            { Enumerations.Css.Site, "Site.chirp.css" },
            { Enumerations.Css.LeftRightList, "LeftRightList.chirp.css" },
            { Enumerations.Css.DataTables, "datatables/css/demo_table.css" },
            { Enumerations.Css.JqueryContextMenu, "jquery.contextMenu.css" },
            { Enumerations.Css.Bootstrap, "bootstrap.css" },
            { Enumerations.Css.JquerySelectMenu, "themes/cupertino/jquery.ui.selectmenu.css" },
            { Enumerations.Css.JqueryJqplot, "jquery.jqplot.chirp.css" },
            { Enumerations.Css.JqueryMultiselect, "jquery.multiselect.css" },
            { Enumerations.Css.JqueryTooltip, "jquery.tooltip.css" }
        };

        public static Dictionary<Enumerations.Css, string> ReleaseCss = new Dictionary<Enumerations.Css, string>()
        {
            { Enumerations.Css.JqueryUi, "themes/cupertino/jquery-ui-1.8.20.custom.min.css" },
            { Enumerations.Css.JqueryUiInverse, "themes/inverse-theme/jquery-ui-1.8.20.custom.min.css" },
            { Enumerations.Css.Site, "Site.min.css" },
            { Enumerations.Css.LeftRightList, "LeftRightList.min.css" },
            { Enumerations.Css.DataTables, "datatables/css/demo_table.css" },
            { Enumerations.Css.JqueryContextMenu, "jquery.contextMenu.css" },
            { Enumerations.Css.Bootstrap, "bootstrap.min.css" },
            { Enumerations.Css.JquerySelectMenu, "themes/cupertino/jquery.ui.selectmenu.css" },
            { Enumerations.Css.JqueryJqplot, "jquery.jqplot.min.css" },
            { Enumerations.Css.JqueryMultiselect, "jquery.multiselect.css" },
            { Enumerations.Css.JqueryTooltip, "jquery.tooltip.css" }
        };

        /// <summary>
        /// Return the Current Version from the AssemblyInfo.cs file.
        /// </summary>
        public static string ProductVersion()
        {
            try
            {
                System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
            catch (Exception)
            {
                return "?.?.?.?";
            }
        }

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> anonymousDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }

        public static string GetControlId<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return htmlHelper.GetFullHtmlFieldId(expression);
        }

        public static IViewComponentResult LeftRightListBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            string controlId = "";
            return htmlHelper.LeftRightListBoxForAuth(expression, selectList, Enumerations.Role.User, htmlAttributes, out controlId);
        }

        public static IViewComponentResult LeftRightListBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, out string controlId)
        {
            return htmlHelper.LeftRightListBoxForAuth(expression, selectList, Enumerations.Role.User, htmlAttributes, out controlId);
        }

        public static IViewComponentResult LeftRightListBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> leftExpression, Expression<Func<TModel, TProperty>> rightExpression, IEnumerable<SelectListItem> enabledList, IEnumerable<SelectListItem> disabledList, object htmlAttributes)
        {
            return htmlHelper.LeftRightListBoxForAuth(leftExpression, rightExpression, enabledList, disabledList, Enumerations.Role.User, htmlAttributes);
        }

        public static IViewComponentResult LeftRightListBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, string leftLabel, string rightLabel, bool allowSorting)
        {
            string controlId = "";
            return htmlHelper.LeftRightListBoxForAuth(expression: expression, selectList: selectList, requiredPrivilege: Enumerations.Role.User, htmlAttributes: htmlAttributes, controlId: out controlId, leftLabel: leftLabel, rightLabel: rightLabel, allowSorting: allowSorting);
        }

        public static IViewComponentResult LeftRightListBoxFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, string leftLabel, string rightLabel, string leftTitle, string rightTitle, bool allowSorting)
        {
            string controlId = "";
            return htmlHelper.LeftRightListBoxForAuth(expression: expression, selectList: selectList, requiredPrivilege: Enumerations.Role.User, htmlAttributes: htmlAttributes, controlId: out controlId, leftLabel: leftLabel, rightLabel: rightLabel, leftTitle: leftTitle, rightTitle: rightTitle, allowSorting: allowSorting);
        }

        public static IHtmlContent Script(this IHtmlHelper helper, Enumerations.JavaScript script)
        {
            var urlHelper = new UrlHelper(helper.ViewContext);

            string source = "";

            #if DEBUG
                source = DebugScripts[script];
            #else
                source = ReleaseScripts[script];
            #endif

            //Append the content path if this isn't an http address
            if (!source.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                source = source.Insert(0, urlHelper.Content("~/Scripts/")) + "?r=" + ProductVersion();
            }

            TagBuilder scriptTag = new TagBuilder("script");
            scriptTag.Attributes.Add("type", "text/javascript");
            scriptTag.Attributes.Add("src", source);

            return scriptTag;
        }

        public static IHtmlContent Script(this IHtmlHelper helper, Enumerations.Css css)
        {
            var urlHelper = new UrlHelper(helper.ViewContext);

            string source = "";

            #if DEBUG
                source = DebugCss[css];
            #else
                source = ReleaseCss[css];
            #endif

            //Append the content path if this isn't an http address
            if (!source.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                source = source.Insert(0, urlHelper.Content("~/Content/")) + "?r=" + ProductVersion();
            }

            TagBuilder scriptTag = new TagBuilder("link");
            scriptTag.Attributes.Add("type", "text/css");
            scriptTag.Attributes.Add("href", source);
            scriptTag.Attributes.Add("rel", "Stylesheet");

            return scriptTag;
        }

        public static IHtmlContent Raw(this IHtmlContent htmlString)
        {
            return new HtmlString(HttpUtility.HtmlDecode(htmlString.ToString()));
        }

        public static string ToJson(this object o)
        {
            var json = JsonConvert.SerializeObject(o);
            return json;
        }

        public static string ToJson(this Dictionary<string,string> item)
        {
            var json = JsonConvert.SerializeObject(item).Replace("\"", "'");
            return json;
        }

        public static T FromJson<T>(this string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }
        /// <summary>
        /// JSON Deserialization
        /// </summary>
        public static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }

        /// <summary>
        /// Gets a row of paging links
        /// </summary>
        /// <param name="currentPage">0 based page index</param>
        /// <param name="totalPages">1 based total pages in dataset</param>
        /// <param name="threshold">1 based count of page numbers to return, from the middle of current page</param>
        /// <returns>HTML string of paging links</returns>
        public static IHtmlContent GetPagingNumbers(int currentPage, int totalPages, int pageSize, int threshold, int totalRecords)
        {
            currentPage++;
            int range = threshold / 2;
            int lower = currentPage;
            int upper =  (currentPage + range) > totalPages ? totalPages : currentPage + range;

            if (currentPage < range)
            {
                lower = 1;
                upper = threshold > totalPages ? totalPages : threshold;
            }
            else if ((currentPage + range) > totalPages)
            {
                lower = 1;

                if (totalPages - threshold > 0)
                {
                    lower = totalPages - threshold;
                    upper = totalPages;
                }
            }
            else
            {
                lower = currentPage - range == 0 ? 1 : currentPage - range;
            }

            StringBuilder result = new StringBuilder();

            for (int i = lower; i <= upper; i++)
            {
                if (i == currentPage)
                {
                    result.AppendFormat("<div class='selected-pager ui-state-error ui-corner-all'><a href='#' class='page-button link' pageNumber='{0}'>{0}</a></div>", i);
                }
                else
                {
                    result.AppendFormat("<a href='#' class='page-button link' pageNumber='{0}'>{0}</a>", i);
                }
            }

            //if(totalRecords == Utils.MaxPagingCountStageOneQuery || upper != totalPages)
            //{
            //    result.AppendFormat("<a href='#' class='page-button link' pageNumber='{0}'>...</a>", upper + 1);
            //}

            if (lower > 1)
            {
                result.Insert(0, string.Format("<a href='#' class='page-button link' pageNumber='{0}'>...</a>", lower - 1));
            }

            return new HtmlString(result.ToString());
        }
        
        /*

        /// <summary>
        /// Takes a relative or absolute url and returns the fully-qualified url path.
        /// </summary>
        /// <param name="text">The url to make fully-qualified. Ex: Home/About</param>
        /// <returns>The absolute url plus protocol, server, & port. Ex: http://localhost:1234/Home/About</returns>
        public static string ToFullyQualifiedUrl(string text)
        {
            //### the VirtualPathUtility doesn"t handle querystrings, so we break the original url up
            var oldUrl = text;
            var oldUrlArray = (oldUrl.Contains("?") ? oldUrl.Split('?') : new[] { oldUrl, "" });

            //### we"ll use the Request.Url object to recreate the current server request"s base url
            //### requestUri.AbsoluteUri = "http://domain.com:1234/Home/Index?page=123"
            //### requestUri.LocalPath = "/Home/Index"
            //### requestUri.Query = "?page=123"
            //### subtract LocalPath and Query from AbsoluteUri and you get "http://domain.com:1234", which is urlBase
            var requestUri = HttpContext.Current.Request.Url;
            var localPathAndQuery = requestUri.LocalPath + requestUri.Query;
            var urlBase = requestUri.AbsoluteUri.Substring(0, requestUri.AbsoluteUri.Length - localPathAndQuery.Length);

            //### convert the request url into an absolute path, then reappend the querystring, if one was specified
            var newUrl = VirtualPathUtility.ToAbsolute(oldUrlArray[0]);
            if (!string.IsNullOrEmpty(oldUrlArray[1]))
                newUrl += "?" + oldUrlArray[1];

            //### combine the old url base (protocol + server + port) with the new local path
            return urlBase + newUrl;
        }*/

        public static IHtmlContent NullableBoolOptionsetFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string trueLabel, string falseLabel, string nullLabel)
        {
            return NullableBoolOptionsetFor(htmlHelper, expression, trueLabel, falseLabel, nullLabel, null);
        }

        public static IHtmlContent NullableBoolOptionsetFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string trueLabel, string falseLabel, string nullLabel, object htmlAttributes)
        {
            ModelExpression modelExpression = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);

            var metadata = modelExpression.Metadata;

            if (metadata.ModelType != typeof(bool?))
            {
                throw new ArgumentException("expression property must be of type [bool?]", "expression");
            }

            var htmlOptions = new RouteValueDictionary(htmlAttributes);

            if (htmlOptions.ContainsKey("checked"))
            {
                htmlOptions.Remove("checked");
            }

            var trueOption = htmlHelper.RadioButtonFor(expression, true, htmlAttributes);
            var falseOption = htmlHelper.RadioButtonFor(expression, false, htmlAttributes);
            var nullOption = htmlHelper.RadioButtonFor(expression, "", htmlAttributes);
            var trueOptionLabel = htmlHelper.LabelFor(expression, trueLabel);
            var falseOptionLabel = htmlHelper.LabelFor(expression, falseLabel);
            var nullOptionLabel = htmlHelper.LabelFor(expression, nullLabel);

            htmlOptions.Add("checked", "checked");

            if ((bool?)modelExpression.Model == true)
            {
                trueOption = htmlHelper.RadioButtonFor(expression, true, htmlOptions);
            }
            else if ((bool?)modelExpression.Model == false)
            {
                falseOption = htmlHelper.RadioButtonFor(expression, false, htmlOptions);
            }
            else
            {
                nullOption = htmlHelper.RadioButtonFor(expression, "", htmlOptions);
            }

            var html = string.Format("<ul><li>{0}{1}</li><li>{2}{3}</li><li>{4}{5}</li></ul>",
                trueOption, trueOptionLabel,
                falseOption, falseOptionLabel,
                nullOption, nullOptionLabel);
            return new HtmlString(html);
        }

        public static IHtmlContent SpanFor<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes, string formatString = null)
        {
            ModelExpression modelExpression = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);

            var metadata = modelExpression.Metadata;
            var htmlOptions = new RouteValueDictionary(htmlAttributes);
            string fullName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(metadata.PropertyName); 

            TagBuilder span = new TagBuilder("span");
            span.MergeAttributes(htmlOptions);
            span.MergeAttribute("name", fullName, true);
            var value = modelExpression.Model;

            if (value != null)
            {
                string stringValue = value.ToString();
                if (formatString != null)
                {
                    stringValue = string.Format(formatString, value);
                }

                span.InnerHtml.Append(stringValue);
            }
            span.GenerateId(fullName,"_");

            return span;
        }

        public static IHtmlContent Span(this IHtmlHelper htmlHelper, object value, object htmlAttributes)
        {
            var htmlOptions = new RouteValueDictionary(htmlAttributes);

            TagBuilder span = new TagBuilder("span");
            span.MergeAttributes(htmlOptions);
            if (value != null)
            {
                span.InnerHtml.Append(value.ToString());
            }
            return span;
        }

        public static string GetInputName<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCallExpression = (MethodCallExpression)expression.Body;
                string name = GetInputName(methodCallExpression);
                return name.Substring(expression.Parameters[0].Name.Length + 1);

            }
            return expression.Body.ToString().Substring(expression.Parameters[0].Name.Length + 1);
        }

        private static string GetInputName(MethodCallExpression expression)
        {
            // p => p.Foo.Bar().Baz.ToString() => p.Foo OR throw...

            MethodCallExpression methodCallExpression = expression.Object as MethodCallExpression;
            if (methodCallExpression != null)
            {
                return GetInputName(methodCallExpression);
            }
            return expression.Object.ToString();
        }

        public static List<SelectListItem> ToSelectList<T>(
         this IEnumerable<T> enumerable,
         Func<T, string> text,
         Func<T, string> value,
         string defaultOption)
        {
            var items = enumerable.Select(f => new SelectListItem()
            {
                Text = text(f),
                Value = value(f)
            }).ToList();
            items.Insert(0, new SelectListItem()
            {
                Text = defaultOption,
                Value = "-1"
            });
            return items;
        }

        public static string ToPublicUrl(this IUrlHelper urlHelper, Uri relativeUri)
        {
            string path = urlHelper.Content(string.Format("~/{0}", relativeUri.ToString()));
            return path;
            /*
            var httpContext = urlHelper.RequestContext.HttpContext;

            var uriBuilder = new UriBuilder
            {
                Host = httpContext.Request.Url.Host,
                Path = "/",
                Port = 80,
                Scheme = "http",
            };

            if (httpContext.Request.IsLocal)
            {
                uriBuilder.Port = httpContext.Request.Url.Port;
            }

            return new Uri(uriBuilder.Uri, relativeUri).AbsoluteUri;*/
            
        }
    }
}
