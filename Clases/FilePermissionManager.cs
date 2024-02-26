using System;
using System.ComponentModel;
using System.IO;
using bindecy.Interfaces;
using bindecy.Models;

namespace bindecy.Clases
{
    public class FilePermissionManager : IFilePermissionInterface
    {
        public int Counter { get; set; }

        //Chilkat.Ftp2 ftp = new Chilkat.Ftp2();

        public Dictionary<int,FilePermissionReq> RequestsSaver { get; set; }

        public Dictionary<string,OperationsByFile> OperationsCounter { get; set; }

        public int Register(string path, bool read, bool write)
        {
            //run chmod
            if (OperationsCounter.ContainsKey(path))
            {
                if (read)
                {
                    OperationsCounter[path].NumberOfReadCalls++;
                }
                if (write)
                {
                    OperationsCounter[path].NumberOfWriteCalls++;
                }
                
            }
            else
            {
                OperationsByFile operationsByFile = new OperationsByFile();
                if (read)
                {
                    operationsByFile.NumberOfReadCalls++;
                }
                if (write)
                {
                    operationsByFile.NumberOfWriteCalls++;
                }
                OperationsCounter.Add(path, operationsByFile);
            }
            Counter++;
            FilePermissionReq filePermissionReq = new FilePermissionReq(path,read,write);
            RequestsSaver.Add(Counter, filePermissionReq);
            return Counter;
        }

        public void UnRegister(int handle)
        {
            var operationToCansle = RequestsSaver[handle];
            if (RequestsSaver.ContainsKey(handle))
            {
                if (!RequestsSaver[handle].isUnRegister)
                {
                    if (OperationsCounter.ContainsKey(operationToCansle.path)){

                        if (operationToCansle.read && operationToCansle.write) 
                        {
                            if(OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1 && OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1)
                            {
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (!(OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1) && OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1)
                            {
                                //run chmod only on read
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1 && !(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                //run chmod only on write
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (!(OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1) && !(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                //run chmod on write and read
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                        }
                        if (operationToCansle.read && !operationToCansle.write)
                        {
                            if (OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1)
                            {
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (!(OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1))
                            {
                                //run chmod only on read
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                        }
                        if (!operationToCansle.read && operationToCansle.write)
                        {
                            if (OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1)
                            {
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (!(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                //run chmod only on read
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                        }
                    }
                }

            }
        }

        public FilePermissionManager()
        {
            RequestsSaver = new Dictionary<int, FilePermissionReq>();
            OperationsCounter = new Dictionary<string, OperationsByFile>();
            Counter = 1;
        }
    }
	
}

