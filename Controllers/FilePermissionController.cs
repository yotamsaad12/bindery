using System;
using System.ComponentModel;
using System.IO;
using bindecy.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace bindecy.Controllers;

[ApiController]
[Route("[controller]")]
public class FilePermissionController : ControllerBase
{
    private readonly ILogger<FilePermissionController> _logger;

    private readonly IFilePermissionInterface _filePermissiomHandler;

    public FilePermissionController(ILogger<FilePermissionController> logger ,IFilePermissionInterface filePermissiomHandler)
    {
        _logger = logger;
        _filePermissiomHandler = filePermissiomHandler;
    }
    
    [HttpPost]
    [Route("Register")]
    public IActionResult Register(string path,bool read,bool write)
    {
        try
        {
            var res = _filePermissiomHandler.Register(path, read, write);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpPost]
    [Route("UnRegister")]
    public IActionResult UnRegister(int handle)
    {
        try
        {
            _filePermissiomHandler.UnRegister(handle);
            return Ok($"operation {handle} finished");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}



