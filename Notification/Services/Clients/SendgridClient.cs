using IT.WebServices.Fragments.Notification;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Threading.Tasks;
using IT.WebServices.Settings;

namespace IT.WebServices.Notification.Services.Clients
{
    public class SendgridClient
    {
        private readonly ISettingsService settingsService;

        public SendgridClient(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public async Task<string> SendEmail(SendEmailRequest request)
        {
            var settings = await settingsService.GetOwnerDataInternal();

            if (settings.Owner?.Notification?.Sendgrid == null)
                return "Email Service Disabled";

            if (!settings.Owner.Notification.Sendgrid.Enabled)
                return "Email Service Disabled";

            var apiKey = settings.Owner.Notification.Sendgrid.ApiKeySecret;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(settings.Owner.Notification.Sendgrid.SendFromAddress);
            var subject = request.Subject;
            var to = new EmailAddress(request.SendToAddress);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, request.BodyPlain, request.BodyHtml);
            var response = await client.SendEmailAsync(msg);

            return null;
        }
    }
}
