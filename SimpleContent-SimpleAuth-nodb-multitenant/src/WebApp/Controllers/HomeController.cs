using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using cloudscribe.SimpleContent.Models;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using cloudscribe.Web.Common.Extensions;
using cloudscribe.Messaging.Email;
using cloudscribe.Web.Common.Razor;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ILogger<HomeController> logger, IProjectService projectService, IProjectEmailService emailService, ViewRenderer viewRenderer)
        {
            log = logger;
            this.projectService = projectService;
            this.emailService = emailService;
            this.viewRenderer = viewRenderer;

        }
        IProjectEmailService emailService;
        IProjectService projectService;
        private ILogger log;
        ViewRenderer viewRenderer;

        public async Task<IActionResult> Index()
        {
            return View();
        }

        //public IActionResult About()
        //{
        //    ViewData["Message"] = "Your application description page.";

        //    return View();
        //}

        //public IActionResult Contact()
        //{
        //    ViewData["Message"] = "Your contact page.";

        //    return View();
        //}

        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                if (statusFeature != null)
                {
                    log.LogWarning("handled 404 for url: {OriginalPath}", statusFeature.OriginalPath);
                }

            }
            return View(statusCode);
        }
        [HttpGet]
        public async Task<IActionResult> Contact()
        {
            IProjectSettings settings = await projectService.GetCurrentProjectSettings();
            ContactViewModel mdl = new ContactViewModel();
            mdl.ProjectSettings = settings;
            mdl.EmailService = emailService;
            return View(mdl);

        }


        [HttpPost]
        public  async Task<IActionResult>Test()
            {
            return Content("test post");
            }
        [HttpGet]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Test(int? id)
        {
            return Content("test get");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjaxPostContact(ContactViewModel model)
        {
            //ContactViewModel model = new ContactViewModel();
            // disable status code page for ajax requests
            var statusCodePagesFeature = HttpContext.Features.Get<IStatusCodePagesFeature>();
            if (statusCodePagesFeature != null)
            {
                statusCodePagesFeature.Enabled = false;
            }

            // this should validate the [EmailAddress] on the model
            // failure here should indicate invalid email since it is the only attribute in use
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 403;
                //await Response.WriteAsync("Please enter a valid e-mail address");
                return Content("Please enter a valid e-mail address");
            }

            var project = await projectService.GetCurrentProjectSettings();

            if (project == null)
            {
                log.LogDebug("returning 500 blog not found");
                return StatusCode(500);
            }


            if (string.IsNullOrEmpty(model.Name))
            {
                log.LogDebug("returning 403 because no name was posted");
                Response.StatusCode = 403;
                //await Response.WriteAsync("Please enter a valid name");
                return Content("Please enter a valid name");
            }

            if (string.IsNullOrEmpty(model.Content))
            {
                log.LogDebug("returning 403 because no content was posted");
                Response.StatusCode = 403;
                //await Response.WriteAsync("Please enter a valid content");
                return Content("Please enter a valid content");
            }


            if (!string.IsNullOrEmpty(project.RecaptchaPublicKey))
            {
                var captchaResponse = await this.ValidateRecaptcha(Request, project.RecaptchaPrivateKey);
                if (!captchaResponse.Success)
                {
                    log.LogDebug("returning 403 captcha validation failed");
                    Response.StatusCode = 403;
                    //await Response.WriteAsync("captcha validation failed");
                    return Content("captcha validation failed");
                }
            }


            model.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            model.Ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            /*var comment = new Comment()
            {
                Id = Guid.NewGuid().ToString(),
                Author = model.Name,
                Email = model.Email,
                Website = GetUrl(model.WebSite),
                Ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                UserAgent = userAgent,
                IsAdmin = User.CanEditProject(project.Id),
                Content = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(
                    model.Content.Trim()).Replace("\n", "<br />"),

                IsApproved = isApproved,
                PubDate = DateTime.UtcNow
            };

            blogPost.Comments.Add(comment);
            await blogService.Update(blogPost);
            */
            // TODO: clear cache

            //no need to send notification when project owner posts a comment, ie in response
            //var shouldSendEmail = !canEdit;

            //if (shouldSendEmail)
            //{
            //    var postUrl = await blogService.ResolvePostUrl(blogPost);
            var baseUrl = string.Concat(HttpContext.Request.Scheme,
                    "://",
                    HttpContext.Request.Host.ToUriComponent());

            IProjectSettings settings = await projectService.GetCurrentProjectSettings();
            //ContactViewModel mdl = new ContactViewModel();
            model.ProjectSettings = settings;
            model.EmailService = emailService;
            model.Url = baseUrl;// + postUrl;
            //EmailSender email = new EmailSender();
            SmtpOptions smtpOptions = model.GetSmptOptions(settings);

            if (smtpOptions == null)
            {
                var logMessage = $"failed to send contact request  email because smtp settings are not populated for project {project.Id}";
                log.LogError(logMessage);
                Response.StatusCode = 500;
                return Content("No Smtp");
            }

            if (string.IsNullOrWhiteSpace(project.WebmasterEmail))
            {
                var logMessage = $"failed to send contact request  email because WebMasterEmail is not populated for project {project.Id}";
                Response.StatusCode = 500;
                log.LogError(logMessage);
                return Content("No WebMasterEmail");
            }

            if (string.IsNullOrWhiteSpace(project.EmailFromAddress))
            {
                var logMessage = $"failed to send contactrequest email because EmailFromAddress is not populated for project {project.Id}";
                log.LogError(logMessage);
                Response.StatusCode = 500;
                return Content("No FromAddress");
            }

            //var model = new CommentNotificationModel(project, post, comment, postUrl);
            var subject = $"Contact Request:{model.Email}";

            string plainTextMessage = model.Content;
            string htmlMessage = null;
            var sender = new EmailSender();
            //string plainTextMessage = null;
            //string htmlMessage = null;
            //var sender = new EmailSender();
            //                    model.Content.Trim()).Replace("\n", "<br />");
            try
            {
                try
                {
                    model.Content = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(model.Content.Trim()).Replace("\n", "<br />");
                    htmlMessage
                        = await viewRenderer.RenderViewAsString<ContactViewModel>("ContactRequestEmail", model);
                }
                catch (Exception ex)
                {
                    log.LogError("error generating html email from razor template", ex);
                    Response.StatusCode = 500;
                    return Content("Could not render email");
                }

                await sender.SendEmailAsync(
                    smtpOptions,
                    project.WebmasterEmail, //to
                    project.EmailFromAddress, //from
                    subject,
                    plainTextMessage,
                    htmlMessage,
                    model.Email //replyto
                    ).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError("error sending comment notification email", ex);
            }



            return StatusCode(200);

        }

    }


}

