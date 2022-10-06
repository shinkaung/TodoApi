using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;
using Newtonsoft.Json;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierTypesController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public SupplierTypesController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Customers
        [HttpGet]
         public async Task<ActionResult<IEnumerable<SupplierType>>> GetSupplierTypes()
        {
            var suppliertypeList = await _repositoryWrapper.SupplierType.FindAllAsync();
            return Ok(suppliertypeList);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
         public async Task<ActionResult<SupplierType>> GetSupplierType(long id)
        {
            var suppliertype = await _repositoryWrapper.SupplierType.FindByIDAsync(id);

            if (suppliertype == null)
            {
                return NotFound();
            }

            return suppliertype;
        }
        
        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
         public async Task<IActionResult> PutSupplierType(long id, SupplierType suppliertype)
        {
            if (id != suppliertype.Id)
            {
                return BadRequest();
            }

            TryValidateModel(suppliertype); //server side validation by using TryValidateModel
            if(!ModelState.IsValid){
                return BadRequest(ModelState.ToList());
            }

            SupplierType? objCusType;
            try
            {
                objCusType = _repositoryWrapper.SupplierType.FindByID(id);
                if (objCusType == null) 
                    throw new Exception("Invalid CustomerType ID");
                
                objCusType.SupplierTypeName = suppliertype.SupplierTypeName;
                await _repositoryWrapper.SupplierType.UpdateAsync(objCusType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierTypeExists(id))
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
         public async Task<ActionResult<CustomerType>> PostSupplierType(SupplierType suppliertype)
        {
             await _repositoryWrapper.SupplierType.CreateAsync(suppliertype, true);
            return CreatedAtAction(nameof(GetSupplierType), new { id = suppliertype.Id }, suppliertype);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplierType(long id)
        {
            var customertype = await _repositoryWrapper.SupplierType.FindByIDAsync(id);
            if (customertype == null)
            {
                return NotFound();
            }
            await _repositoryWrapper.SupplierType.DeleteAsync(customertype, true);
            
            return NoContent();
        }

         [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<SupplierType>>>  SearchSupplierType(dynamic param)
        {
            dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
            string nameFilter = filterObj.term;
            var customertypeList = await _repositoryWrapper.SupplierType.SearchSupplierType(nameFilter);
            return Ok(customertypeList);
            
        }

        private bool SupplierTypeExists(long id)
        {
            return _repositoryWrapper.SupplierType.IsExists(id);
        }
    }
}
