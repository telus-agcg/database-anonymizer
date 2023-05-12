# Database Anonymizer

The Database Anonymizer is a cross-platform tool written in C# (.NET 6.0+) which makes it easy to anonymize the data within a database in a declarative way.

### Purpose

A typical example of its usage would be, for example, if your developers need to work with a fresh copy of the production database, one cannot simply provide them with a local or remote copy of the database because of some data might be considered as a personal identifiable information (PII). Should the database then be in the possession of an unauthorized entity (person or organization), the confidentiality of that data will be breached. The power of the Database Anonymizer is that it can be used to effectively anonymize any data that will no longer exposer any PII, and as such can be safely and legally used by the developers to work with. 

## Supported Databases

Currently, the initial version of the Database Anonymizer supports **Microsoft SQL Server**. One can submit a pull request to augment the scope of databases that can be anonymized by writing a specific provider for it, such as one for Postgres SQL, MySQL, etc.

## Getting Started

1. Create a JSON configuration file with the correct data source and tables to anonymize. You can refer to the `configAdventureWorks.example.json` file as an example. You can then specify the filename to the configuration file when running the application:
```
DatabaseAnonymizer.exe configAdventureWorks.example.json
```
2. If you need to run specific SQL commands on the database prior to running the anonymization process, you can do that by updating the `SpecificScripts.sql` file. You can take a look at that file as an example.
3. And that's it. The anonymization is configured in a declarative way. Given that the binary is an executable file, you can integrate it within a scheduler to run whenever you need your database to be anonymized.

## Try it in action

The example configuration is written to work with [AdventureWorks](https://learn.microsoft.com/en-us/sql/samples/adventureworks-install-configure?view=sql-server-ver16&tabs=ssms) Database. So feel free to download it, restore on your local SQL Servel instance and start the app. You will have anonymized Person table ( FirstName, MiddleName, LastName ) and PersonPhone table ( PhoneNumber ) after the execution. 
```
DatabaseAnonymizer.exe configAdventureWorks.example.json
```

## Contribution Ideas

The application works for us, but we're sure that it can be further refactored to be more generic. We accept pull requests to make this project easier to use. Though the application emits logs about its process, another idea can be to send notifications to specific sinks (such as Slack or email) whenever the anonymization process has completed. Furthermore, this README file can always be updated to make it easier to read.