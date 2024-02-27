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

namespace bindecy.Classes
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
                _logger.LogInformation($"successfully run chmod command for {path}");
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
                    AddNewOperationByFileToOperationsCounter(path, read, write);
                }
            }
            Counter++;
            FilePermissionReq filePermissionReq = new FilePermissionReq(path,read,write);
            RequestsSaver.Add(Counter, filePermissionReq);
            _logger.LogInformation($"successfully added to RequestsSaver with the key {Counter}");
            return Counter;
        }

        public void UnRegister(int handle)
        {
            var operationToCancel = RequestsSaver[handle];
            _logger.LogInformation($"start unregister to {operationToCancel.path} file with read={operationToCancel.read} and write={operationToCancel.write}");
            if (!RequestsSaver[handle].isUnRegister)
            {
                bool cancelRead = operationToCancel.read && (OperationsCounter[operationToCancel.path].NumberOfReadCalls == 1);
                bool cancelWrite = operationToCancel.write && (OperationsCounter[operationToCancel.path].NumberOfWriteCalls == 1);
                setFilePermission(operationToCancel.path, cancelRead, cancelWrite, false);
                if (operationToCancel.read && DecreaseValue(OperationsCounter[operationToCancel.path].NumberOfReadCalls))
                {
                    OperationsCounter[operationToCancel.path].NumberOfReadCalls--;
                }
                if (operationToCancel.write && DecreaseValue(OperationsCounter[operationToCancel.path].NumberOfWriteCalls))
                {
                    OperationsCounter[operationToCancel.path].NumberOfWriteCalls--;
                }
                RequestsSaver[handle].isUnRegister = true;
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
            string command = "o";
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

        private bool DecreaseValue(int val)
        {
            if (val != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddNewOperationByFileToOperationsCounter(string path,bool read,bool write)
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
    }

}

