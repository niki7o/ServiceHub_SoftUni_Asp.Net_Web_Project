
ServiceHub е цялостно ASP.NET Core MVC уеб приложение, разработено като финален проект по курса ASP.NET Advanced - June 2025 @ SoftUni.

Проектът представлява централизирана платформа, която агрегира множество дигитални услуги в едно, като демонстрира ефективно използване на ключови технологии и архитектурни подходи на ASP.NET Core.

🛠 Технологии и инструменти
ASP.NET Core 8.0 MVC

Entity Framework Core с MS SQL Server

ASP.NET Identity за управление на потребители и достъп (User и Admin роли)

SignalR за комуникация в реално време (Live Search)

Dependency Injection за скалируемост и лесна поддръжка

Repository & Service Layer архитектура за разделяне на отговорностите

Tag Helpers & Partial Views за генериране на четим HTML и повторно използване на код

Web API за връзка с клиентския интерфейс

JSON Serialization (CamelCase, IgnoreCycles)

Client & Server Side Validation за сигурност и надеждност на данните

Deployment на Azure : тук https://servicehub20250807125912-hvhxf5gwephcf2ew.canadacentral-01.azurewebsites.net/

📌 Основни функционалности
Регистрация и логин на потребители с ролеви достъп.

Диспечер на услуги: Динамично извикване на различни услуги, базирано на избрания от потребителя идентификатор.

Услуги в реално време: Търсене на услуги с незабавни резултати, докато потребителят пише, благодарение на SignalR.

Колекция от услуги:

Генератор на договори и фактури

Генератор на автобиографии (CV)

Инструменти за работа с текст (брояч на думи, конвертор на регистър)

Генератор на пароли

Финансов калкулатор

Конвертор на код

Административен панел (Areas/Admin) за управление на потребителите и услугите.

Защитен достъп с [Authorize] атрибути.

Защита срещу CSRF атаки с [ValidateAntiForgeryToken].

📂 Архитектура
Структурата на проекта следва принципите на разделяне на отговорностите:

ServiceHub_SoftUni_Asp.Net_Web_Project/
│
├── ServiceHub.Common             # Константни стойности, помощни класове
├── ServiceHub.Data               # Entity модели, DbContext, миграции
├── ServiceHub.Services           # Бизнес логика, интерфейси и репозиторита
├── ServiceHub.Web                # Controllers, Views, Hubs, wwwroot, Program.cs
└── ...
