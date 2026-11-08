\# RaceDay — Part 3



\## Project Overview



RaceDay is a full-stack ASP.NET Core application designed to help users discover and participate in running, walking, and cycling events in South Africa.



The system supports two main user roles: \*\*Organiser\*\* and \*\*Participant\*\*.



Organisers can manage events, create event categories, and record race results. Participants can browse events, enrol in available categories, and view their race results.



This repository contains Part 3 of the RaceDay project, including the completed application, automated tests, and GitHub Actions continuous integration workflow.



\## Technologies Used



\- C# and .NET 8

\- ASP.NET Core Web API

\- ASP.NET Core MVC

\- Entity Framework Core

\- Microsoft SQL Server

\- HTML, CSS, JavaScript and Razor Views

\- xUnit for automated testing

\- Git and GitHub

\- GitHub Actions for CI/CD

\- Visual Studio 2022



\## Solution Structure



```text

RaceDay\_Part3/

├── .github/

│   └── workflows/

│       └── dotnet.yml

├── RaceDay.Api/

├── RaceDay.Mvc/

├── RaceDay.Tests/

├── RaceDay.sln

├── .gitignore

└── README.md

```



\*\*RaceDay.Api\*\* — Contains the Web API, controllers, models, database context and business logic.



\*\*RaceDay.Mvc\*\* — Contains the MVC web interface, controllers and Razor views.



\*\*RaceDay.Tests\*\* — Contains automated tests for the application.



\## Main Features



\### Authentication and User Roles



\- User registration and login

\- Logout functionality

\- Role-based access for Organisers and Participants

\- Session and cookie-based authentication



\### Event Management



\- Organisers can create events

\- Organisers can edit event details

\- Events can be published

\- Organisers can create event categories

\- Participants can browse available events



\### Participant Enrolment



\- Participants can enrol in event categories

\- Duplicate enrolments are prevented

\- Participants can view their enrolments



\### Race Results



\- Organisers can record participant race results

\- Finish times and finishing positions can be stored

\- Participants can view their recorded results

\- Duplicate result recording is prevented



\## Prerequisites



Before running the application, install or have access to:



\- Visual Studio 2022 with ASP.NET and web development support

\- .NET 8 SDK

\- SQL Server or SQL Server Express

\- Git



\## Installation and Setup



\### 1. Clone the Repository



```bash

git clone https://github.com/ST10458344/RaceDay\_Part\_3.git

cd RaceDay\_Part\_3

```



\### 2. Restore Dependencies



```bash

dotnet restore RaceDay.sln

```



\### 3. Configure the Database



Configure the `DefaultConnection` connection string in your local application configuration for `RaceDay.Api`.



Use your own SQL Server instance and keep database credentials out of source control.



\### 4. Apply Entity Framework Core Migrations



From the solution directory, run:



```bash

dotnet ef database update --project RaceDay.Api --startup-project RaceDay.Api

```



Alternatively, use Visual Studio's Package Manager Console with the API project selected.



\### 5. Run the API



```bash

dotnet run --project RaceDay.Api

```



The API's configured local development HTTPS address is:



`https://localhost:7060`



Swagger is available in Development mode at:



`https://localhost:7060/swagger/index.html`



\### 6. Run the MVC Application



Open a second terminal and run:



```bash

dotnet run --project RaceDay.Mvc

```



The MVC application's configured local development HTTPS address is:



`https://localhost:7049`



Keep both applications running while testing the website.



\## Automated Testing



The solution includes an xUnit test project.



To execute the automated tests:



```bash

dotnet test RaceDay.sln

```



The latest locally confirmed test results were:



\- Total tests: 2

\- Passed: 2

\- Failed: 0

\- Skipped: 0



\## GitHub Actions CI/CD



The repository contains an automated GitHub Actions workflow at:



`.github/workflows/dotnet.yml`



The workflow runs when changes are pushed, when pull requests are created, or when it is manually triggered.



The workflow performs the following operations:



1\. Checks out the repository.

2\. Installs the .NET 8 SDK.

3\. Restores project dependencies.

4\. Builds the solution in Release configuration.

5\. Runs automated tests.



\### Successful CI/CD Execution



The first GitHub Actions workflow completed successfully on 8 November 2026.



\- Workflow: RaceDay Part 3 - Build and Test

\- Branch: `main`

\- Commit: `ca9def1`

\- Duration: 26 seconds

\- Result: Success



\[View GitHub Actions workflow](https://github.com/ST10458344/RaceDay\_Part\_3/actions)



!\[Successful GitHub Actions workflow](images/github-actions-success.png)



\## Project Demonstration



An unlisted YouTube demonstration will be provided as part of the final submission.



The demonstration will cover registration, authentication, event management, participant enrolment, race result recording, and automated testing.



\*\*YouTube demonstration link:\*\* To be added.



\## Repository



GitHub: https://github.com/ST10458344/RaceDay\_Part\_3



\## Author



Tumelo Tshepang Matshele





