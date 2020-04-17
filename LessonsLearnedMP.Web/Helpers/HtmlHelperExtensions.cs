using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Suncor.LessonsLearnedMP.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string GetExpressionText<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TResult>> expression)
        {
            ModelExpressionProvider expresionProvider = htmlHelper.GetModelExpressionProvider();

            return expresionProvider.GetExpressionText(expression);
        }

        public static ModelExpressionProvider GetModelExpressionProvider<TModel>(this IHtmlHelper<TModel> htmlHelper)
        {
            return htmlHelper.GetHttpContext().RequestServices
                            .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;
        }

        public static HttpContext GetHttpContext(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.ViewContext.HttpContext;
        }

        public static IDistributedCache GetDistributedCache(this IHtmlHelper htmlHelper)
        {
            return htmlHelper.GetHttpContext().RequestServices
                            .GetService(typeof(IDistributedCache)) as IDistributedCache;
        }



        public static string GetFullHtmlFieldId<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TResult>> expression)
        {
            return htmlHelper.GenerateIdFromName(htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlHelper.GetExpressionText(expression)));
        }
        public static ICompositeViewEngine GetCompositeViewEngine(this Controller controller)
        {
            var engine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            return engine;
        }
        /*
        public static IDictionary<string, object> UnobtrusiveValidationAttributesFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var metadata = htmlHelper.GetModelExpressionProvider().CreateModelExpression(htmlHelper.ViewData, expression);
            
            var validator = htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(ValidationHtmlAttributeProvider)) as ValidationHtmlAttributeProvider;

            Dictionary<string, string> attributes = new Dictionary<string, string>();
            validator.AddAndTrackValidationAttributes(htmlHelper.ViewContext, metadata.ModelExplorer, expression, attributes);

            
        }*/
    }
}
