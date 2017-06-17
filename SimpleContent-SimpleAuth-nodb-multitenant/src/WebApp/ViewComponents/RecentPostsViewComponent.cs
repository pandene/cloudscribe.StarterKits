using cloudscribe.SimpleContent.Models;
using cloudscribe.SimpleContent.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace eo4coding.Web
{
    public class RecentPostsViewComponent2 : ViewComponent
    {
        public RecentPostsViewComponent2(
            IProjectService projectService,
            IPostQueries postQueries,
            IHtmlProcessor htmlProcessor,
            IBlogRoutes blogRoutes, 
            IBlogService blogService
            )
        {
            this.blogService = blogService;
            this.projectService = projectService;
            this.postQueries = postQueries;
            this.htmlProcessor = htmlProcessor;
            this.blogRoutes = blogRoutes;

        }
        private IBlogRoutes blogRoutes;
    private IBlogService blogService;

    private IProjectService projectService;
        private IPostQueries postQueries;
        private IHtmlProcessor htmlProcessor;


        public async Task<IViewComponentResult> InvokeAsync(string viewName = "RecentPosts", int numberToShow = 5)
        {
            //var model = new RecentPostsViewModel();
            var projectSettings = await projectService.GetCurrentProjectSettings().ConfigureAwait(false);
            var list = await postQueries.GetRecentPosts(projectSettings.Id, numberToShow);

            var model = new BlogViewModel(htmlProcessor);
            //model.ProjectSettings = projectSettings;
            // check if the user has the BlogEditor claim or meets policy
            model.CanEdit = false;// await User.CanEditBlog(projectSettings.Id, authorizationService);
            model.ProjectSettings = projectSettings;
            model.BlogRoutes = blogRoutes;
            model.CurrentCategory = null;
            if (!string.IsNullOrEmpty(model.CurrentCategory))
            {
                model.ListAction = "Category";
            }

            model.Posts = list;
            model.Categories = await blogService.GetCategories(model.CanEdit);
            model.Archives = await blogService.GetArchives(model.CanEdit);
            model.Paging.ItemsPerPage = model.ProjectSettings.PostsPerPage;
            /*
            model.Paging.CurrentPage = page;
            model.Paging.TotalItems = result.TotalItems;
            model.TimeZoneHelper = timeZoneHelper;
            model.TimeZoneId = model.ProjectSettings.TimeZoneId;
            model.NewItemPath = Url.RouteUrl(blogRoutes.PostEditRouteName, new { slug = "" });

            model.ProjectSettings = settings;
            model.Posts = list;
            */
            return View(viewName, model);
        }

    }
}
