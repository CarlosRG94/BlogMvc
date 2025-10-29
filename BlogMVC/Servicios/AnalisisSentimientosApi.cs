using BlogMVC.Configuraciones;
using BlogMVC.Datos;
using BlogMVC.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogMVC.Servicios
{
    public class AnalisisSentimientosAPI : IAnalisisSentimientos
    {
        private readonly ApplicationDBContext context;
        private readonly HttpClient httpClient;

        public AnalisisSentimientosAPI(ApplicationDBContext context, HttpClient httpClient)
        {
            this.context = context;
            this.httpClient = httpClient;
        }

        public async Task AnalizarComentariosPendientes()
        {
            var comentariosPendientes = await context.Comentarios
                .Where(x => x.Puntuacion == null)
                .Take(500)
                .ToListAsync();

            if (!comentariosPendientes.Any())
                return;

            foreach (var comentario in comentariosPendientes)
            {
                var data = new { texto = comentario.Cuerpo };
                var json = JsonSerializer.Serialize(data);
                var contenido = new StringContent(json, Encoding.UTF8, "application/json");

                // ⚠️ cambia esto al dominio de Railway cuando despliegues
                var respuesta = await httpClient.PostAsync("https://analisissentimientosapi-production.up.railway.app/predict", contenido);

                if (respuesta.IsSuccessStatusCode)
                {
                    var cuerpo = await respuesta.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(cuerpo);

                    var sentimiento = doc.RootElement.GetProperty("sentimiento").GetString();

                    comentario.Puntuacion = sentimiento == "Negativo" ? 1 : 5;
                }
            }

            await context.SaveChangesAsync();
        }

        // Ya no necesitas ProcesarLotesPendientes
        public Task ProcesarLotesPendientes() => Task.CompletedTask;
    }
}
