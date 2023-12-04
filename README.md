# TestTask
 Этот проект представляет собой систему для обработки XML файлов с использованием микросервисного подхода в среде .NET. Проект включает два основных сервиса: FileParser Service и DataProcessor Service, которые взаимодействуют через RabbitMQ. Все данные сохраняются в локальной базе данных SQLite.

Требования
Для успешного развертывания и запуска системы, вам потребуется:

.NET SDK для сборки и запуска проекта.
RabbitMQ для обеспечения взаимодействия между сервисами.
SQLite для хранения данных.

Шаги по установке и запуску

1)Клонирование репозитория
2)Конфигурирование 
Убедитесь, что RabbitMQ установлен и запущен на вашем сервере.
Проверьте конфигурационные файлы FileParser/appsettings.json и DataProcessor/appsettings.json для корректности параметров подключения к RabbitMQ,указания диска для чтения файлов и конфигурации Базы Данных.

Изменяемые параметры:
Оба сервиса
RabbitMQ:ConnectionString - строка подключения к RabbitMQ.
RabbitMQ:QueueName - имя очереди RabbitMQ.

DataProcessor Service
ConnectionDBStrings:AppDbContext - строка подключения к базе данных SQLite.

FileParser Service
RabbitMQ:ExchangeName - имя обмена RabbitMQ.
RabbitMQ:RoutingKey - ключ маршрутизации RabbitMQ.
DirectoryPath - путь к директории, где хранятся XML файлы.


3.1) Для запуска FileParser следует перейти в каталог 
\FileParser

и выполнить

dotnet build
dotnet run

3.2) Для запуска DataProcessorследует перейти в каталог 

\DataProcessor

и выполнить

dotnet ef database update
dotnet build
dotnet run





This project is a system for processing XML files using a microservices approach in the .NET environment. The project includes two main services: the FileParser Service and the DataProcessor Service, which interact through RabbitMQ. All data is stored in a local SQLite database.

Requirements
To successfully deploy and run the system, you will need:

.NET SDK for building and running the project.
RabbitMQ for facilitating communication between services.
SQLite for data storage.
Installation and Running Steps

1)Clone the repository

2)Configuration
Ensure that RabbitMQ is installed and running on your server.
Check the configuration files FileParser/appsettings.json and DataProcessor/appsettings.json for the correctness of the connection parameters to RabbitMQ, specifying the disk for reading files, and configuring the database.

Editable parameters:
Both services

RabbitMQ:ConnectionString - connection string to RabbitMQ.
RabbitMQ:QueueName - RabbitMQ queue name.

DataProcessor Service

ConnectionDBStrings:AppDbContext - SQLite database connection string.

FileParser Service

RabbitMQ:ExchangeName - RabbitMQ exchange name.
RabbitMQ:RoutingKey - RabbitMQ routing key.
DirectoryPath - path to the directory where XML files are stored.
3.1) To run FileParser, navigate to the directory
\FileParser
and execute

dotnet build
dotnet run
3.2) To run DataProcessor, navigate to the directory
\DataProcessor
and execute

dotnet ef database update
dotnet build
dotnet run