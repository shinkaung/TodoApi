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
    public class SuppliersController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IRepositoryWrapper _repositoryWrapper;
        public SuppliersController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Employees
        // [HttpPost]

        //  public async Task<ActionResult<IEnumerable<Supplier>>> PostSuppliers()
        // {
        //     var supList = await _repositoryWrapper.Supplier.FindAllAsync();
        //     return Ok(supList);
        // }

        [HttpPost("showlist")]
        public async Task<JsonResult> PostSupplierGrid([DataSourceRequest]DataSourceRequest request)
        {   
            // Task.Delay(3000).Wait();
            var dsminQuery = new JsonResult (await _repositoryWrapper.Supplier.GetSupplierInfoGrid(request));
            return dsminQuery;
        }


        // GET: api/Employees/5
        [HttpGet("{id}")]
         public async Task<ActionResult<Supplier>> GetSupplier(long id)
        {
            var employee = await _repositoryWrapper.Supplier.FindByIDAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }
       

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
         public async Task<IActionResult> PutSupplier(long id, Supplier supplier)
        {
            if (id != supplier.Id)
            {
                return BadRequest();
            }

            TryValidateModel(supplier);
            if(!ModelState.IsValid){
                return BadRequest(ModelState.ToList());
            }
            Supplier? objSup;
            try
            {
                objSup = _repositoryWrapper.Supplier.FindByID(id);
                if (objSup == null) 
                    throw new Exception("Invalid Supplier ID");
                
                objSup.SupplierName = supplier.SupplierName;
                objSup.RegisterDate = supplier.RegisterDate;
                objSup.SupplierAddress = supplier.SupplierAddress;
                objSup.SupplierTypeId = supplier.SupplierTypeId;
                if(supplier.SupplierPhoto!= null && supplier.SupplierPhoto != "")
                {
                    //single
                    FileService.DeleteFileNameOnly("SupplierPhoto", id.ToString());
                    FileService.MoveTempFile("SupplierPhoto", supplier.Id.ToString(), supplier.SupplierPhoto);
                }
                await _repositoryWrapper.Supplier.UpdateAsync(objSup);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierExists(id))
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
         public async Task<ActionResult<Supplier>> PostSupplier(Supplier supplier)
        {
            try
            {
                Validator.ValidateObject(supplier, new ValidationContext(supplier), true);
                await _repositoryWrapper.Supplier.CreateAsync(supplier, true);
                if(supplier.SupplierPhoto!= null && supplier.SupplierPhoto != "")
                {
                    //single
                    FileService.MoveTempFile("SupplierPhoto", supplier.Id.ToString(), supplier.SupplierPhoto);
                }
                return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
            }
            catch(ValidationException vex)
            {
                return BadRequest(vex.Message);
            }
            
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(long id)
        {
            var employee = await _repositoryWrapper.Supplier.FindByIDAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
             //single
             FileService.DeleteFileNameOnly("SupplierPhoto", id.ToString());

            await _repositoryWrapper.Supplier.DeleteAsync(employee, true);
            
            return NoContent();
        }

         [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Supplier>>>  SearchSupplier(dynamic param)
        {
            dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
            string nameFilter = filterObj.term;
            var empList = await _repositoryWrapper.Supplier.SearchSupplier(nameFilter);
            return Ok(empList);
            
        }

        private bool SupplierExists(long id)
        {
            return _repositoryWrapper.Supplier.IsExists(id);
        }
    }
}
