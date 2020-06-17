using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Interfaces;
using Models;
using Newtonsoft.Json;

namespace AutoStatus
{
    public class AutoStatusSender : IAutoStatusSender
    {
        ITfsService tfsService;
        IEmailSender emailSender;

        public AutoStatusSender(ITfsService _tfsService, IEmailSender _emailSender)
        {
            tfsService = _tfsService;
            emailSender = _emailSender;
        }

        public async Task SendStatus()
        {
            Uri collectionUri = new Uri(ConfigurationManager.AppSettings.Get("collectionUri"));
            string projectName = ConfigurationManager.AppSettings.Get("projectName");
            string folderHierarchy = ConfigurationManager.AppSettings.Get("queryFolderHierarchy");
            var folders = ExtractFolderNames(folderHierarchy, ',');

            try
            {
                 var statusRecords = await tfsService.GetDataFromTFS(collectionUri, projectName, folders);
                //var statusRecords = new List<StatusRecord>();
                emailSender.SendEmail(statusRecords);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception :" + ex.Message);
            }

            Console.ReadKey();
        }

        private List<string> ExtractFolderNames(string folderHierarchy, char seperator)
        {
            return folderHierarchy.Split(seperator).ToList();
        }
    }
}
