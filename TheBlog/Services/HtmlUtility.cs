
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TheBlog.Models;

namespace TheBlog.Services
{
    public static class HtmlUtility
    {
        public static string IsActive(this IHtmlHelper html,
            string control,
            string action)
        {


            var routeData = html.ViewContext.RouteData;
            var razorRoute = (routeData.Values["page"]?.ToString())?[1..]?.Split('/');

            if (razorRoute is not null)
            {
                var razorAction = razorRoute[0];
                var razorControl = razorRoute[1];

                var returnActive = string.Equals(control, razorAction) && string.Equals(action, razorControl);
                return returnActive ? "active" : "";
            }
            else
            {
                var routeAction = (string)routeData.Values["action"];
                var routeControl = (string)routeData.Values["controller"];

                var returnActive = string.Equals(control, routeControl) && string.Equals(action, routeAction);
                return returnActive ? "active" : "";
            }
        }
    }
}