using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;
using Newtonsoft.Json;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminLevelController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public AdminLevelController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Customers
        [HttpGet]
         public async Task<ActionResult<IEnumerable<AdminLevel>>> GetAdminLevels()
        {
            var admlvlList = await _repositoryWrapper.AdminLevel.FindAllAsync();
            return Ok(admlvlList);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
         public async Task<ActionResult<AdminLevel>> GetAdminLevel(long id)
        {
            var admlvl = await _repositoryWrapper.AdminLevel.FindByIDAsync(id);

            if (admlvl == null)
            {
                return NotFound();
            }

            return admlvl;
        }
        
        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
         public async Task<IActionResult> PutAdminLevel(long id, AdminLevel adminLevel)
        {
            if (id != adminLevel.Id)
            {
                return BadRequest();
            }

            TryValidateModel(adminLevel); //server side validation by using TryValidateModel
            if(!ModelState.IsValid){
                return BadRequest(ModelState.ToList());
            }

            AdminLevel? objAdmLvl;
            try
            {
                objAdmLvl = _repositoryWrapper.AdminLevel.FindByID(id);
                if (objAdmLvl == null) 
                    throw new Exception("Invalid AdminLevel ID");
                
                objAdmLvl.AdminLevelName = adminLevel.AdminLevelName;
                await _repositoryWrapper.AdminLevel.UpdateAsync(objAdmLvl);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminLevelExists(id))
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
         public async Task<ActionResult<AdminLevel>> PostAdminLevel(AdminLevel adminLevel)
        {
             await _repositoryWrapper.AdminLevel.CreateAsync(adminLevel, true);
             return CreatedAtAction(nameof(GetAdminLevel), new { id = adminLevel.Id }, adminLevel);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdminLevel(long id)
        {
            var adminLevel = await _repositoryWrapper.AdminLevel.FindByIDAsync(id);
            if (adminLevel == null)
            {
                return NotFound();
            }

            await _repositoryWrapper.CustomerType.DeleteAsync(adminLevel, true);
            
            return NoContent();
        }

        //  [HttpPost("search")]
        // public async Task<ActionResult<IEnumerable<AdminLevel>>>  SearchAdminLevel(dynamic param)
        // {
        //     dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
        //     string nameFilter = filterObj.term;
        //     var customertypeList = await _repositoryWrapper.AdminLevel.SearchAdminLevel(nameFilter);
        //     return Ok(customertypeList);
            
        // }

        private bool AdminLevelExists(long id)
        {
            return _repositoryWrapper.CustomerType.IsExists(id);
        }
    }
}
