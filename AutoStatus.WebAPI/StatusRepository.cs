using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interfaces;
using Newtonsoft.Json;
using TfsProject;

namespace AutoStatus
{
    public class StatusRepository 
    {
        ITfsService tfsService;
        IEmailSender emailSender;

        public StatusRepository()
        {
            tfsService = new TfsService(); 
            emailSender = new EmailSender.EmailSender();
        }

        public async Task<string> GetMailBody()
        {
            //Uri collectionUri = new Uri(ConfigurationManager.AppSettings.Get("collectionUri"));
            //string projectName = ConfigurationManager.AppSettings.Get("projectName");
            //string folderHierarchy = ConfigurationManager.AppSettings.Get("queryFolderHierarchy");
            //var folders = ExtractFolderNames(folderHierarchy, ',');

            var collectionUri = new Uri("https://dev.azure.com/KantolaTraining/");
            var projectName = "Kantola LMS";
            var folders = ExtractFolderNames("Shared Queries,Daily Status Latest", ',');
            string body = null;

            try
            {
                var statusRecords = await tfsService.GetDataFromTFS(collectionUri, projectName, folders);
                // emailSender.SendEmail(statusRecords);
                body = EmailSender.EmailSender.GetEmailBody(statusRecords);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
            }

            return body;
        }

        public void SendMail(string htmlString)
        {
            EmailSender.EmailSender.SendEmail(htmlString);
        }

        private List<string> ExtractFolderNames(string folderHierarchy, char seperator)
        {
            return folderHierarchy.Split(seperator).ToList();
        }
    }
}

