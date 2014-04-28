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
			string assemblyName = string.Empty;
			if (args.Length > 0)
				assemblyName = args[0];
			if (string.IsNullOrEmpty(assemblyName)) {
				Console.WriteLine("You need to specify an assembly");
				Environment.Exit((int)ExitCode.NullAssembly);
			}

			//param 1 = Class Name
			string className = string.Empty;
			if (args.Length > 1)
				className = args[1];
			if (string.IsNullOrEmpty(className)) {
				Console.WriteLine("You need to specify a class");
				Environment.Exit((int)ExitCode.NullClass);
			}

			//param 2 = database name
			Database scDB = null;
			if (args.Length > 2)
				scDB = Sitecore.Configuration.Factory.GetDatabase(args[2]);
			if(scDB == null) {
				Console.WriteLine("You need to specify a database");
				Environment.Exit((int)ExitCode.NullDatabase);
			}

			//param 3 = connection string name
			string connStr = null;
			if (args.Length > 3)
				connStr = ConfigurationManager.ConnectionStrings[args[3]].ConnectionString;
			if (string.IsNullOrEmpty(connStr)) {
				Console.WriteLine("You need to specify a connection string");
				Environment.Exit((int)ExitCode.NullConnectionString);
			}
	
			//param 4 = import definition item id
			Item importDefItem = null;
			if (args.Length > 4) {
				string idStr = args[4];
				if(ID.IsID(idStr))
					importDefItem = scDB.GetItem(ID.Parse(idStr));
			}
			if (importDefItem == null) {
				Console.WriteLine("You need to specify an import definition item ID");
				Environment.Exit((int)ExitCode.NullImportItem);
			}

			using (new SecurityDisabler()) {
				BaseDataMap map = (BaseDataMap)Sitecore.Reflection.ReflectionUtil.CreateObject(assemblyName, className, new object[] { scDB, connStr, importDefItem });
				string message = map.Process();
				Console.WriteLine(message);
			}
		}
	}
}
