using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AutomatedQA.TestPlan
{
    public class SendReport
    {
        public static void SendReportByEmail(string subject, string emailFrom, string emailPassword, string emailTo,
            string ccTo, string emailHost, int emailPort, bool enableSsl, string tempPath, DataTable dtResult, params string[] attachments)
        {
            List<string> imagefiles = new List<string>();

            StringBuilder sb = new StringBuilder(@"<style type='text/css'>
  th {background-color: #0099CC;color: white;}
  .fail {background-color: #FF6666;}
  .success {background-color: #99CC66;}
</style>");
            sb.Append("<table width='100%' border='1' cellpadding='0' cellspacing='0'><tr>");
            foreach (DataColumn dc in dtResult.Columns)
                sb.AppendFormat("<th>{0}</th>", dc.ColumnName);

            List<Attachment> lstAttachment = attachments == null ? new List<Attachment>() : attachments.Select(attachFilePath => new Attachment(attachFilePath)).ToList();

            foreach (DataRow dr in dtResult.Rows)
            {
                if (!DBNull.Value.Equals(dr["Image"]) && !string.IsNullOrEmpty(dr["Image"].ToString()))
                {
                    string imageHtml = string.Empty;
                    foreach (string errImage in dr["Image"].ToString().Split(';'))
                    {
                        if (string.IsNullOrEmpty(errImage))
                            continue;
                        Attachment attachImage = new Attachment(errImage, "image/png");
                        attachImage.Name = Path.GetFileNameWithoutExtension(errImage);
                        imageHtml += string.Format("<img src=\"cid:{0}\">", attachImage.ContentId);
                        lstAttachment.Add(attachImage);
                        imagefiles.Add(errImage);
                    }
                    dr["Image"] = imageHtml;
                    sb.Append("</tr><tr class='fail'>");
                }
                else if (dr["Result"].Equals("Passed"))
                    sb.Append("</tr><tr class='success'>");
                else
                    sb.Append("</tr><tr>");

                foreach (DataColumn dc in dtResult.Columns)
                {
                    sb.AppendFormat("<td>{0}</td>", dr[dc]);
                }
            }
            sb.Append("</tr></table>");


            MailMessage mailMessage = null;
            AlternateView htmlBody = null;
            SmtpClient client = null;
            try
            {
                mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(emailFrom);
                foreach (string toemail in emailTo.Split(';'))
                    mailMessage.To.Add(toemail);
                if (!string.IsNullOrEmpty(ccTo))
                {
                    foreach (string ccemail in ccTo.Split(';'))
                        mailMessage.CC.Add(ccemail);
                }
                mailMessage.Subject = subject;
                htmlBody = AlternateView.CreateAlternateViewFromString(sb.ToString(), null,
                    "text/html");
                foreach (var image in lstAttachment)
                    mailMessage.Attachments.Add(image);
                mailMessage.AlternateViews.Add(htmlBody);
                client = new SmtpClient(emailHost, emailPort);

                client.Credentials = new System.Net.NetworkCredential(emailFrom, emailPassword);
                client.EnableSsl = enableSsl;
                client.Send(mailMessage);

            }
            catch (SmtpException ex)
            {
                throw new Exception("Email error, detail information:" + ex.Message);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
                if (htmlBody != null)
                {
                    htmlBody.Dispose();
                }
                if (mailMessage != null)
                {
                    mailMessage.Dispose();
                }
            }

        }
    }
}
