window.observarElemento = (idElemento, dotNetHelper) => {
    let observador = new IntersectionObserver((entradas) => {
        if (entradas[0].isIntersecting) {
            console.log("Se alcanzó el final, invocando CargarMasElementos...");
            dotNetHelper.invokeMethodAsync("CargarMasElementos");
        }
    });

    let elemento = document.getElementById(idElemento);
    if (elemento) {
        observador.observe(elemento);
    }
}