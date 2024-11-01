using Microsoft.Extensions.Configuration;
using Moq;
using Store.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreTests
{
    public class SendEmailGoogleTests
    {
        [Fact]
        public async Task Can_Send_Email()
        {
            // Arrange
            var configMock = new Mock<IConfiguration>();
            configMock.SetupGet(c => c["GmailAccount:Name"]).Returns("coolmartun@gmail.com");
            configMock.SetupGet(c => c["GmailAccount:AppPassword"]).Returns("voms nylf npaa prvh");
            var emailSender = new SendEmailGoogle(configMock.Object);


            string htmlContent = @"
            <!DOCTYPE html>
            <html lang=""en"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Welcome to ILoveParts! 🎉</title>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        background-color: #f4f4f4;
                        display: flex;
                        flex-direction: column;
                        align-items: center;
                        justify-content: center;
                        min-height: 100vh;
                    }

                    .container {
                        background-color: #fff;
                        padding: 40px;
                        border-radius: 8px;
                        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                        text-align: center;
                    }

                    img {
                        max-width: 150px;
                        margin-bottom: 20px;
                    }

                    h1 {
                        color: #333;
                        margin-bottom: 20px;
                    }

                    p {
                        line-height: 1.6;
                        color: #666;
                    }

                    a {
                        color: #007bff;
                        text-decoration: none;
                    }

                    a:hover {
                        text-decoration: underline;
                    }
                </style>
            </head>
            <body>
                <div class=""container"">
                    <img src=""https://dontknowhowtonameit1.blob.core.windows.net/web/wwwroot/images/logo_iparts.png"" alt=""Your App Logo""> <!-- Logo path updated -->
                    <h1>Welcome to ILoveParts! 🎉</h1>
                    <p>Congratulations! You have successfully registered for ILoveParts.</p>
                    <p>You can now start exploring all the amazing features and benefits of ILoveParts.</p>
                    <p>If you have any questions, please don't hesitate to contact us at info.iloveparts@gmail.com.</p>
                    <p>Happy using ILoveParts!</p>
                </div>
            </body>
            </html>";

            // Act
            // string resp = await emailSender.SendEmailAsync("matevoshay@gmail.com", "Test Subject", htmlContent);
        }
    }
}
