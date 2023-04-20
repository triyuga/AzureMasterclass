
using System.Reflection;
using DbUp;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace AzureMasterclass.DbUp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static int Main(string[] args)
        {   
            var app = new CommandLineApplication
            {
                Name = Assembly.GetExecutingAssembly().GetName().Name,
                Description = "Database creation and migrations"
            };

            app.HelpOption("-?|-h|--help");
            var resetOption = app.Option("-r|--reset", "Reset the database (delete all existing data)", CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var connectionString = GetConnectionString();

                // create database if not exist
                EnsureDatabase.For.SqlDatabase(connectionString);
                
                if (resetOption.HasValue())
                {
                    ResetDatabase(connectionString);
                }

                var upgrader =DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.Error);
                    Console.ResetColor();
#if DEBUG
                    Console.ReadLine();
#endif
                    return -1;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success!");
                Console.ResetColor();
                return 0;
            });

            return app.Execute(args);
        }

        private static string GetConnectionString() {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            var connectionString = configuration.GetConnectionString("DbUpMasterclass");
            return connectionString!;
        }
        
        private static void ResetDatabase(string connectionString)
        {
            const string resetDbScript = @"
                    --
                    -- This script drops all non-system objects (tables, views, SP, functions and so on)
                    --
                    DECLARE @SQL nvarchar(max) = '';

                    -- procedures
                    SELECT @SQL += 'DROP PROCEDURE ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.procedures WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- check constraints
                    SET @SQL = '';
                    SELECT @SQL += 'ALTER TABLE ['+SCHEMA_NAME([schema_id])+'].['+OBJECT_NAME([parent_object_id])+'] DROP CONSTRAINT ['+name+'];
                    ' FROM sys.check_constraints WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- functions
                    SET @SQL = '';
                    SELECT @SQL += 'DROP FUNCTION ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.objects WHERE TYPE IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- views
                    SET @SQL = '';
                    SELECT @SQL += 'DROP VIEW ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.views WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- foreign keys
                    SET @SQL = '';
                    SELECT @SQL += 'ALTER TABLE ['+SCHEMA_NAME([schema_id])+'].['+OBJECT_NAME(parent_object_id)+'] DROP CONSTRAINT ['+[name]+'];
                    ' FROM sys.foreign_keys WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- tables
                    SET @SQL = '';
                    SELECT @SQL += 'DROP TABLE ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.tables WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- user defined types
                    SET @SQL = '';
                    SELECT @SQL += 'DROP TYPE ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.types WHERE is_user_defined = 1;
                    EXEC (@SQL);

                    -- sequences
                    SET @SQL = '';
                    SELECT @SQL += 'DROP SEQUENCE ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.sequences WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- synonyms
                    SET @SQL = '';
                    SELECT @SQL += 'DROP SYNONYM ['+SCHEMA_NAME([schema_id])+'].['+[name]+'];
                    ' FROM sys.synonyms WHERE is_ms_shipped = 0;
                    EXEC (@SQL);

                    -- fulltext catalogs
                    SET @SQL = '';
                    SELECT @SQL += 'DROP FULLTEXT CATALOG ['+[name]+'];
                    ' FROM sys.fulltext_catalogs;
                    EXEC (@SQL);
                    ";
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Resetting the database ...{connectionString} <EOF> ");
            using var dbConnection = new SqlConnection(connectionString);
            dbConnection.Open();
            using var cmd = dbConnection.CreateCommand();
            cmd.CommandText = resetDbScript;
            cmd.ExecuteNonQuery();
            Console.WriteLine("Database reset successfully");
            Console.ResetColor();
        }
    }
}

