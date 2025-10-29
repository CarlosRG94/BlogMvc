using BlogMVC.Configuraciones;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace BlogMVC.Servicios
{
    public class ServicioChatOpenAI : IServicioChat
    {
        private readonly IOptions<ConfiguracionesIA> options;
        private readonly OpenAIClient openAIClient;

        private string systemPromptGenerarCuerpo = """
            Eres un redactor profesional especializado en viajes y turismo.
            Escribes artículos informativos, prácticos y bien estructurados sobre destinos, monumentos, rutas y experiencias viajeras.
            Tu estilo es claro, objetivo y útil para el lector. Ofreces información precisa, recomendaciones, consejos logísticos (cómo llegar, qué visitar, cuándo ir, precios aproximados, etc.) y datos relevantes de cada destino.
            Evitas un tono demasiado emocional; priorizas la utilidad, claridad y fiabilidad.
            Si es relevante, puedes incluir enlaces a fuentes oficiales, páginas de turismo o sitios útiles.
            """;
        private string ObtenerPromptGeneraCuerpo(string titulo) => $"""
            Crear un artículo para un blog de viajes. El título del artículo será "{titulo}".

            El artículo debe ofrecer información práctica y útil sobre el destino o tema indicado. 
            Incluye datos como ubicación, cómo llegar, principales atracciones, mejores épocas para visitar, opciones de alojamiento o gastronomía local, si aplica.

            Puedes incluir enlaces a sitios web oficiales o fuentes relevantes (por ejemplo, páginas de turismo, transporte, museos, etc.).

            El formato de respuesta debe ser HTML (sin incluir etiquetas globales como DOCTYPE, html, head o body).
            Usa encabezados (<h2>, <h3>), listas (<ul>, <ol>), negritas y párrafos bien separados para mejorar la legibilidad.

            No incluyas el título del artículo dentro del texto, ya que será agregado por el sistema del blog.
            """;
        public ServicioChatOpenAI(IOptions<ConfiguracionesIA> options, OpenAIClient openAIClient)
        {
            this.options = options;
            this.openAIClient = openAIClient;
        }

        public async Task<string> GenerarCuerpo(string titulo)
        {
            var modeloTexto = options.Value.ModeloTexto;
            var clienteChat = openAIClient.GetChatClient(modeloTexto);

            var mensajeDeSistema = new SystemChatMessage(systemPromptGenerarCuerpo);

            var promptUsuario = ObtenerPromptGeneraCuerpo(titulo);

            var mensajeUsuario = new UserChatMessage(promptUsuario);

            ChatMessage[] mensajes = { mensajeDeSistema, mensajeUsuario };
            var respuesta = await clienteChat.CompleteChatAsync(mensajes);
            var cuerpo = respuesta.Value.Content[0].Text;
            return cuerpo;

        }

        public async IAsyncEnumerable<string> GenerarCuerpoStream(string titulo)
        {
            var modeloTexto = options.Value.ModeloTexto;
            var clienteChat = openAIClient.GetChatClient(modeloTexto);

            var mensajeDeSistema = new SystemChatMessage(systemPromptGenerarCuerpo);

            var promptUsuario = ObtenerPromptGeneraCuerpo(titulo);

            var mensajeUsuario = new UserChatMessage(promptUsuario);

            ChatMessage[] mensajes = { mensajeDeSistema, mensajeUsuario };

            await foreach (var completionUpdate in clienteChat.CompleteChatStreamingAsync(mensajes))
            {
                foreach (var contenido in completionUpdate.ContentUpdate)
                {
                    yield return contenido.Text;
                }
            }
        }
    }
}
