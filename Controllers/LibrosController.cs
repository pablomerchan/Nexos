using Datos;
using Dominio;
using Dominio.Modelos;
using Logica.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Servicios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly SqlDbContext _DbContext;
        private readonly IReglaNegocioLibros _reglaNegocioLibros;

        public LibrosController(SqlDbContext context, IReglaNegocioLibros reglaNegocioLibros)
        {
            _DbContext = context;
            _reglaNegocioLibros = reglaNegocioLibros;
        }

        // GET: api/Libros
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Elibros>>> GetTbllibros()
        {
            return await _DbContext.Tbllibros.ToListAsync();
        }

        // GET: api/Libros/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Elibros>> GetElibros(int id)
        {
            

            var elibros = await _DbContext.Tbllibros.FindAsync(id);

            if (elibros == null)
            {
                return NotFound();
            }

            return elibros;
        }

        // PUT: api/Libros/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutElibros(int id, Elibros elibros)
        {
            if (id != elibros.IdReg)
            {
                return BadRequest();
            }

            _DbContext.Entry(elibros).State = EntityState.Modified;

            try
            {
                await _DbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ElibrosExists(id))
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


        [HttpPost]
        public async Task<ActionResult<Elibros>> PostElibros(Elibros elibros)
        {
            bool CupoLibrosEstaCompleto = false;
            bool AutorYaEstaRegistrado = false;
            string MensajeError = string.Empty;
            bool datosIngresadosSonValidos = false;

            // Se debe garantizar la integridad de la información
            DtoVerificacionDatos ResultadoVerificacionDatos = VerificarDatosIngresados(elibros);
            if (ResultadoVerificacionDatos.EsValido)
            {
                datosIngresadosSonValidos = true;
            } else
            {
                MensajeError= ResultadoVerificacionDatos.MensajeError;
            }

            // Se debe controlar el número de libros permitidos
            if (datosIngresadosSonValidos)
            {
                CupoLibrosEstaCompleto = _reglaNegocioLibros.EstaLaCantidadDelibrosCompleta(elibros.IdLibro);
            } else
            {
                //Si al intentar registrar un libro se supera el máximo permitido, debe generarse una excepción
                // y responder con el mensaje: “No es posible registrar el libro, se alcanzó el máximo permitido.”.
                MensajeError = MensajesDelProceso.CupoLibrosCompleto;
            }

            //Si al intentar registrar un libro y no existe autor registrado...
            if (CupoLibrosEstaCompleto==false)
            {
                var respuesta = VerificarSiAutorEstaRegistrado(elibros);
                AutorYaEstaRegistrado = respuesta.EsValido;

                if (respuesta.EsValido==false)
                {
                    //Responder con el mensaje: “El autor no está registrado”.
                    MensajeError = MensajesDelProceso.AutorNoRegistrado;
                }
            } 

            if (AutorYaEstaRegistrado)
            {
                ///<remarks>
                /// Las validaciones fueron exitosas y se procede a la insercion del nuevo libro
                /// </remarks>       
                _DbContext.Tbllibros.Add(elibros);
                await _DbContext.SaveChangesAsync();
                return CreatedAtAction("GetElibros", new { id = elibros.IdReg }, elibros);
            }

            if (MensajeError.Length > 0)
            {
                return BadRequest(new { Ok = false, Mensaje = MensajeError });
            }

            return Ok();
        }

        private DtoVerificacionDatos VerificarSiAutorEstaRegistrado(Elibros elibros)
        {
            DtoVerificacionDatos respuestaMetodo = new();
            respuestaMetodo.EsValido = false;

            if (_DbContext.TblAutores.Where(e => e.IdAutor == elibros.IdAutor).Count()>0)
            {
                respuestaMetodo.EsValido = true;
            } else
            {
                respuestaMetodo.MensajeError = MensajesDelProceso.AutorNoRegistrado;
            }

            return respuestaMetodo;
        }

        private DtoVerificacionDatos VerificarDatosIngresados(Elibros elibros)
        {
            DtoVerificacionDatos respuestaMetodo = new DtoVerificacionDatos();

            if (elibros.IdLibro == 0)
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.IdLibroVacio;
            }

            if (elibros.titulo.Length == 0)
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.TituloVacio;
            }

            if (elibros.anio == 0)
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.añoNoValido;
            }

            if (elibros.genero.Length == 0) 
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.GeneroNoValido;
            }

            if (elibros.numeroPaginas == 0) 
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.NumeroPaginasNoValido;
            }

            if (elibros.IdAutor == 0) 
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.IdAutorNoValido;
            }

            if (elibros.IdLibro == 0) 
            {
                respuestaMetodo.EsValido = false;
                respuestaMetodo.MensajeError = MensajesDelProceso.IdLibroVacio;
            }

            return respuestaMetodo;
        }
      
        private bool ElibrosExists(int id)
        {
            return _DbContext.Tbllibros.Any(e => e.IdReg == id);
        }

        private class DtoVerificacionDatos
        {
            public bool EsValido { get; set; }
            public string MensajeError { get; set; }
        }
    }
      
}
