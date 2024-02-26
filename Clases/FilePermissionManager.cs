using System;
using bindecy.Interfaces;
using bindecy.Models;

namespace bindecy.Clases
{
    public class FilePermissionManager : IFilePermissionInterface
    {
        public int Counter { get; set; }

        public Dictionary<int,FilePermissionReq> RequestsSaver { get; set; }

        public Dictionary<string,OperationsByFile> OperationsCounter { get; set; }

        public int Register(string path, bool read, bool write)
        {
            throw new NotImplementedException();
        }

        public void UnRegister(int handle)
        {
            throw new NotImplementedException();
        }

        public FilePermissionManager()
        {
            Counter = 1;
        }
    }
	
}

