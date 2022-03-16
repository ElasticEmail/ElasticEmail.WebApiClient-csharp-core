**LEGACY**

New version of API - https://github.com/ElasticEmail/elasticemail-csharp

**This library allows you to quickly and easily use the Elastic Email Web API v2 via C# with .NET Core.**

Version 1.X.X+ of this library provides full support for all Elastic Email Web API v2 endpoints.

We want this library to be community driven and Elastic Email led. We need your help to realize this goal. To help make sure we are building the right things in the right order, we ask that you create [issues](https://github.com/ElasticEmail/ElasticEmail.WebApiClient-csharp-core/issues) or simply upvote or comment on existing issues or pull requests.

Please browse the rest of this README for further detail.

We appreciate your continued support, thank you!

# Table of Contents #
* Installation
* Quick Start
* About

# Installation #
## Prerequisites ##
* .NET Core version 1.6 or higher
* [An Elastic Email account](https://elasticemail.com/account/)
## Install Package ##
To use ElasticEmail in your C# project, you can either [download the ElasticEmail C# .NET libraries directly from our Github repository](https://github.com/ElasticEmail/ElasticEmail.WebApiClient-csharp-core) or, if you have the NuGet package manager installed, you can grab them automatically.

```
PM> Install-Package ElasticEmail.WebApiClient
```

Once you have the ElasticEmail.WebApiClient libraries properly referenced in your project, you can include calls to them in your code. 

# Quick Start #
## Send Email ##
The following is the minimum needed code to send an simple email:

```
using static ElasticEmailClient.Api;
using static ElasticEmailClient.ApiTypes;

namespace SendEmailExample
{
    class Program
    {
        static void Main(string[] args)
        {
            ApiKey = "YOUR-API-KEY";

            var task = SendEmail("Hello World from Elastic Email!", "fromAddress@exmple.com", "John Tester", new string[] { "toAddress@exmple.com" },
                                "<h1>Hello! This mail was sent by Elastic Email service.<h1>", "Hello! This mail was sent by Elastic Email service.");

            task.ContinueWith(t =>
            {
                if (t.Result == null)
                    Console.WriteLine("Something went wrong. Check the logs.");
                else
                {
                    Console.WriteLine("MsgID to store locally: " + t.Result.MessageID); // Available only if sent to a single recipient
                    Console.WriteLine("TransactionID to store locally: " + t.Result.TransactionID);
                }
            });

            task.Wait();
        }

        public async static Task<ElasticEmailClient.ApiTypes.EmailSend> SendEmail(string subject, string fromEmail, string fromName, string[] msgTo, string html, string text)
        {
            try
            {
                return await ElasticEmailClient.Api.Email.SendAsync(subject, fromEmail, fromName, msgTo: msgTo, bodyHtml: html, bodyText: text);
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    Console.WriteLine("Server didn't accept the request: " + ex.Message);
                else
                    Console.WriteLine("Something unexpected happened: " + ex.Message);

                return null;
            }
        }
    }
}
```
## Load Account ##
This is a simple example on how to download your Account's information:
```
using System.Threading.Tasks;
using static ElasticEmailClient.Api;
using static ElasticEmailClient.ApiTypes;

namespace LoadAccountExample
{
    class Program
    {
        static void Main(string[] args)
        {
            ApiKey = "YOUR-API-KEY";

            var task = LoadAccount();

            task.ContinueWith(t =>
            {
                if (t.Result == null)
                    Console.WriteLine("Something went wrong. Check the logs.");
                else
                    Console.WriteLine(t.Result.PricePerEmail);
            });

            task.Wait();
        }

        public async static Task<ElasticEmailClient.ApiTypes.Account> LoadAccount()
        {
            try
            {
                return await ElasticEmailClient.Api.Account.LoadAsync();
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException)
                    Console.WriteLine("Server didn't accept the request: " + ex.Message);
                else
                    Console.WriteLine("Something unexpected happened: " + ex.Message);

                return null;
            }
        }
    }
}
```
# About #
ElasticEmail.WebApiClient-csharp-core is guided and supported by the ElasticEmail Dev Team.

ElasticEmail.WebApiClient-csharp-core is maintained and funded by Elastic Email Inc. The names and logos for ElasticEmail.WebApiClien-csharp are trademarks of Elastic Email Inc.

![logo](https://elasticemail.com/files/ee_200x200.png )
