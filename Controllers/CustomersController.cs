using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Kendo.Mvc.UI;
using TodoApi.Util;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public CustomersController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Customers
        [HttpGet]
         public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customerList = await _repositoryWrapper.Customer.FindAllAsync();
            return Ok(customerList);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
         public async Task<ActionResult<Customer>> GetCustomer(long id)
        {
            var customer = await _repositoryWrapper.Customer.FindByIDAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }
        
        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
         public async Task<IActionResult> PutCustomer(long id, Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            TryValidateModel(customer); //server side validation by using TryValidateModel
            if(!ModelState.IsValid){
                return BadRequest(ModelState.ToList());
            }

            Customer? objCus;
            try
            {
                objCus = _repositoryWrapper.Customer.FindByID(id);
                if (objCus == null) 
                    throw new Exception("Invalid Customer ID");
                
                objCus.RegisterDate = customer.RegisterDate;
                objCus.CustomerName = customer.CustomerName;
                objCus.CustomerAddress = customer.CustomerAddress;
                objCus.CustomerTypeId = customer.CustomerTypeId;
                if(customer.CustomerPhoto!= null && customer.CustomerPhoto != "")
                {
                    //multi
                    FileService.MoveTempFileDir("CustomerPhoto", customer.Id.ToString(), customer.CustomerPhoto);
                }
                await _repositoryWrapper.Customer.UpdateAsync(objCus);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
         public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            try{
                Validator.ValidateObject(customer, new ValidationContext(customer), true);
                await _repositoryWrapper.Customer.CreateAsync(customer, true);
                if(customer.CustomerPhoto != null && customer.CustomerPhoto != "")
                {
                    //FOR multiple file upload
                   FileService.MoveTempFileDir("CustomerPhoto", customer.Id.ToString(), customer.CustomerPhoto);
                }
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch(ValidationException vex)
            {
                return BadRequest(vex.Message);
            } 
        }
        
        [HttpPost("showlist")]
        public async Task<JsonResult> PostCustomerGrid([DataSourceRequest]DataSourceRequest request)
        {
            //Task.Delay(3000).Wait(); //to test loading icon
            var dsmainQuery = new JsonResult (await _repositoryWrapper.Customer.GetCustomerInfoGrid(request));
            return dsmainQuery;
        }

        [HttpPost("report")]
        public async Task<dynamic> PostCustomerReport(Newtonsoft.Json.Linq.JObject param)
        {
            return await _repositoryWrapper.Customer.GetCustomerReport(param);
        }


        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(long id)
        {
            var customer = await _repositoryWrapper.Customer.FindByIDAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            //multi upload
            FileService.DeleteDir("CustomerPhoto", id.ToString());
            await _repositoryWrapper.Customer.DeleteAsync(customer, true);
            return NoContent();
        }

        private bool CustomerExists(long id)
        {
            return _repositoryWrapper.Customer.IsExists(id);
        }
    }
}
