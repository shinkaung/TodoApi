using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;
using Newtonsoft.Json;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerTypeController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public CustomerTypeController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Customers
        [HttpGet]
         public async Task<ActionResult<IEnumerable<CustomerType>>> GetCustomerTypes()
        {
            var customertypeList = await _repositoryWrapper.CustomerType.FindAllAsync();
            return Ok(customertypeList);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
         public async Task<ActionResult<CustomerType>> GetCustomerType(long id)
        {
            var customertype = await _repositoryWrapper.CustomerType.FindByIDAsync(id);

            if (customertype == null)
            {
                return NotFound();
            }

            return customertype;
        }
        
        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
         public async Task<IActionResult> PutCustomerType(long id, CustomerType customertype)
        {
            if (id != customertype.Id)
            {
                return BadRequest();
            }

            TryValidateModel(customertype); //server side validation by using TryValidateModel
            if(!ModelState.IsValid){
                return BadRequest(ModelState.ToList());
            }

            CustomerType? objCusType;
            try
            {
                objCusType = _repositoryWrapper.CustomerType.FindByID(id);
                if (objCusType == null) 
                    throw new Exception("Invalid CustomerType ID");
                
                objCusType.CustomerTypeName = customertype.CustomerTypeName;
                await _repositoryWrapper.CustomerType.UpdateAsync(objCusType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerTypeExists(id))
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
         public async Task<ActionResult<CustomerType>> PostCustomerType(CustomerType customertype)
        {
             await _repositoryWrapper.CustomerType.CreateAsync(customertype, true);
            return CreatedAtAction(nameof(GetCustomerType), new { id = customertype.Id }, customertype);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerType(long id)
        {
            var customertype = await _repositoryWrapper.CustomerType.FindByIDAsync(id);
            if (customertype == null)
            {
                return NotFound();
            }

            await _repositoryWrapper.CustomerType.DeleteAsync(customertype, true);
            
            return NoContent();
        }

         [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<CustomerType>>>  SearchCustomerType(dynamic param)
        {
            dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
            string nameFilter = filterObj.term;
            var customertypeList = await _repositoryWrapper.CustomerType.SearchCustomerType(nameFilter);
            return Ok(customertypeList);
            
        }

        private bool CustomerTypeExists(long id)
        {
            return _repositoryWrapper.CustomerType.IsExists(id);
        }
    }
}
