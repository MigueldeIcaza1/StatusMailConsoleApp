using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;

using Interfaces;
using Models;

namespace EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly int MaxRetryCount = 5;

        public void SendEmail(List<StatusRecord> statusList)
        {
            var retrycount = 0;
            try
            {
                var body = GetEmailBody(statusList);
                SendEmail(body);
            }
            catch (Exception)
            {
                Console.WriteLine("Mail sending failed.. Trying again");
                if (retrycount++ < MaxRetryCount)
                {
                    var body = GetEmailBody(statusList);
                    SendEmail(body);
                }
                else
                {
                    Console.WriteLine($"Exceeded max retry count of {MaxRetryCount} for sending mail");
                }
            }
        }

        public static void SendEmail(string htmlString)
        {
            var fromEmail = ConfigurationManager.AppSettings.Get("fromMail");
            var toEmail = ConfigurationManager.AppSettings.Get("toMail");
            var subject = ConfigurationManager.AppSettings.Get("subject");
            var ccMail = ConfigurationManager.AppSettings.Get("ccMail");
            var smtpSection = (SmtpSection)ConfigurationManager.GetSection("mailSettings/smtp_1");

            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress(fromEmail);
            message.To.Add(new MailAddress(toEmail));

            if (!string.IsNullOrEmpty(ccMail))
            {
                message.CC.Add(new MailAddress(ccMail));
            }

            message.Subject = subject;
            message.IsBodyHtml = true; //to make message body as html  
            message.Body = htmlString;
            smtp.Port = smtpSection.Network.Port;
            smtp.Host = smtpSection.Network.Host; //for gmail host  
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
            Console.WriteLine("\n Mail sent successfully..!!!!");
            Console.WriteLine("::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
        }

        public static string GetEmailBody(List<StatusRecord> workItems)
        {
            var rootPath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            var dailyStatusHTMLPath = rootPath + "/Assets/DailyStatus.html";
            var dailyStatusHtml = File.ReadAllText(dailyStatusHTMLPath);

            var statusRowHtmlPath = rootPath + "/Assets/StatusRow.html";
            var statusRowHtml = File.ReadAllText(statusRowHtmlPath);
            var allStatusRowsHtml = AppenedStatusRows(statusRowHtml, workItems);

            dailyStatusHtml = dailyStatusHtml.Replace("#StatusRows#", allStatusRowsHtml);
            return dailyStatusHtml;
        }

        private static string AppenedStatusRows(string rowHTML, List<StatusRecord> workItems)
        {
            var statusRowsHTML = string.Empty;
            var groupedWorkItems = workItems.GroupBy(t => t.ParentTitle).ToList();

            foreach (var groupItem in groupedWorkItems)
            {
                var count = 0;
                foreach (var workItem in groupItem)
                {
                    count++;
                    string rowHtmlWithReplacedWorkItem;
                    if (count == 1)
                    {
                        var workItemHtml = $"<td rowspan = \"{groupItem.Count()}\"> {groupItem.Key} </td>";
                        rowHtmlWithReplacedWorkItem = rowHTML.Replace("#GroupedWorkItemHTML#", workItemHtml);
                    } 
                    else
                    {
                        rowHtmlWithReplacedWorkItem = rowHTML.Replace("#GroupedWorkItemHTML#", string.Empty);
                    }

                    statusRowsHTML = statusRowsHTML + ReplaceWithWorkItem(rowHtmlWithReplacedWorkItem, workItem);
                }
            }

            return statusRowsHTML;
        }

        // need to do dynamically
        private static string ReplaceWithWorkItem(string rowHtml, StatusRecord workItem)
        {
            rowHtml = rowHtml.Replace("#Title#", workItem.TaskTitle);
            rowHtml = rowHtml.Replace("#ID#", workItem.TaskIdWithLink.Id.ToString());
            rowHtml = rowHtml.Replace("#IDLink#", workItem.TaskIdWithLink.Link);
            rowHtml = rowHtml.Replace("#AssignedTo#", workItem.AssignedTo);
            rowHtml = rowHtml.Replace("#Status#", workItem.TaskStatus.ToString());
            return rowHtml;
        }
    }
}
