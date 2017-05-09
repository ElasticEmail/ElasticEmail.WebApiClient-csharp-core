**This library allows you to quickly and easily use the Elastic Email Web API v2 via C# with .NET.**

Version 1.X.X+ of this library provides full support for all Elastic Email Web API v2 endpoints.

We want this library to be community driven and Elastic Email led. We need your help to realize this goal. To help make sure we are building the right things in the right order, we ask that you create [issues](https://github.com/ElasticEmail/ElasticEmail.WebApiClient-csharp/issues) or simply upvote or comment on existing issues or pull requests.

Please browse the rest of this README for further detail.

We appreciate your continued support, thank you!

# Table of Contents #
* Installation
* Quick Start
* About

# Installation #
## Prerequisites ##
* .NET version 4.5.2 and higher
* [An Elastic Email account](https://elasticemail.com/account/)
## Install Package ##
To use ElasticEmail in your C# project, you can either [download the ElasticEmail C# .NET libraries directly from our Github repository](https://github.com/ElasticEmail/ElasticEmail.WebApiClient-csharp) or, if you have the NuGet package manager installed, you can grab them automatically.

```
PM> Install-Package ElasticEmail.WebApiClient
```

Once you have the ElasticEmail.WebApiClient libraries properly referenced in your project, you can include calls to them in your code. 

# Quick Start #
## Send Email ##
The following is the minimum needed code to send an simple email:
```
using static ElasticEmail.WebApiClient.Api;
using static ElasticEmail.WebApiClient.ApiTypes;

namespace SendEmailExample
{
    class Program
    {
        static void Main(string[] args)
        {
            ElasticEmail.WebApiClient.Api.ApiKey = "YOUR-API-KEY";
            EmailSend emailSend = Email.Send("Hello World from Elastic Email!", "fromAddress@exmple.com", "John Tester", null, null, null, null, null, null, null, new string[] { "toAddress@exmple.com" }, null, null, null, null, null, null, "<h1>Hello! This mail was sent by Elastic Email service.<h1>", "Hello! This mail was sent by Elastic Email service.");
        }
    }
}
```
# About #
ElasticEmail.WebApiClient-csharp is guided and supported by the ElasticEmail Dev Team.

ElasticEmail.WebApiClient-csharp is maintained and funded by Elastic Email Inc. The names and logos for ElasticEmail.WebApiClien-csharp are trademarks of Elastic Email Inc.

![logo](https://elasticemail.com/files/ee_200x200.png )