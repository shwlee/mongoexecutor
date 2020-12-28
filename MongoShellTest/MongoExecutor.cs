using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace MongoShellTest
{
	public class MongoExecutor : IDisposable
	{
		private Process _mongo;

		private const string Value = "value";

		private const string True = "true";

		private bool _startReceive;

		/// <summary>
		/// shell 에서 호출한 함수의 마지막 데이터를 받았는지 여부. 이 값이 true 되어야 데이터 수신이 마무리 된다.
		/// </summary>
		private bool _hasLast;

		private string ConnectUrl => $"{this.Url.TrimEnd('/')}/{this.DbName}";

		private const string ScriptFilePath = "mongo\\_mongoquery.js";

		private string _queryPath;

		private StringBuilder _messageBuilder;

		private BlockingCollection<string> _messageBag;

		private string _functions;

		public string Url { get; set; }

		public string Script { get; set; }

		public string DbName { get; set; }

		public string Functions
		{
			get => this._functions;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					value = ScriptWrapper.EmptyFunction;
				}

				if (string.Compare(this._functions, value, StringComparison.Ordinal) == 0)
				{
					return;
				}

				this._functions = value;
			}
		}

		public void Start()
		{
			// node 실행 전 mongodb shell 초기화.
			this._startReceive = true;

			this.InitShell(this.ConnectUrl);

			this._mongo.BeginOutputReadLine();

			this._messageBag.TryTake(out _, 500);

			this._startReceive = false;

			this._messageBuilder.Clear();

			this._mongo.CancelOutputRead();
		}

		public void Close()
		{
			this.Release();
		}

		private void InitShell(string arg)
		{
			this.Release();

			this.SaveQueryFile();

			this._messageBag = new BlockingCollection<string>();
			this._messageBuilder = new StringBuilder();

			this._mongo = new Process
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

			this._mongo.OutputDataReceived += this.OnShellDataReceived;

			this._mongo.Start();

			PrivilegeSetter.EnablePrivilege(this._mongo.Id, PrivilegeSetter.SeIncreaseWorkingSetPrivilege, true);

			this._mongo.StandardInput.AutoFlush = true;
			this._mongo.StandardInput.WriteLine($"load(\'{this._queryPath}\')");
		}

		public string ExecuteCommand(string filter, string document)
		{
			try
			{
				if (this._mongo == null)
				{
					this.InitShell(this.ConnectUrl);
				}

				this._startReceive = true;

				this._mongo.BeginOutputReadLine();

				var execute = $"execute({(string.IsNullOrWhiteSpace(filter) ? "\'\'" : filter)}, {(string.IsNullOrWhiteSpace(document) ? "\'\'" : document)})";
				this._mongo.StandardInput.WriteLine(execute);

				this._messageBag.TryTake(out var message, 30000); // 최대 30초 대기.

				this._startReceive = false;

				this._mongo.StandardInput.WriteLine("gc()");

				this._mongo.CancelOutputRead();

				var result = message;

				return result;
			}
			catch (Exception ex)
			{
				this.Release();

				Console.WriteLine($"MongoDbHandler query error in mongo shell. {ex}");
				return string.Empty;
			}
		}

		private void SaveQueryFile()
		{
			try
			{
				var functionString = this.GenerateFunctionString();
				var location = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
				var path = Path.Combine(location, ScriptFilePath);
				var dir = Path.GetDirectoryName(path);
				if (Directory.Exists(dir) == false)
				{
					Directory.CreateDirectory(dir);
				}

				this._queryPath = path.Replace("\\", "/");

				File.WriteAllText(path, functionString);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"mongo query file save error. \r{ex}");
			}
		}

		private string GenerateFunctionString()
		{
			var functionString = string.Format(ScriptWrapper.Execute, this.Script);
			return functionString;
		}

		private void OnShellDataReceived(object sender, DataReceivedEventArgs e)
		{
			try
			{
				if (this._hasLast)
				{
					// append last data.
					this.GetResult(e.Data);

					this._hasLast = false;

					return;
				}

				if (this._startReceive == false)
				{
					return;
				}

				if (string.Compare(e.Data, True, StringComparison.OrdinalIgnoreCase) == 0)
				{
					// shell message 는 무시한다.
					this._messageBuilder.Clear();

					return;
				}

				if (e.Data.Contains(ScriptWrapper.EtxComplated))
				{
					// append last data. 완전히 받음.
					this.GetResult(e.Data);

					return;
				}

				if (e.Data.Contains(ScriptWrapper.Error) || e.Data.Contains(ScriptWrapper.Etx))
				{
					// etx. 완전히 받지 않음.
					this._hasLast = true;
					this._messageBuilder.Append(e.Data);

					return;
				}

				this._messageBuilder?.Append(e.Data);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Mongo shell data received error. \r{ex}");
			}
		}

		private void GetResult(string data)
		{
			// append last data.
			this._messageBuilder.Append(data);

			var completeMessage = this._messageBuilder.ToString();

			this._messageBag.Add(completeMessage);
			this._messageBuilder.Clear();
		}

		private void Release()
		{
			this._messageBag?.CompleteAdding();
			this._messageBag?.Dispose();
			this._messageBag = null;

			this._messageBuilder?.Clear();
			this._messageBuilder = null;

			try
			{
				if (this._mongo != null)
				{
					this._mongo.OutputDataReceived -= this.OnShellDataReceived;
					this._mongo.CancelOutputRead();
					this._mongo.StandardInput.AutoFlush = true;
					this._mongo.StandardInput.WriteLine($"exit");
					this._mongo.WaitForExit(1);
				}

				this._mongo = null;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error mongo shell dispose. \r{ex}");
				this._mongo = null;
			}
		}

		public void Dispose()
		{
			this._mongo?.Dispose();
			this._messageBag?.Dispose();
		}
	}
}
