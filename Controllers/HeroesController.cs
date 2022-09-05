using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Newtonsoft.Json;
using TodoApi.Repositories;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroesController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public HeroesController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }

        // GET: api/Heroes
        [HttpGet]
         public async Task<ActionResult<IEnumerable<Hero>>> GetHero()
        {
            var heroList = await _repositoryWrapper.Hero.FindAllAsync();
            return Ok(heroList);
        }

        // GET: api/Heroes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hero>> GetHero(long id)
        {
            var hero = await _repositoryWrapper.Hero.FindByIDAsync(id);

            if (hero == null)
            {
                return NotFound();
            }

            return hero;
        }

        // PUT: api/Heroes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHero(long id, Hero hero)
        {
            if (id != hero.id)
            {
                return BadRequest();
            }

            Hero? objHero;
            try
            {
                objHero = _repositoryWrapper.Hero.FindByID(id);
                if (objHero == null) 
                    throw new Exception("Invalid Employee ID");
                
                objHero.Name = hero.Name;
                await _repositoryWrapper.Hero.UpdateAsync(objHero);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HeroExists(id))
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

        // POST: api/Heroes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            await _repositoryWrapper.Employee.CreateAsync(employee, true);
            return CreatedAtAction(nameof(GetHero), new { id = employee.id }, employee);
        }

        // DELETE: api/Heroes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHero(long id)
        {
            var hero = await _repositoryWrapper.Hero.FindByIDAsync(id);
            if (hero == null)
            {
                return NotFound();
            }

            await _repositoryWrapper.Employee.DeleteAsync(hero, true);
            
            return NoContent();
        }
           
        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Hero>>> SearchHero(dynamic param)
        {
            dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
            string nameFilter = filterObj.term;
            var heroList = await _repositoryWrapper.Hero.SearchHero(nameFilter);
            return Ok(heroList);
        }

        private bool HeroExists(long id)
        {
            return _repositoryWrapper.Hero.IsExists(id);
        }
    }
}
