namespace MongoShellTest
{
	public class ScriptWrapper
	{
		public const string Etx = "~#GREG#~";

		public const string EtxComplated = "\"~#GREG#~\" }";

		public const string Error = "@(shell) ";

		public const string Execute =
			@"function execute(filter, document){{	
	const action = ({0});
	const result ={{
		value : action(filter, document),
		end : '~#GREG#~' }}
	
	return result;	
}}";

		public const string EmptyFunction =
			@"function(filter, document){{
	
}}";
		//		public const string Execute =
		//@"function execute(filter, document){{
		//	const action = ({0});
		//	return result = action(filter, document);

		//}}
		//";

		//		public const string EmptyFunction =
		//			@"function(filter, document){

		//}
		//";

		//		public const string Documents =
		//@"{
		//	""document"" : {
		//		""Alarms"" : [""20:00:00:00:00:01"", ""20:00:00:00:00:02""]
		//	}
		//}";
		//		public static string GenerateFunctionString(string functions)
		//		{
		//			var functionString = string.Format(ScriptWrapper.Execute, functions);
		//			return functionString;
		//		}

	}
}
