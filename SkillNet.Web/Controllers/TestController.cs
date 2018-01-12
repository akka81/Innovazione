using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SkillNet.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Test")]
    public class TestController : Controller
    {

       [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}