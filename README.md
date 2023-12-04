# TestTask
 ���� ������ ������������ ����� ������� ��� ��������� XML ������ � �������������� ��������������� ������� � ����� .NET. ������ �������� ��� �������� �������: FileParser Service � DataProcessor Service, ������� ��������������� ����� RabbitMQ. ��� ������ ����������� � ��������� ���� ������ SQLite.

����������
��� ��������� ������������� � ������� �������, ��� �����������:

.NET SDK ��� ������ � ������� �������.
RabbitMQ ��� ����������� �������������� ����� ���������.
SQLite ��� �������� ������.

���� �� ��������� � �������

1)������������ �����������
2)���������������� 
���������, ��� RabbitMQ ���������� � ������� �� ����� �������.
��������� ���������������� ����� FileParser/appsettings.json � DataProcessor/appsettings.json ��� ������������ ���������� ����������� � RabbitMQ,�������� ����� ��� ������ ������ � ������������ ���� ������.

���������� ���������:
��� �������
RabbitMQ:ConnectionString - ������ ����������� � RabbitMQ.
RabbitMQ:QueueName - ��� ������� RabbitMQ.

DataProcessor Service
ConnectionDBStrings:AppDbContext - ������ ����������� � ���� ������ SQLite.

FileParser Service
RabbitMQ:ExchangeName - ��� ������ RabbitMQ.
RabbitMQ:RoutingKey - ���� ������������� RabbitMQ.
DirectoryPath - ���� � ����������, ��� �������� XML �����.


3.1) ��� ������� FileParser ������� ������� � ������� 
\FileParser

� ���������

dotnet build
dotnet run

3.2) ��� ������� DataProcessor������� ������� � ������� 

\DataProcessor

� ���������

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