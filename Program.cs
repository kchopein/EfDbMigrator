using CommandLine;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;

namespace Kchopein.EfDbMigrator
{
    internal class Program
    {
        public class Options
        {
            [Option(longName: "publishFolder", Required = false, HelpText = "Folder containing the published DLLs. Defaults to the working folder.")]
            public string PublishFolder { get; set; }

            [Option(longName: "dbContextAssembly", Required = true, HelpText = "The assembly containing the DbContext to use.")]
            public string DbContextAssembly { get; set; }

            [Option(longName: "dbContextTypeName", Required = true, HelpText = "Name of the DbContext to use.")]
            public string DbContextTypeName { get; set; }

            [Option(longName: "connectionString", Required = true, HelpText = "The connection string.")]
            public string ConnectionString { get; set; }
        }

        /// <summary>
        /// Mains the specified publish folder.
        /// </summary>
        /// <param name="publishFolder">Folder containing the published DLLs.</param>
        /// <param name="dnContextAssembly">The assembly containing the DbContext to use.</param>
        /// <param name="dbContextTypeName">Name of the DbContext to use.</param>
        /// <param name="connectionString">The connection string.</param>
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    LoadAssemblies(o.PublishFolder ?? Directory.GetCurrentDirectory());

                    var dbContextAssemblyName = o.DbContextAssembly.Remove(o.DbContextAssembly.ToUpper().IndexOf(".DLL"));

                    var contextDll = AssemblyLoadContext.Default.Assemblies.Single(a => a.GetName().Name == dbContextAssemblyName);
                    var dbContextType = contextDll.GetTypes().Single(t => t.Name == o.DbContextTypeName);

                    var DbContextOptionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType);

                    var optionsBuilder = Activator.CreateInstance(DbContextOptionsBuilderType) as DbContextOptionsBuilder;
                    optionsBuilder.UseSqlServer(o.ConnectionString);

                    var dbContext = Activator.CreateInstance(dbContextType, optionsBuilder.Options) as DbContext;

                    dbContext.Database.Migrate();
                });
        }

        private static void LoadAssemblies(string folder)
        {
            //var dlls = Directory.GetFiles($"{folder}{Path.DirectorySeparatorChar}*.dll");
            var dlls = Directory.GetFiles(folder, "*.dll");

            foreach (var dll in dlls)
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
            }
        }
    }
}