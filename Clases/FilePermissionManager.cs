using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using bindecy.Interfaces;
using bindecy.Models;

namespace bindecy.Clases
{
    public class FilePermissionManager : IFilePermissionInterface
    {
        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string pathname, int mode);
        // user permissions
        const int S_IRUSR = 0x100;
        const int S_IWUSR = 0x80;
        // group permission
        const int S_IRGRP = 0x20;
        const int S_IWGRP = 0x10;
        // other permissions
        const int S_IROTH = 0x4;
        const int S_IWOTH = 0x2;

        public int Counter { get; set; }

        //Chilkat.Ftp2 ftp = new Chilkat.Ftp2();

        public Dictionary<int,FilePermissionReq> RequestsSaver { get; set; }

        public Dictionary<string,OperationsByFile> OperationsCounter { get; set; }

        public int Register(string path, bool read, bool write)
        {

            setFilePermission(path, read, write);
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
                                setFilePermission(operationToCansle.path, !operationToCansle.read, !operationToCansle.write);
                                //run chmod only on read
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1 && !(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                setFilePermission(operationToCansle.path, !operationToCansle.read, !operationToCansle.write);
                                //run chmod only on write
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (!(OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1) && !(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                setFilePermission(operationToCansle.path, !operationToCansle.read, !operationToCansle.write);
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
                                setFilePermission(operationToCansle.path, !operationToCansle.read, !operationToCansle.write);
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
                                setFilePermission(operationToCansle.path, !operationToCansle.read, !operationToCansle.write);
                                //run chmod only on write
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

        private void setFilePermission(string path,bool read,bool write)
        {
            if (read || write)
            {
                int permission = 0;
                if (read && write)
                {
                    permission = S_IRUSR | S_IWUSR | S_IRGRP | S_IROTH | S_IWGRP | S_IWOTH;
                }
                else if (!read && write)
                {
                    permission = S_IWUSR | S_IWGRP | S_IWOTH;
                }
                else if (!read && write)
                {
                    permission = S_IRUSR | S_IRGRP | S_IROTH;
                }

                var res =chmod(path, (int)permission);
            }
        }
    }
	
}

