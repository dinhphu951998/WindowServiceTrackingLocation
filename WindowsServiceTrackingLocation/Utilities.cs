using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceTrackingLocation
{
    public class Utilities
    {
        private string filename = AppDomain.CurrentDomain.BaseDirectory + "\\LocationFile.txt";

        public void Log(string msg)
        {
            StreamWriter writer = null;
            if (!File.Exists(filename))
            {
                File.Create(filename);
            }
            using (writer = new StreamWriter(filename, true))
            {
                writer.WriteLine(DateTime.Now.ToString() + " " + msg);
                writer.Flush();
            }

        }

        public void LogAddress(string address, string address_component)
        {
            if (!File.Exists(filename))
            {
                File.Create(filename);
            }
            StreamWriter writer = null;
            using (writer = new StreamWriter(filename, true))
            {
                writer.WriteLine(DateTime.Now.ToString());
                writer.WriteLine($"{address}");
                writer.WriteLine($"{address_component}");
                writer.Flush();
            }

        }

        //public bool SendMail()
        //{
        //    MailAddress from = new MailAddress("dongphan24@gmail.com"
        //                                        , "Location tracking");
        //    MailAddress to = new MailAddress("phundse63159@fpt.edu.vn");
        //    MailMessage message = new MailMessage(from, to);
        //    message.Subject = "Tracking Location " + DateTime.Now.ToString();
        //    string content = "";
        //    StreamReader reader = null;
        //    using (reader = new StreamReader(filename))
        //    //AppDomain.CurrentDomain.BaseDirectory + "\\" +
        //    {
        //        content = reader.ReadToEnd();
        //    }
        //    message.Body = content;
        //    System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient()
        //    {
        //        Host = "smtp.gmail.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential("dongphan24@gmail.com"
        //                                            , "dongphan987654321")
        //    };

        //    try
        //    {
        //        client.Send(message);
        //    }
        //    catch (Exception e)
        //    {
        //        Log("Error in sending mail process " + e.Message);
        //        return false;
        //    }
        //    return true;
        //}

        public bool SendingMailUsingMailKit()
        {
            try
            {
                string content = "";
                StreamReader reader = null;
                using (reader = new StreamReader(filename, true))
                {
                    content = reader.ReadToEnd();
                }
                if (content.Trim().Equals(String.Empty))
                {
                    return true;
                }
                var message = new MimeMessage();
                MailboxAddress from = new MailboxAddress("Location tracking"
                                                    , "WindowServiceTrackingLocation@admin.vn");
                MailboxAddress to = new MailboxAddress("dongphan24@gmail.com");
                message.From.Add(from);
                message.To.Add(to);
                message.Subject = "Tracking Location " + DateTime.Now.ToString();

                message.Body = new TextPart()
                {
                    Text = content
                };
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587);
                    client.Authenticate("dongphan24@gmail.com", "dongphan987654321");
                    client.Send(message);
                    client.Disconnect(true);
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\" + filename
                                        , String.Empty);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log("Error in sending mail process " + e.Message);
                return false;
            }
        }



    }
}
