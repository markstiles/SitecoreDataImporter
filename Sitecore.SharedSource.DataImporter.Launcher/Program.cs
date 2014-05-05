using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.DataImporter.Providers;

namespace Sitecore.SharedSource.DataImporter.Launcher {

	public enum ExitCode : int {
		Success = 0,
		NullAssembly = 2,
		NullClass = 4,
		NullDatabase = 8,
		NullConnectionString = 16,
		NullImportItem = 32,
	}

	public class Program {
		static void Main(string[] args) {

			//param 0 = Assembly Name
			string assemblyName = GetArg(args, "-a");
			if (string.IsNullOrEmpty(assemblyName)) {
				Console.WriteLine("You need to specify an assembly");
				Environment.Exit((int)ExitCode.NullAssembly);
			}

			//param 1 = Class Name
			string className = GetArg(args, "-cn");
			if (string.IsNullOrEmpty(className)) {
				Console.WriteLine("You need to specify a class");
				Environment.Exit((int)ExitCode.NullClass);
			}

			//param 2 = database name
			string dbName = GetArg(args, "-d");
			Database scDB = null;
			try {
				scDB = Sitecore.Configuration.Factory.GetDatabase(dbName);
			} catch (InvalidOperationException ioe) {
				Console.WriteLine("You don't have a connection string for this database in your exe.config");
				Environment.Exit((int)ExitCode.NullDatabase);
			}
			if (scDB == null) {
				Console.WriteLine("You need to specify a database");
				Environment.Exit((int)ExitCode.NullDatabase);
			}

			//param 3 = connection string name
			string connStrName = GetArg(args, "-cs");
			string connStr = string.Empty;
			try {
				connStr = ConfigurationManager.ConnectionStrings[connStrName].ConnectionString;
			} catch(InvalidOperationException ioe){
				Console.WriteLine("You don't have a connection string for this database in your exe.config");
				Environment.Exit((int)ExitCode.NullDatabase);
			}
			if (string.IsNullOrEmpty(connStr)) {
				Console.WriteLine("You need to specify a connection string");
				Environment.Exit((int)ExitCode.NullConnectionString);
			}
	
			//param 4 = import definition item id
			string idStr = GetArg(args, "-i");
			if (string.IsNullOrEmpty(idStr)) {
				Console.WriteLine("You need to specify an import definition item ID");
				Environment.Exit((int)ExitCode.NullImportItem);
			} 
			Item importDefItem = null;
			if (ID.IsID(idStr)) {
				importDefItem = scDB.GetItem(ID.Parse(idStr));
			} else {
				Console.WriteLine("The import definition item ID you specified was not an item id");
				Environment.Exit((int)ExitCode.NullImportItem);
			}
			if (importDefItem == null) {
				Console.WriteLine("The import definition item ID you specified returned a null item");
				Environment.Exit((int)ExitCode.NullImportItem);
			}

			using (new SecurityDisabler()) {
				BaseDataMap map = (BaseDataMap)Sitecore.Reflection.ReflectionUtil.CreateObject(assemblyName, className, new object[] { scDB, connStr, importDefItem });
				string message = map.Process();
				Console.WriteLine(message);
			}
		}

		protected static string GetArg(string[] args, string argName) {
			for (int i = 0; i < args.Length; i++) {
				if (args[i] != argName)
					continue;
				int nextPos = i + 1;
				if (args.Length <= nextPos || args[nextPos] == null || args[nextPos].StartsWith("-"))
					continue;
				return args[nextPos];
			}
			return string.Empty;
		}
	}
}
