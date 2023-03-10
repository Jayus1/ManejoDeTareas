async function mensajesDeErrorApi(respuesta) {
    let mensajeDeError = '';

    if (respuesta.status == 400) {
        mensajeDeError = await respuesta.text();
    } else if (respuesta.status == 401) {
        mensajeDeError = recursoNoEncontrado;
    }
    else {
        mensajeDeError = errorInesperado;
    }

    mostrarMensajeDeError(mensajeDeError);

}

function mostrarMensajeDeError(mensaje) {
    Swal.fire({
        icon: 'error',
        title: 'errror.....',
        text: mensaje
    });
}