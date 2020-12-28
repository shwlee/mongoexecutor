namespace MongoExecutor.Mongo
{
	public class ScriptWrapper
	{
		public const string Etx = "~#GREG#~";

		public const string EtxCompleted = "\"~#GREG#~\" }";

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
			@"function(filter, document) {
	
}";
	}
}
