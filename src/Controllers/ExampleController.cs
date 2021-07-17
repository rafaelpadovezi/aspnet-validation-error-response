using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExampleApi.Controllers
{ 
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get(int id)
        {
            if (id == 1)
                return Ok(new ExampleRequest{Name = "Example1"});
            
            return NotFound();
        }

        [HttpPost]
        public ActionResult Add(ExampleRequest example)
        {
            return Ok();
        }
    }

    public class ExampleRequest
    {
        [Required]
        public string Name { get; set; }
    }
}