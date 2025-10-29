using BlogMVC.Configuraciones;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Images;

namespace BlogMVC.Servicios
{
    public class ServicioImagenesOpenAI : IServicioImagenes
    {
        private readonly IOptions<ConfiguracionesIA> options;
        private readonly OpenAIClient openAIClient;

        public ServicioImagenesOpenAI(IOptions<ConfiguracionesIA> options, OpenAIClient openAIClient)
        {
            this.options = options;
            this.openAIClient = openAIClient;

        }

        public async Task<byte[]> GenerarPortadaEntrada(string titulo)
        {
            string prompt = $"""
                Una imagen foto-realista inspirada en el tema "{titulo}".

                La escena debe representar de forma atractiva el destino, monumento, ciudad o experiencia de viaje mencionada en el título.

                Si el tema trata sobre una ciudad o destino, mostrar sus paisajes, arquitectura o puntos de interés más reconocibles.
                Si el tema trata sobre un monumento, mostrar el monumento en su entorno real, con buena iluminación y perspectiva.
                Si el tema está relacionado con una región o ruta, representar los paisajes o aspectos culturales típicos (gastronomía, transporte, tradiciones, etc.).
                Si el tema es un tipo de viaje (aventura, playa, cultural, gastronómico...), reflejar el ambiente y la sensación del tipo de experiencia.

                La iluminación debe ser natural y realista, con colores equilibrados y buena profundidad de campo.
                Evita incluir texto, logotipos o personas reconocibles (solo figuras genéricas si es necesario).
                """;

            var imageGenerationOptions = new ImageGenerationOptions
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1792xH1024,
                Style = GeneratedImageStyle.Natural,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            var modeloImagenes = options.Value.ModeloImagenes;
            var clienteImagen = openAIClient.GetImageClient(modeloImagenes);
            var imagenGenerada = await clienteImagen.GenerateImageAsync(prompt, imageGenerationOptions);
            var bytes = imagenGenerada.Value.ImageBytes.ToArray();
            return bytes;
        }
    }
}