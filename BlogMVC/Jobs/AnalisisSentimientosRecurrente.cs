using BlogMVC.Servicios;

namespace BlogMVC.Jobs
{
    public class AnalisisSentimientosRecurrente : BackgroundService
    { 
        //Singleton
        private readonly IServiceProvider serviceProvider;

        public AnalisisSentimientosRecurrente(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    System.Diagnostics.Debug.WriteLine("Iniciando análisis de sentimientos de comentarios");
                    var analisisSentimientos = scope.ServiceProvider.GetRequiredService<IAnalisisSentimientos>();
                    await analisisSentimientos.AnalizarComentariosPendientes();
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
