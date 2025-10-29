
namespace BlogMVC.Servicios
{
    public class AlmacenadorArchivosLocal : IAlmacenadorArchivos
    {
        private readonly string volumenPath;
        private readonly IHttpContextAccessor httpContextAccesor;

        public AlmacenadorArchivosLocal(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccesor = httpContextAccessor;
            // Ruta del volumen persistente
            this.volumenPath = Environment.GetEnvironmentVariable("VOLUMEN_IMAGENES")
                               ?? throw new Exception("Variable de entorno VOLUMEN_IMAGENES no definida");
        }

        public async Task<string> Almacenar(string contenedor, IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(volumenPath, contenedor);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string ruta = Path.Combine(folder, nombreArchivo);

            using (var ms = new MemoryStream())
            {
                await archivo.CopyToAsync(ms);
                var contenido = ms.ToArray();
                await File.WriteAllBytesAsync(ruta, contenido);
            }

            var request = httpContextAccesor.HttpContext!.Request;
            var url = $"{request.Scheme}://{request.Host}";
            var urlArchivo = Path.Combine(contenedor, nombreArchivo).Replace("\\", "/"); // URL relativa
            return $"{url}/{urlArchivo}";
        }

        public Task Borrar(string? ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return Task.CompletedTask;
            }

            var nombreArchivo = Path.GetFileName(ruta);
            var directorioArchivo = Path.Combine(volumenPath, contenedor, nombreArchivo);

            if (File.Exists(directorioArchivo))
            {
                File.Delete(directorioArchivo);
            }

            return Task.CompletedTask;
        }
    }
}
