using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using bindecy.Interfaces;
using bindecy.Models;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Windows.Input;

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
                setFilePermission(path, read, write,true);
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
                                    setFilePermission(operationToCansle.path, operationToCansle.read, operationToCansle.write, false);
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
                                    setFilePermission(operationToCansle.path, operationToCansle.read, operationToCansle.write, false);
                                    OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                }
                                RequestsSaver[handle].isUnRegister = true;
                            }
                            else if (OperationsCounter[operationToCansle.path].NumberOfWriteCalls == 1 && OperationsCounter[operationToCansle.path].NumberOfReadCalls == 1)
                            {
                                setFilePermission(operationToCansle.path, operationToCansle.read, operationToCansle.write, false);
                                OperationsCounter[operationToCansle.path].NumberOfWriteCalls--;
                                OperationsCounter[operationToCansle.path].NumberOfReadCalls--;
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
                            else if (OperationsCounter[operationToCansle.path].NumberOfReadCalls == 1)
                            {
                                setFilePermission(operationToCansle.path, operationToCansle.read, operationToCansle.write, false);
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
                            else if (OperationsCounter[operationToCansle.path].NumberOfWriteCalls == 1)
                            {
                                setFilePermission(operationToCansle.path, operationToCansle.read, operationToCansle.write, false);
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

        private void setFilePermission(string path, bool read, bool write,bool isAddMode)
        {
            Process process = new Process();
            string command = BuildCommand(path, isAddMode, read, write);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {command}";
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix ||
                     Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c {command}";
            }
            else
            {
                throw new NotSupportedException("Unsupported operating system.");
            }
            process.StartInfo.UseShellExecute = true; 
            process.StartInfo.CreateNoWindow = true; 

            process.Start();
            process.WaitForExit();

        }

        private string BuildCommand(string path,bool isAddMode, bool read, bool write)
        {
            string command = "";
            if (isAddMode)
            {
                command = "chmod +";
            }
            else
            {
                command = "chmod -";
            }
            if (read)
            {
                command = command + "r";
            }
            if (write)
            {
                command = command + "w";
            }
            command = command + " " + path;

            return command;
        }
    }

}

