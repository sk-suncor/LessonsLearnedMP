using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Suncor.LessonsLearnedMP.Framework;
using Suncor.LessonsLearnedMP.Web.Common;
using Suncor.LessonsLearnedMP.Web.ViewData.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Suncor.LessonsLearnedMP.Web.Helpers
{
    public static class AuthHtmlHelpers
    {
        /// <summary>
        /// Allows or blocks the display of an action link depending on user authorization level.
        /// </summary>
        public static IHtmlContent ActionLinkAuth(this IHtmlHelper htmlHelper, Enumerations.Role requiredPrivilege, Enumerations.AuthFailedBehaviour authFailedBehaviour, string linkText, string actionName, bool requiresFullAccess = false)
        {
            return ActionLinkAuth(htmlHelper, requiredPrivilege, authFailedBehaviour, linkText, actionName, null, null, null, null);
        }

        /// <summary>
        /// Allows or blocks the display of an action link depending on user authorization level.
        /// </summary>
        public static IHtmlContent ActionLinkAuth(this IHtmlHelper htmlHelper, Enumerations.Role requiredPrivilege, Enumerations.AuthFailedBehaviour authFailedBehaviour, string linkText, string actionName, object routeValues, bool requiresFullAccess = false)
        {
            return ActionLinkAuth(htmlHelper, requiredPrivilege, authFailedBehaviour, linkText, actionName, null, routeValues, null, null);
        }

        /// <summary>
        /// Allows or blocks the display of an action link depending on user authorization level.
        /// </summary>
        public static IHtmlContent ActionLinkAuth(this IHtmlHelper htmlHelper, Enumerations.Role requiredPrivilege, Enumerations.AuthFailedBehaviour authFailedBehaviour, string linkText, string actionName, string controllerName, bool requiresFullAccess = false)
        {
            return ActionLinkAuth(htmlHelper, requiredPrivilege, authFailedBehaviour, linkText, actionName, controllerName, null, null, null);
        }

        /// <summary>
        /// Allows or blocks the display of an action link depending on user authorization level.
        /// </summary>
        public static IHtmlContent ActionLinkAuth(this IHtmlHelper htmlHelper, Enumerations.Role requiredPrivilege, Enumerations.AuthFailedBehaviour authFailedBehaviour, string linkText, string actionName, object routeValues, object htmlAttributes, bool requiresFullAccess = false)
        {
            return ActionLinkAuth(htmlHelper, requiredPrivilege, authFailedBehaviour, linkText, actionName, null, routeValues, htmlAttributes, null);
        }

        /// <summary>
        /// Allows or blocks the display of an action link depending on user authorization level.
        /// </summary>
        public static IHtmlContent ActionLinkAuth(this IHtmlHelper htmlHelper,  Enumerations.Role requiredPrivilege, Enumerations.AuthFailedBehaviour authFailedBehaviour, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes, string alternateLinkText)
        {
            //If no link text, just return an empty string.
            if (string.IsNullOrEmpty(linkText))
            {
                return HtmlString.Empty;
            }

            // If user has no privileges refuse access
            bool hasAccess = new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(requiredPrivilege);

            var htmlOptions = new RouteValueDictionary(htmlAttributes);
            var routeOptions = new RouteValueDictionary(routeValues);

            //Remove disabled attribute if present
            if (htmlOptions.ContainsKey("disabled"))
            {
                htmlOptions.Remove("disabled");
            }

            IHtmlContent result = htmlHelper.ActionLink(linkText, actionName, controllerName, routeOptions, htmlOptions);

            if (!hasAccess)
            {
                switch (authFailedBehaviour)
                {
                    case Enumerations.AuthFailedBehaviour.AlternateLink:
                        if (string.IsNullOrEmpty(alternateLinkText))
                        {
                            throw new ArgumentNullException("alternateLinkText", "alternateLinkText cannot be null or empty");
                        }
                        result = htmlHelper.ActionLink(alternateLinkText, actionName, controllerName, routeOptions, htmlOptions);
                        break;
                    case Enumerations.AuthFailedBehaviour.EmptyString:
                        result = HtmlString.Empty;
                        break;
                    case Enumerations.AuthFailedBehaviour.InnerHtml:
                        result = htmlHelper.Span(linkText, htmlOptions);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("authFailedBehaviour");
                }
            }

            return result;
        }

        /// <summary>
        /// Allows or blocks the display of a checkbox depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="requiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent CheckBoxForAuth<TModel>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, object htmlAttributes, bool forceReadOnly = false)
        {
            // If user has no privileges refuse access
            bool fullAccess = new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(requiredPrivilege);

            string html = htmlHelper.CheckBoxFor(expression, htmlAttributes).ToString();

            if (!fullAccess)
            {
                ModelExpression modelExpression = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);
                var expressionValue = (bool)modelExpression.Model;

                switch (readOnlyBehaviour)
                {
                    case Enumerations.ReadOnlyBehaviour.CheckedIcon:
                        html = expressionValue ? "<span class='red-icon red-icon-check' />" : "";
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.Disabled:
                        var htmlOptions = new RouteValueDictionary(htmlAttributes);

                        if (!htmlOptions.ContainsKey("disabled"))
                        {
                            htmlOptions.Add("disabled", "disabled");
                        }
                        else
                        {
                            htmlOptions["disabled"] = "disabled";
                        }

                        html = htmlHelper.CheckBoxFor(expression, htmlOptions).ToString();
                        break;

                    case Enumerations.ReadOnlyBehaviour.EmptyString:
                        html = htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.InnerHtml:
                        html = htmlHelper.SpanFor(expression, htmlAttributes).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("readOnlyBehaviour");
                }
            }

            return new HtmlString(html);
        }

        /// <summary>
        /// Allows or blocks the display of a textbox depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="requiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent TextBoxForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, object htmlAttributes, string formatString = null, bool forceReadOnly = false)
        {
            return TextBoxForAuth(htmlHelper, expression, requiredPrivilege, readOnlyBehaviour, null, null, null, null, null, htmlAttributes, formatString, forceReadOnly);
        }

        /// <summary>
        /// Allows or blocks the display of a textbox depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="fieldRequiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="linkRequiredPrivilege">Privilege needed to navigate to the link destination</param>
        /// <param name="linkAuthFailedBehaviour">Behavior of the link text if not authorized</param>
        /// <param name="actionName">Action destination of the link</param>
        /// <param name="controllerName">Controller destination of the link</param>
        /// <param name="routeValues">Route values of the link</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">(Optional) Force the field into read-only mode regardless of authorization</param>
        /// /// <param name="formatString">(Optional) String used to format the value of the control</param>
        /// <returns></returns>
        public static IHtmlContent TextBoxForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Enumerations.Role fieldRequiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, Enumerations.Role? linkRequiredPrivilege, Enumerations.AuthFailedBehaviour? linkAuthFailedBehaviour, string actionName, string controllerName, object routeValues, object htmlAttributes, string formatString = null, bool forceReadOnly = false)
        {
            // If user has no privileges refuse access
            bool fullAccess = !forceReadOnly && new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(fieldRequiredPrivilege);

            string controlId = htmlHelper.GetControlId(expression);

            ModelExpression modelExpression = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);

            var expressionValue = modelExpression.Model;
            string stringValue = expressionValue == null ? "" : expressionValue.ToString();

            if (formatString != null)
            {
                stringValue = string.Format(formatString, expressionValue);
            }

            string html = htmlHelper.TextBox(controlId, stringValue, htmlAttributes).ToString();

            if (!fullAccess)
            {
                switch (readOnlyBehaviour)
                {
                    case Enumerations.ReadOnlyBehaviour.Disabled:
                        var htmlOptions = new RouteValueDictionary(htmlAttributes);

                        if (!htmlOptions.ContainsKey("disabled"))
                        {
                            htmlOptions.Add("disabled", "disabled");
                        }
                        else
                        {
                            htmlOptions["disabled"] = "disabled";
                        }

                        html = htmlHelper.TextBox(controlId + "__text", stringValue, htmlOptions).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.EmptyString:
                        html = htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.InnerHtml:
                        html = htmlHelper.SpanFor(expression, htmlAttributes, formatString).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.AuthorizationLink:
                        if (!linkAuthFailedBehaviour.HasValue || !linkRequiredPrivilege.HasValue)
                        {
                            throw new ArgumentNullException("linkAuthFailedBehaviour", "linkAuthFailedBehaviour or linkRequiredPrivilege cannot be null");
                        }
                        html = htmlHelper.ActionLinkAuth(linkRequiredPrivilege.Value, linkAuthFailedBehaviour.Value, stringValue, actionName, routeValues, htmlAttributes).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("readOnlyBehaviour");
                }
            }

            return new HtmlString(html);
        }

        /// <summary>
        /// Allows or blocks the display of a dropdownlist depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="selectList"></param>
        /// <param name="requiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent DropDownListForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour,  object htmlAttributes, bool forceReadOnly = false)
        {
            return DropDownListForAuth(htmlHelper,expression, selectList, null, requiredPrivilege, readOnlyBehaviour, null, null, null, null, null, htmlAttributes, forceReadOnly);
        }

        /// <summary>
        /// Allows or blocks the display of a dropdownlist depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="selectList"></param>
        /// <param name="optionLabel"></param>
        /// <param name="requiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent DropDownListForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string optionLabel, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, bool forceReadOnly = false)
        {
            return DropDownListForAuth(htmlHelper, expression, selectList, optionLabel, requiredPrivilege, readOnlyBehaviour, null, null, null, null, null, null, forceReadOnly);
        }

        /// <summary>
        /// Allows or blocks the display of a dropdownlist depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="selectList"></param>
        /// <param name="fieldRequiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="linkRequiredPrivilege">Privilege needed to navigate to the link destination</param>
        /// <param name="linkAuthFailedBehaviour">Behavior of the link text if not authorized</param>
        /// <param name="actionName">Action destination of the link</param>
        /// <param name="controllerName">Controller destination of the link</param>
        /// <param name="routeValues">Route values of the link</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent DropDownListForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, Enumerations.Role fieldRequiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, Enumerations.Role? linkRequiredPrivilege, Enumerations.AuthFailedBehaviour? linkAuthFailedBehaviour, string actionName, string controllerName, object routeValues, object htmlAttributes, bool forceReadOnly = false)
        {
            return DropDownListForAuth(htmlHelper, expression, selectList, null, fieldRequiredPrivilege, readOnlyBehaviour, linkRequiredPrivilege, linkAuthFailedBehaviour, actionName, controllerName, routeValues, htmlAttributes, forceReadOnly);
        }

        /// <summary>
        /// Allows or blocks the display of a dropdownlist depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="selectList"></param>
        /// <param name="optionLabel"></param>
        /// <param name="fieldRequiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="linkRequiredPrivilege">Privilege needed to navigate to the link destination</param>
        /// <param name="linkAuthFailedBehaviour">Behavior of the link text if not authorized</param>
        /// <param name="actionName">Action destination of the link</param>
        /// <param name="controllerName">Controller destination of the link</param>
        /// <param name="routeValues">Route values of the link</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent DropDownListForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, string optionLabel, Enumerations.Role fieldRequiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, Enumerations.Role? linkRequiredPrivilege, Enumerations.AuthFailedBehaviour? linkAuthFailedBehaviour, string actionName, string controllerName, object routeValues, object htmlAttributes, bool forceReadOnly = false)
        {
            // If user has no privileges refuse access
            bool fullAccess = !forceReadOnly && new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(fieldRequiredPrivilege);
            string controlId = htmlHelper.GetControlId(expression);

            string html = htmlHelper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes).ToString();

            if (!fullAccess)
            {
                var selected = selectList.Where(x => x.Selected).FirstOrDefault();

                switch (readOnlyBehaviour)
                {
                    case Enumerations.ReadOnlyBehaviour.Disabled:
                        var htmlOptions = new RouteValueDictionary(htmlAttributes);

                        if (!htmlOptions.ContainsKey("disabled"))
                        {
                            htmlOptions.Add("disabled", "disabled");
                        }
                        else
                        {
                            htmlOptions["disabled"] = "disabled";
                        }

                        html = htmlHelper.DropDownList(controlId + "__ddl", selectList, optionLabel, htmlOptions).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.EmptyString:
                        html = htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.InnerHtml:
                        html = htmlHelper.Span(selected == null ? "" : selected.Text, htmlAttributes).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.AuthorizationLink:
                        if (!linkAuthFailedBehaviour.HasValue || !linkRequiredPrivilege.HasValue)
                        {
                            throw new ArgumentNullException("linkAuthFailedBehaviour", "linkAuthFailedBehaviour or linkRequiredPrivilege cannot be null");
                        }
                        html = htmlHelper.ActionLinkAuth(linkRequiredPrivilege.Value, linkAuthFailedBehaviour.Value, selected == null ? "" : selected.Text, actionName, controllerName, routeValues, htmlAttributes, null).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("readOnlyBehaviour");
                }
            }

            return new HtmlString(html);
        }

        public static IHtmlContent TextAreaForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, bool forceReadOnly = false)
        {
            return TextAreaForAuth(htmlHelper, expression, requiredPrivilege, readOnlyBehaviour, null, forceReadOnly);
        }

        public static IHtmlContent TextAreaForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, object htmlAttributes, bool forceReadOnly = false)
        {
            // If user has no privileges refuse access
            bool fullAccess = new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(requiredPrivilege);
            string controlId = htmlHelper.GetControlId(expression);

            string html = htmlHelper.TextAreaFor(expression, htmlAttributes).ToString();

            if (!fullAccess)
            {
                ModelExpression modelExpression = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);
                var expressionValue = modelExpression.Model;

                switch (readOnlyBehaviour)
                {
                    case Enumerations.ReadOnlyBehaviour.Disabled:
                        var htmlOptions = new RouteValueDictionary(htmlAttributes);
                        
                        if (!htmlOptions.ContainsKey("disabled"))
                        {
                            htmlOptions.Add("disabled", "disabled");
                        }
                        else
                        {
                            htmlOptions["disabled"] = "disabled";
                        }

                        html = htmlHelper.TextArea(controlId + "__text", htmlOptions).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.EmptyString:
                        html = htmlHelper.HiddenFor(expression).ToString();
                        break;
                    case Enumerations.ReadOnlyBehaviour.InnerHtml:
                        html = htmlHelper.SpanFor(expression, htmlAttributes).ToString();
                        html += htmlHelper.HiddenFor(expression).ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("readOnlyBehaviour");
                }
            }

            return new HtmlString(html);
        }

        public static IViewComponentResult LeftRightListBoxForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, Enumerations.Role requiredPrivilege, object htmlAttributes)
        {
            string controlId = "";
            return LeftRightListBoxForAuth(htmlHelper, expression, selectList, requiredPrivilege, htmlAttributes, out controlId);
        }

        public static IViewComponentResult LeftRightListBoxForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, Enumerations.Role requiredPrivilege, object htmlAttributes, out string controlId, string leftLabel = "Enabled", string rightLabel = "Disabled", string leftTitle = "Enable", string rightTitle = "Disable", bool allowSorting = true)
        {
            // If user has no privileges refuse access
            bool fullAccess = new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(requiredPrivilege);

            //Get the name of the control
            string name = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlHelper.GetExpressionText(expression));
            controlId = htmlHelper.GetFullHtmlFieldId(expression);

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty", "expression");
            }

            IEnumerable<SelectListItem> rightList = (from s in selectList
                                                     where s.Selected
                                                     select s).ToList();

            IEnumerable<SelectListItem> leftList = (from s in selectList
                                                    where !s.Selected
                                                    select s).ToList();

            LeftRightListViewdata model = new LeftRightListViewdata
            {
                LeftList = leftList,
                RightList = rightList,
                LeftName = name + "__left",
                LeftId = controlId + "__left",
                RightName = name,
                RightId = controlId,
                HtmlAttributes = htmlAttributes,
                RequiredPrivilege = requiredPrivilege,
                AllowSorting = allowSorting,
                LeftLabel = leftLabel,
                RightLabel = rightLabel,
                LeftTitle = leftTitle,
                RightTitle = rightTitle
            };
            return (new LeftRightListBoxComponent()).InvokeAsync("LeftRightList",  new { list = model }).Result;
            
            //return htmlHelper.Action("LeftRightList", "Shared", new { list = model });
        }

        public static IViewComponentResult LeftRightListBoxForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> leftExpression, Expression<Func<TModel, TProperty>> rightExpression, IEnumerable<SelectListItem> enabledList, IEnumerable<SelectListItem> disableList, Enumerations.Role requiredPrivilege, object htmlAttributes, string leftLabel = "Enabled", string rightLabel = "Disabled", string leftTitle = "Enable", string rightTitle = "Disable", bool allowSorting = true)
        {
            // If user has no privileges refuse access
            bool fullAccess = new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(requiredPrivilege);

            //Get the name of the control
            string leftName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlHelper.GetExpressionText(leftExpression));
            string leftControlId = htmlHelper.GetFullHtmlFieldId(leftExpression);

            string rightName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlHelper.GetExpressionText(rightExpression));
            string rightControlId = htmlHelper.GetFullHtmlFieldId(rightExpression);

            if (String.IsNullOrEmpty(leftName))
            {
                throw new ArgumentException("Value cannot be null or empty", "expression");
            }

            IEnumerable<SelectListItem> rightList = disableList == null ? new SelectList(null) : disableList;

            IEnumerable<SelectListItem> leftList = enabledList == null ? new SelectList(null) : enabledList;

            LeftRightListViewdata model = new LeftRightListViewdata
            {
                LeftList = leftList,
                RightList = rightList,
                LeftName = leftName,
                LeftId = leftControlId,
                RightName = rightName,
                RightId = rightControlId,
                HtmlAttributes = htmlAttributes,
                RequiredPrivilege = requiredPrivilege,
                AllowSorting = allowSorting,
                LeftLabel = leftLabel,
                RightLabel = rightLabel,
                LeftTitle = leftTitle,
                RightTitle = rightTitle
            };
            return (new LeftRightListBoxComponent()).InvokeAsync("LeftRightList", new { list = model }).Result;

            //return htmlHelper.Action("LeftRightList", "Shared", new { list = model });
        }
        /// <summary>
        /// Allows or blocks the display of an editable div depending on user authorization level.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="requiredPrivilege">Privilege needed to show the field</param>
        /// <param name="readOnlyBehaviour">Behavior of the field if not authorized</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="forceReadOnly">Force the field into read-only mode regardless of authorization</param>
        /// <returns></returns>
        public static IHtmlContent EditableDivAreaForAuth<TModel, TProperty>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, Enumerations.Role requiredPrivilege, Enumerations.ReadOnlyBehaviour readOnlyBehaviour, object htmlAttributes, bool forceReadOnly = false)
        {
            // If user has no privileges refuse access
            bool fullAccess = !forceReadOnly && new UserSessionContext(htmlHelper.GetHttpContext()).UserHasAccess(requiredPrivilege);
            var htmlOptions = new RouteValueDictionary(htmlAttributes);

            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(htmlOptions);
            divTag.AddCssClass("editable-div");
            divTag.GenerateId(htmlHelper.GetControlId(expression) + "__editableDiv","_");
            divTag.Attributes.Add("parentId", htmlHelper.GetControlId(expression));

            ModelExpression modelExpression = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);

            divTag.InnerHtml.Append(HttpUtility.HtmlDecode((string)modelExpression.Model));

            string html = divTag.ToString();

            if (!fullAccess)
            {
                switch (readOnlyBehaviour)
                {
                    case Enumerations.ReadOnlyBehaviour.Disabled:
                        divTag.AddCssClass("disabled");
                        html = divTag.ToString();
                        break;

                    case Enumerations.ReadOnlyBehaviour.EmptyString:
                        break;
                    case Enumerations.ReadOnlyBehaviour.InnerHtml:
                        html = htmlHelper.SpanFor(expression, htmlAttributes).ToString();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("readOnlyBehaviour");
                }
            }

            html += htmlHelper.HiddenFor(expression).ToString();

            return new HtmlString(html);
        }

    }
}
