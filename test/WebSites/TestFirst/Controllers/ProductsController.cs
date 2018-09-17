using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TestFirst.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        [HttpPost]
        public IActionResult CreateProduct([FromBody]Product product)
        {
            if (!this.ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            //return new StatusCodeResult(201);
            return Created("api/products/1", null);
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            return new ObjectResult(new[] { new Product { Id = 1, Name = "Test product" } });
        }
    }

    public class Product
    {
        public int Id { get; internal set;}

        [Required]
        public string Name { get; set; }
    }
}
