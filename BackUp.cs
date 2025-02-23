
namespace CREDAJAX.Serves
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Data.SqlClient;

    public class BackUp
    {
        private readonly string _connectionString;
        private readonly string _backupFolderPath;
        private readonly string _fromEmail = "Email@gmail"; // بريد المرسل
        private readonly string _fromPassword = "Password (Google-scurty-AppPasword)"; // كلمة مرور التطبيقات
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _toEmail = "Svhod@gmail.com"; // البريد الذي سيتم إرسال النسخة إليه

        public BackUp(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Mycon"); //مكان حفظ نص الاتصال 
            _backupFolderPath = "C:\\DatabaseBackups"; 
        }

        public async Task CreateBackupAndSendEmailAsync()
        {
            try
            {
                // 1️⃣ إنشاء النسخة الاحتياطية
                string databaseName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
                string backupFileName = Path.Combine(_backupFolderPath, $"{databaseName}_{DateTime.Now:yyyyMMddHHmmss}.bak");

                if (!Directory.Exists(_backupFolderPath))
                {
                    Directory.CreateDirectory(_backupFolderPath);
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFileName}'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        await cmd.ExecuteNonQueryAsync(); // استخدام ExecuteNonQueryAsync بدلاً من ExecuteNonQuery
                    }
                }

                Console.WriteLine($"✔️ تم إنشاء النسخة الاحتياطية بنجاح: {backupFileName}");

                // 2️⃣ إرسال النسخة الاحتياطية عبر البريد
                await SendBackupEmailAsync(backupFileName); // استخدام الدالة المعدلة
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ أثناء النسخ الاحتياطي: {ex.Message}");
            }
        }

        private async Task SendBackupEmailAsync(string backupFilePath)
        {
            try
            {
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = "📌 نسخة احتياطية لقاعدة البيانات",
                   
                    Body = $"تم إنشاء نسخة احتياطية مرفقة لقاعدة البيانات في {DateTime.Now:yyyy/MM/dd HH:mm:ss}.",
                    IsBodyHtml = false
                };

                mail.To.Add(_toEmail);
                mail.Attachments.Add(new Attachment(backupFilePath));

                SmtpClient smtp = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail); // استخدام SendMailAsync بدلاً من Send
                Console.WriteLine($"📧 تم إرسال النسخة الاحتياطية إلى {_toEmail} بنجاح!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ أثناء إرسال البريد الإلكتروني: {ex.Message}");
            }
        }
    }


}

