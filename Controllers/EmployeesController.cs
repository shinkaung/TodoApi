using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;
using Newtonsoft.Json;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public EmployeesController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Employees
        [HttpGet]
         public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var empList = await _repositoryWrapper.Employee.FindAllAsync();
            return Ok(empList);
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
         public async Task<ActionResult<Employee>> GetEmployee(long id)
        {
            var employee = await _repositoryWrapper.Employee.FindByIDAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }
        
        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
         public async Task<IActionResult> PutEmployee(long id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            Employee? objEmp;
            try
            {
                objEmp = _repositoryWrapper.Employee.FindByID(id);
                if (objEmp == null) 
                    throw new Exception("Invalid Employee ID");
                
                objEmp.empName = employee.empName;
                objEmp.empAddress = employee.empAddress;
                await _repositoryWrapper.Employee.UpdateAsync(objEmp);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
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
         public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            await _repositoryWrapper.Employee.CreateAsync(employee, true);
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(long id)
        {
            var employee = await _repositoryWrapper.Employee.FindByIDAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            await _repositoryWrapper.Employee.DeleteAsync(employee, true);
            
            return NoContent();
        }

         [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Employee>>>  SearchEmployee(dynamic param)
        {
            dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
            string nameFilter = filterObj.term;
            var empList = await _repositoryWrapper.Employee.SearchEmployee(nameFilter);
            return Ok(empList);
            
        }

        private bool EmployeeExists(long id)
        {
            return _repositoryWrapper.Employee.IsExists(id);
        }
    }
}
