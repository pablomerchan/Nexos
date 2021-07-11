using Datos;
using Dominio;
using Dominio.Modelos;
using Logica.Libros;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

             
        [HttpPost]
        public async Task<ActionResult<Elibros>> PostElibros(Elibros elibros)
        {
            bool CupoLibrosEstaCompleto = false;
            bool AutorYaEstaRegistrado = false;
            string MensajeError = "";
            bool datosIngresadosSonValidos = false;

            try
            {
                // Se debe garantizar la integridad de la información
                DtoVerificacionDatos ResultadoVerificacionDatos = VerificarDatosIngresados(elibros);
                if (ResultadoVerificacionDatos.EsValido)
                {
                    datosIngresadosSonValidos = true;
                }
                else
                {
                    MensajeError = ResultadoVerificacionDatos.MensajeError;
                }

                // Se debe controlar el número de libros permitidos
                if (datosIngresadosSonValidos)
                {
                    CupoLibrosEstaCompleto = _reglaNegocioLibros.EstaLaCantidadDelibrosCompleta(elibros.IdLibro);
                    if (CupoLibrosEstaCompleto==true)
                    {
                        MensajeError = MensajesDelProceso.CupoLibrosCompleto;
                        CupoLibrosEstaCompleto = true;
                    }
                }
               
                //Si al intentar registrar un libro y no existe autor registrado...
                if (CupoLibrosEstaCompleto == false)
                {
                    var respuesta = VerificarSiAutorEstaRegistrado(elibros);
                    AutorYaEstaRegistrado = respuesta.EsValido;

                    if (respuesta.EsValido == false)
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
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Ok = false, Mensaje = ex.Message });
            }

            return Ok();
        }

        private DtoVerificacionDatos VerificarSiAutorEstaRegistrado(Elibros elibros)
        {
            DtoVerificacionDatos respuestaMetodo = new();
            respuestaMetodo.EsValido = false;

            try
            {
                if (_DbContext.TblAutores.Where(e => e.IdAutor == elibros.IdAutor).Count() > 0)
                {
                    respuestaMetodo.EsValido = true;
                }
                else
                {
                    respuestaMetodo.MensajeError = MensajesDelProceso.AutorNoRegistrado;
                }
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
            return respuestaMetodo;
        }

        private DtoVerificacionDatos VerificarDatosIngresados(Elibros elibros)
        {
            DtoVerificacionDatos respuestaMetodo = new DtoVerificacionDatos();
            bool ValidacionPreviaIsValida = true;
            respuestaMetodo.EsValido = true;

            try
            {
                if (elibros.IdLibro == 0)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.IdLibroVacio;
                    ValidacionPreviaIsValida = false;

                }

                if (elibros.titulo.Length == 0 && ValidacionPreviaIsValida == true)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.TituloVacio;
                    ValidacionPreviaIsValida = false;
                }

                if (elibros.anio == 0 && ValidacionPreviaIsValida == true)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.añoNoValido;
                    ValidacionPreviaIsValida = false;
                }

                if (elibros.genero.Length == 0 && ValidacionPreviaIsValida == true)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.GeneroNoValido;
                    ValidacionPreviaIsValida = false;
                }

                if (elibros.numeroPaginas == 0 && ValidacionPreviaIsValida == true)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.NumeroPaginasNoValido;
                    ValidacionPreviaIsValida = false;
                }

                if (elibros.IdAutor == 0 && ValidacionPreviaIsValida == true)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.IdAutorNoValido;
                    ValidacionPreviaIsValida = false;
                }

                if (elibros.IdLibro == 0 && ValidacionPreviaIsValida == true)
                {
                    respuestaMetodo.EsValido = false;
                    respuestaMetodo.MensajeError = MensajesDelProceso.IdLibroVacio;
                    ValidacionPreviaIsValida = false;
                }
            }
            catch (System.Exception e)
            {
                throw new Exception(e.Message);
            }
            return respuestaMetodo;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteELibros(int id)
        {
            var elibros = await _DbContext.Tbllibros.FindAsync(id);
            if (elibros == null)
            {
                return NotFound();
            }

            _DbContext.Tbllibros.Remove(elibros);
            await _DbContext.SaveChangesAsync();

            return NoContent();
        }


        private class DtoVerificacionDatos
        {
            public bool EsValido { get; set; }
            public string MensajeError { get; set; }
        }
    }
      
}
