using Datos;
using Dominio.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Servicios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly SqlDbContext _DbContext;

        public AutoresController(SqlDbContext context)
        {
            _DbContext = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Eautores>>> GetAutores()
        {
            return await _DbContext.TblAutores.ToListAsync();
        }

             
        [HttpPost]
        public async Task<ActionResult<Eautores>> PostEautores(Eautores eautores)
        {
            _DbContext.TblAutores.Add(eautores);
            await _DbContext.SaveChangesAsync();

            return CreatedAtAction("GetEautores", new { id = eautores.IdReg }, eautores);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEautores(int id)
        {
            var eautores = await _DbContext.TblAutores.FindAsync(id);
            if (eautores == null)
            {
                return NotFound();
            }

            _DbContext.TblAutores.Remove(eautores);
            await _DbContext.SaveChangesAsync();

            return NoContent();
        }

    }
}
