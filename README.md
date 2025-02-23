# BackUp Service for Database

## Overview
This service is responsible for creating a backup of a SQL Server database and sending it as an email attachment using Gmail's SMTP server.

## Features
- Automatically creates a backup of the database.
- Stores the backup file in a designated folder (`C:\DatabaseBackups`).
- Sends the backup file as an email attachment.
- Uses asynchronous operations for better performance.

## Prerequisites
Before using this service, ensure that you have the following:

1. **.NET 6 or higher** installed on your system.
2. **SQL Server** with a configured database.
3. **Gmail account** with an App Password enabled for SMTP access.
4. **Install required packages:**
   ```sh
   dotnet add package System.Data.SqlClient
   ```

## Configuration
Modify the `appsettings.json` file to include the database connection string:
```json
{
  "ConnectionStrings": {
    "Mycon": "Server=YOUR_SERVER;Database=YOUR_DATABASE;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

## Implementation
### 1. Inject Configuration
The `BackUp` class reads the connection string from the application's configuration.

### 2. Backup Database
It executes an SQL `BACKUP DATABASE` command to create a `.bak` file stored in `C:\DatabaseBackups`.

### 3. Send Backup via Email
The backup file is attached to an email and sent to the specified recipient using SMTP.

## Usage
To use this service, inject it into your controller or application and call the method:
```csharp
await _backupService.CreateBackupAndSendEmailAsync();
```

## Code Explanation

```csharp
public class BackUp
{
    private readonly string _connectionString;
    private readonly string _backupFolderPath;
    private readonly string _fromEmail = "Email@gmail.com"; // Sender email
    private readonly string _fromPassword = "GoogleAppPassword"; // App password
    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _toEmail = "recipient@gmail.com"; // Receiver email

    public BackUp(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Mycon");
        _backupFolderPath = "C:\\DatabaseBackups";
    }

    public async Task CreateBackupAndSendEmailAsync()
    {
        try
        {
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
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            Console.WriteLine($"✔️ Backup created successfully: {backupFileName}");
            await SendBackupEmailAsync(backupFileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Backup error: {ex.Message}");
        }
    }

    private async Task SendBackupEmailAsync(string backupFilePath)
    {
        try
        {
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = "📌 Database Backup",
                Body = $"A database backup has been created on {DateTime.Now:yyyy/MM/dd HH:mm:ss}.",
                IsBodyHtml = false
            };
            mail.To.Add(_toEmail);
            mail.Attachments.Add(new Attachment(backupFilePath));

            SmtpClient smtp = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
            Console.WriteLine($"📧 Backup email sent to {_toEmail} successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email sending error: {ex.Message}");
        }
    }
}
```

## Notes
- Ensure your Gmail account has **App Passwords** enabled to allow SMTP authentication.
- Verify that your SQL Server has permissions to create backups in the specified folder.
- Ensure the folder `C:\DatabaseBackups` exists or is writable by the application.

## License
This project is open-source and can be modified as needed.

