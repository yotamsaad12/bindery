using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using bindecy.Interfaces;
using bindecy.Models;
using System.Security.AccessControl;

namespace bindecy.Clases
{
    public class FilePermissionManager : IFilePermissionInterface
    {

        public int Counter { get; set; }

        public Dictionary<int,FilePermissionReq> RequestsSaver { get; set; }

        public Dictionary<string,OperationsByFile> OperationsCounter { get; set; }

        public int Register(string path, bool read, bool write)
        {
            if(read || write)
            {
                addFilePermission(path, read, write);
            }
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
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                if (OperationsCounter[operationToCansle.path].NumberOfReadCalls == 1)
                                {
                                    removeFilePermission(operationToCansle.path, operationToCansle.read);
                                    OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                }
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1 && !(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                //run chmod only on write
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                if (OperationsCounter[operationToCansle.path].NumberOfWriteCalls == 1)
                                {
                                    removeFilePermission(operationToCansle.path, operationToCansle.write);
                                    OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                }
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (!(OperationsCounter[operationToCansle.path].NumberOfReadCalls > 1) && !(OperationsCounter[operationToCansle.path].NumberOfWriteCalls > 1))
                            {
                                //run chmod on write and read
                                if (OperationsCounter[operationToCansle.path].NumberOfWriteCalls == 1 && OperationsCounter[operationToCansle.path].NumberOfReadCalls == 1)
                                {
                                    removeFilePermission(operationToCansle.path,operationToCansle.read, operationToCansle.write);
                                    OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                    OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                }
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
                                if (OperationsCounter[operationToCansle.path].NumberOfReadCalls == 1)
                                {
                                    removeFilePermission(operationToCansle.path, operationToCansle.read);
                                    OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
                                }
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
                                //run chmod only on write
                                if (OperationsCounter[operationToCansle.path].NumberOfWriteCalls == 1)
                                {
                                    removeFilePermission(operationToCansle.path, operationToCansle.write);
                                    OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                }
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

        private void addFilePermission(string path, [Optional] bool read, [Optional] bool write)
        {
            FileInfo fileInfo = new FileInfo(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            if (read)
            {
                fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Allow));
            }
            if (write)
            {
                fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));
            }

            fileInfo.SetAccessControl(fileSecurity);
            
        }

        private void removeFilePermission(string path, [Optional] bool read, [Optional] bool write)
        {
            FileInfo fileInfo = new FileInfo(path);
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            if (read)
            {
                fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Read, AccessControlType.Deny));
            }
            if (write)
            {
                fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Deny));
            }

            fileInfo.SetAccessControl(fileSecurity);

        }
    }

}

