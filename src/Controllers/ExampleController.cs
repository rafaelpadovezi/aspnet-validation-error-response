using ExampleApi.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExampleApi.Controllers
{ 
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
        [StringLength(1000)]
        public string Description { get; set; }
        [Range(1, 100)]
        public int SomeValue { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [IsEven]
        public int EvenNumber { get; set; }
    }
}