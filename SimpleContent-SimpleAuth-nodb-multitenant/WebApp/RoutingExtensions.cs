using cloudscribe.SimpleContent.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public static class RoutingExtensions
    {
        public static IRouteBuilder AddRoutes(this IRouteBuilder routes)
        {
            return routes.AddBlogRoutesForSimpleContent().AddDefaultPageRouteForSimpleContent();
        }

        public static IRouteBuilder AddBlogRoutesForSimpleContent(this IRouteBuilder routes)
        {
            routes.MapRoute(
                   name: ProjectConstants.BlogCategoryRouteName,
                   template: "blog/category/{category=''}/{pagenumber=1}"
                   , defaults: new { controller = "Blog", action = "Category" }
                   );


            routes.MapRoute(
                  ProjectConstants.BlogArchiveRouteName,
                  "blog/{year}/{month}/{day}",
                  new { controller = "Blog", action = "Archive", month = "00", day = "00" },
                  //new { controller = "Blog", action = "Archive" },
                  new { year = @"\d{4}", month = @"\d{2}", day = @"\d{2}" }
                );

            routes.MapRoute(
                  ProjectConstants.PostWithDateRouteName,
                  "blog/{year}/{month}/{day}/{slug}",
                  new { controller = "Blog", action = "PostWithDate" },
                  new { year = @"\d{4}", month = @"\d{2}", day = @"\d{2}" }
                );

            routes.MapRoute(
               name: ProjectConstants.PostEditRouteName,
               template: "blog/edit/{slug?}"
               , defaults: new { controller = "Blog", action = "Edit" }
               );

            routes.MapRoute(
               name: ProjectConstants.PostDeleteRouteName,
               template: "blog/delete/{id?}"
               , defaults: new { controller = "Blog", action = "Delete" }
               );

            //routes.MapRoute(
            //   name: ProjectConstants.NewPostRouteName,
            //   template: "blog/new"
            //   , defaults: new { controller = "Blog", action = "New" }
            //   );

            routes.MapRoute(
              name: ProjectConstants.MostRecentPostRouteName,
              template: "blog/mostrecent"
              , defaults: new { controller = "Blog", action = "MostRecent" }
              );

            routes.MapRoute(
               name: ProjectConstants.PostWithoutDateRouteName,
               template: "blog/{slug}"
               , defaults: new { controller = "Blog", action = "PostNoDate" }
               );

            routes.MapRoute(
               name: ProjectConstants.BlogIndexRouteName,
               template: ""
               , defaults: new { controller = "Blog", action = "Index" }
               );

            return routes;
        }
        public static IRouteBuilder AddDefaultPageRouteForSimpleContent(this IRouteBuilder routes)
        {
            routes.MapRoute(
               name: ProjectConstants.PageEditRouteName,
               template: "edit/{slug?}"
               , defaults: new { controller = "Page", action = "Edit" }
               );

            routes.MapRoute(
               name: ProjectConstants.PageDevelopRouteName,
               template: "development/{slug}"
               , defaults: new { controller = "Page", action = "Development" }
               );

            routes.MapRoute(
               name: ProjectConstants.PageTreeRouteName,
               template: "tree"
               , defaults: new { controller = "Page", action = "Tree" }
               );

            routes.MapRoute(
               name: ProjectConstants.PageDeleteRouteName,
               template: "delete/{id}"
               , defaults: new { controller = "Page", action = "Delete" }
               );

            routes.MapRoute(
               name: ProjectConstants.PageIndexRouteName,
               template: "{slug=none}"
               , defaults: new { controller = "Page", action = "Index" }
               );



            return routes;
        }

    }
}
