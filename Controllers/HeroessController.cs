using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Repositories;
using Newtonsoft.Json;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroessController : ControllerBase
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public HeroessController(IRepositoryWrapper RW)
        {
            _repositoryWrapper = RW;
        }
        // GET: api/Heroess
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Heroes>>> GetHeroess()
        {
            var heroList = await _repositoryWrapper.Hero.FindAllAsync();
            return Ok(heroList);
        }

        // GET: api/Heroess/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Heroes>> GetHeroess(long id)
        {
       
            var hero = await _repositoryWrapper.Hero.FindByIDAsync(id);

                if (hero == null)
                {
                    return NotFound();
                }

                return hero;
        }

        // PUT: api/Heroess/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHeroes(long id, Heroes heroes)
        {
            // return NoContent();
             if (id != heroes.Id)
            {
                return BadRequest();
            }

            Heroes? objHero;
            try
            {
                objHero = _repositoryWrapper.Hero.FindByID(id);
                if (objHero == null) 
                    throw new Exception("Invalid Hero ID");
                
                objHero.Name = heroes.Name;
                await _repositoryWrapper.Hero.UpdateAsync(objHero);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HeroesExists(id))
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

        // POST: api/Heroess
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Heroes>> PostHeroes(Heroes heroes)
        {
            await _repositoryWrapper.Hero.CreateAsync(heroes, true);
            return CreatedAtAction(nameof(GetHeroess), new { id = heroes.Id }, heroes);
      
        }

        // DELETE: api/Heroess/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHeroes(long id)
        {
            var hero = await _repositoryWrapper.Hero.FindByIDAsync(id);
            if (hero == null)
            {
                return NotFound();
            }

            await _repositoryWrapper.Hero.DeleteAsync(hero, true);
            
            return NoContent();
        }
         [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<Heroes>>>  SearchHero(dynamic param)
        {
            dynamic filterObj = JsonConvert.DeserializeObject<dynamic>(param.ToString());
            string nameFilter = filterObj.term;
            var heroList = await _repositoryWrapper.Hero.SearchHero(nameFilter);
            return Ok(heroList);
            
        }

        private bool HeroesExists(long id)
        {
             return _repositoryWrapper.Hero.IsExists(id);
       
         }
    }
}
