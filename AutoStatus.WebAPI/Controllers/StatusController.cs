using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoStatus.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "value1";
        }

        [HttpGet]
        public ActionResult<string> GetMailBody()
        {
            var repo = new StatusRepository();
            var result =  repo.GetMailBody().Result;
            return result;
        }

        [HttpGet]
        public ActionResult<bool> SendMail(string htmlString)
        {
            var repo = new StatusRepository();
            repo.SendMail(htmlString);
            return true;
        }

    }
}