using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace MongoExecutor.Mongo
{
	public class PrivilegeSetter
	{
		public const string SeIncreaseWorkingSetPrivilege = "SeIncreaseWorkingSetPrivilege";

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

		[DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
		internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

		[DllImport("advapi32.dll", SetLastError = true)]
		internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		internal struct TokPriv1Luid
		{
			public int Count;

			public long Luid;

			public int Attr;
		}

		internal const int SE_PRIVILEGE_ENABLED = 0x00000002;

		internal const int SE_PRIVILEGE_DISABLED = 0x00000000;

		internal const int TOKEN_QUERY = 0x00000008;

		internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;

		public static bool EnablePrivilege(int pid, string privilege, bool disable)
		{
			var processHandle = System.Diagnostics.Process.GetProcesses().FirstOrDefault(p => p.Id == pid);
			if (processHandle == null)
			{
				Console.WriteLine("Process not running.");
				return false;
			}

			TokPriv1Luid tp;

			var hproc = processHandle.Handle;

			var htok = IntPtr.Zero;

			var retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);

			tp.Count = 1;

			tp.Luid = 0;
			tp.Attr = disable ? SE_PRIVILEGE_DISABLED : SE_PRIVILEGE_ENABLED;
			retVal = LookupPrivilegeValue(null, privilege, ref tp.Luid);

			retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);

			return retVal;
		}
	}
}
