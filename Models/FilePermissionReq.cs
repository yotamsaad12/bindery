using System;
namespace bindecy.Models
{
	public class FilePermissionReq
	{
		public string path { get; set; }

		public bool read { get; set; }

		public bool write { get; set; }

		public bool isUnRegister { get; set; }

		public FilePermissionReq(string _path,bool _read ,bool _write)
		{
			path = _path;
			read = _read;
			write = _write;
			isUnRegister = false;
		}
	}
}

