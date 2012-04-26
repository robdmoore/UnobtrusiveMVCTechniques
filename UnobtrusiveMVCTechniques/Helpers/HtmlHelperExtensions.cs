using System;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace UnobtrusiveMVCTechniques.Helpers
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders a textbox field with the corresponding label and validation messages all wrapped in a &lt;p&gt; tag.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TAttribute">Model attribute type for field being rendered</typeparam>
        /// <param name="htmlHelper">The Html helper for the model</param>
        /// <param name="field">An expression supplying the field in the model to render</param>
        /// <returns>An IHtmlString object that can be rendered to the page</returns>
        public static IHtmlString OutputField<TModel, TAttribute>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TAttribute>> field)
        {
            var s = new StringBuilder();

            s.Append("\r\n\t<p>");
            s.Append(htmlHelper.LabelFor(field));
            s.Append(" ");
            s.Append(htmlHelper.TextBoxFor(field));
            s.Append("<br />");
            s.Append(htmlHelper.ValidationMessageFor(field));
            s.Append("</p>\r\n");

            return new HtmlString(s.ToString());
        }
    }
}