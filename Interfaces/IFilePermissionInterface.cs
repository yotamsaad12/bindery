using System;
namespace bindecy.Interfaces
{
	public interface IFilePermissionInterface
	{
		public int Register(string path, bool read, bool write);

		public void UnRegister(int handle);
	}
}

