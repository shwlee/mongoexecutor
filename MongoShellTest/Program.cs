using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MongoShellTest
{
	class Program
	{
		private static Process _mongo;

		static void Main(string[] args)
		{
			try
			{
				var url = "mongodb://localhost:27017";
				var db = "test";
				var arg = $"{url}/{db}";
				//var arg = $"{url}";

				var path =
					"D:/Projects/IPWall/Performer/Services/Performer.App/bin/Debug/modules/mongo/b7fd2450a5103bc3_mongoquery.js";

				_mongo = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "mongo.exe",
						Arguments = arg,
						UseShellExecute = false,
						WindowStyle = ProcessWindowStyle.Hidden,
						StandardOutputEncoding = Encoding.UTF8,
						RedirectStandardInput = true,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				_mongo.Start();
				_mongo.BeginOutputReadLine();
				_mongo.OutputDataReceived += Received;
				_mongo.StandardInput.WriteLine($"load(\"{path}\")");
				while (true)
				{
					var input = Console.ReadLine();
					if (input == "q")
					{
						return;
					}



					_mongo.StandardInput.AutoFlush = true;
					//proc.StandardInput.WriteLine("use ksnc");

					//_mongo.StandardInput.WriteLine($"printjson(execute(null, {ScriptWrapper.Documents}))");
					//proc.StandardInput.WriteLine();
					//proc.StandardInput.Flush();

					//proc.StandardInput.WriteLine("quit()");

					//proc.StandardInput.Write("quit()");
					//proc.StandardInput.WriteLine();
					//proc.StandardInput.Flush();

					//proc.OutputDataReceived -= Received;

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"MongoDbHandler query error in CMD. {ex}");
			}
		}

		private static void Received(object sender, DataReceivedEventArgs e)
		{
			//var output = Proc.StandardOutput;
			Console.WriteLine(e.Data);
		}
	}
}
