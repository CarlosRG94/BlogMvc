using BlogMVC.Datos;
using BlogMVC.Entidades;
using BlogMVC.Models;
using BlogMVC.Servicios;
using BlogMVC.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Controllers
{
    public class ComentariosController:Controller
    {
        private readonly ApplicationDBContext context;
        private readonly IServicioUsuarios serviciosUsuarios;

        public ComentariosController(ApplicationDBContext context, IServicioUsuarios serviciosUsuarios)
        {
            this.context = context;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Comentar(EntradasComentarViewModel modelo)
        {
            if (!ModelState.IsValid) 
            { 
                return RedirectToAction("detalle", "entradas", new {id = modelo.Id});
            }

            var existeEntrada = await context.Entradas.AnyAsync(x => x.Id == modelo.Id);

            if (!existeEntrada) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var usuarioId = serviciosUsuarios.ObtenerUsarioId()!;

            var comentario = new Comentario
            {
                EntradaId = modelo.Id,
                Cuerpo = modelo.Cuerpo,
                UsuarioId = usuarioId,
                FechaPublicacion = DateTime.UtcNow
            };

            context.Add(comentario);
            await context.SaveChangesAsync();
            return RedirectToAction("detalle", "entradas", new {id = modelo.Id});
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Borrar(int id) 
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario == null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var usuarioId = serviciosUsuarios.ObtenerUsarioId();
            var puedeBorrarCualquierComentario = await serviciosUsuarios.PuedeUsuarioBorrarComentarios();

            if(usuarioId != comentario.UsuarioId && !puedeBorrarCualquierComentario) 
            {
                var urlRetorno = HttpContext.ObtenerUrlRetorno();
                return RedirectToAction("login", "usuarios", new { urlRetorno });
            }
            return View(comentario);
            
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> BorrarComentario(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentario == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var usuarioId = serviciosUsuarios.ObtenerUsarioId();
            var puedeBorrarCualquierComentario = await serviciosUsuarios.PuedeUsuarioBorrarComentarios();

            if (usuarioId != comentario.UsuarioId && !puedeBorrarCualquierComentario)
            {
                var urlRetorno = HttpContext.ObtenerUrlRetorno();
                return RedirectToAction("login", "usuarios", new { urlRetorno });
            }
            comentario.Borrado = true;
            await context.SaveChangesAsync();
            return RedirectToAction("Detalle", "Entradas", new {id=comentario.EntradaId});

        }

    }

}
