using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using bindecy.Interfaces;
using bindecy.Models;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Windows.Input;
using bindecy.Controllers;

namespace bindecy.Clases
{
    public class FilePermissionManager : IFilePermissionInterface
    {
        private readonly ILogger<FilePermissionManager> _logger;

        private int Counter { get; set; }

        private Dictionary<int,FilePermissionReq> RequestsSaver { get; set; }

        private Dictionary<string,OperationsByFile> OperationsCounter { get; set; }

        public int Register(string path, bool read, bool write)
        {
            _logger.LogInformation($"register to {path} with read={read} and write={write}");
            if(read || write)
            {
                setFilePermission(path, read, write,true);
                _logger.LogInformation($"seccessfully run chmod command for {path}");
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
            _logger.LogInformation($"seccessfully added to RequestsSaver with the key {Counter}");
            return Counter;
        }

        public void UnRegister(int handle)
        {
            var operationToCansle = RequestsSaver[handle];
            _logger.LogInformation($"start unregister to {operationToCansle.path} file with read={operationToCansle.read} and write={operationToCansle.write}");
            if (!RequestsSaver[handle].isUnRegister)
            {
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
                            setFilePermission(operationToCansle.path, operationToCansle.read, false, false);
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
                            setFilePermission(operationToCansle.path, false, operationToCansle.write, false);
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
                else if (operationToCansle.read && !operationToCansle.write)
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
                else if (!operationToCansle.read && operationToCansle.write)
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
            else
            {
                _logger.LogInformation($"the call with the index {handle} is already done");
            }
        }

        public FilePermissionManager(ILogger<FilePermissionManager> logger)
        {
            RequestsSaver = new Dictionary<int, FilePermissionReq>();
            OperationsCounter = new Dictionary<string, OperationsByFile>();
            Counter = 0;
            _logger = logger;
        }

        private void setFilePermission(string path, bool read, bool write,bool isAddMode)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                _logger.LogInformation("operating system is windows");
                if (System.IO.File.Exists(path))
                {
                    var command = BuildCommandForWindows(path, isAddMode, read, write);
                    _logger.LogInformation($"the command :{command}");
                    Process.Start("icacls",command ).WaitForExit();
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix ||
                     Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                _logger.LogInformation("operating system is linux/mac");
                if (System.IO.File.Exists(path))
                {
                    var command = BuildCommandForMac(path, isAddMode, read, write);
                    _logger.LogInformation($"the command :{command}");
                    Process.Start("chmod",command ).WaitForExit();
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported operating system.");
            }

        }

        private string BuildCommandForMac(string path,bool isAddMode, bool read, bool write)
        {
            string command = "u";
            if (isAddMode)
            {
                command = "+";
            }
            else
            {
                command = "-";
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

        private string BuildCommandForWindows(string path, bool isAddMode, bool read, bool write)
        {
            string command = path;
            if (isAddMode)
            {
                command = command+ " /grant Everyone:(";
            }
            else
            {
                command = command + " /deny Everyone:(";
            }
            if (read)
            {
                command = command + "R";
            }
            if (write)
            {
                if (command[command.Length - 1].Equals("R"))
                {
                    command = command + ",W)";
                }
                else
                {
                    command = command + "W)";
                }
                
            }
            
            return command;
        }
    }

}

