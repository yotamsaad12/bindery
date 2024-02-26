using System;
using Microsoft.AspNetCore.Mvc;

namespace bindecy.Controllers;

[ApiController]
[Route("[controller]")]
public class FilePermissionController : ControllerBase
{
    private readonly ILogger<FilePermissionController> _logger;



    public FilePermissionController(ILogger<FilePermissionController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost]
    [Route("Register")]
    public IActionResult Register(string path,bool read,bool write)
    {
        return Ok(1);
    }

    [HttpPost]
    [Route("UnRegister")]
    public IActionResult UnRegister(int handle)
    {
        return Ok(1);
    }
}



