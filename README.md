# Open-Blackboard
Purpose of this framework is to build a lightweight portable infrastructure to quickly create client &mdash; web and desktop &mdash; and server applications to collect, interactively analyze and explore data for clinical trials.

Configuration based: the entire toolchain is based on configuration. Each aspect can be customized and adapted editing text documents, usually there is no need to recompile anything to support a new protocol for a new clinical trial: add a new JSON description of the protocol and everything immediately works as expected.

## Server
Central repository is built on .NET Core to have the flexibility to host your server on Windows, Linux or Mac using IIS, a self-hosted process and a Nginx front-end. You will have then the opportunity to scale your requirements hosting your application on a standalone machine, on your own server or on Microsoft Azure App Service with a minimum configuration effort.

## Client
Native client applications can be used to both insert and explore data. Mobile-first HTML5 Web Application or Windows Universal App client can provide best native experience for almost every platform. Data, when  stored in the cloud, are accessible instantly from everywhere and can be exported to Microsoft Excel or to Adobe PDF documents to quickly create data reports. Server may be configured with different access privileges giving to each client exactly the required permissions to insert, view and analyze the entire dataset for one center or from all aggregated centers.

## Interoperability
Server exposes a RESTful API which can be accessed from any client application regardless the platform and the technology used to build it. Client libraries &mdash; available in C++, C#, Java and Python &mdash; make integration with existing application easy and smooth. A bunch of PowerShell cmdlets also simplify integration with in-place processes to automate data submitting (for example triggered on database changes.)

Using a portable local _connector_ data can be sent to a local service which also performs transformations and mapping between different schemas; for example a protocol may require just a boolean flag for patients with dyabet but your software uses different values to distinguish between different dyabet types, this mapping is completely transparent.

