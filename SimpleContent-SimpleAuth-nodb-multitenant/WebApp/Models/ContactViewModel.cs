using cloudscribe.Messaging.Email;
using cloudscribe.SimpleContent.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class ContactViewModel
    {
        public IProjectSettings ProjectSettings { get; set; }
        public IProjectEmailService EmailService { get; set; }

        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string WebSite { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;


        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public string Url { get; set;}

        public SmtpOptions GetSmptOptions(IProjectSettings project)
        {
            if (string.IsNullOrWhiteSpace(project.SmtpServer)) { return null; }

            SmtpOptions smtpOptions = new SmtpOptions();
            smtpOptions.Password = project.SmtpPassword;
            smtpOptions.Port = project.SmtpPort;
            smtpOptions.PreferredEncoding = project.SmtpPreferredEncoding;
            smtpOptions.RequiresAuthentication = project.SmtpRequiresAuth;
            smtpOptions.Server = project.SmtpServer;
            smtpOptions.User = project.SmtpUser;
            smtpOptions.UseSsl = project.SmtpUseSsl;

            return smtpOptions;
        }

    }
}
