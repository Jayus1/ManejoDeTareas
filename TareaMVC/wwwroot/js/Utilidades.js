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

function confirmarAccion({callBackAceptar, callBackCancelar})
{
    Swal.fire({
        title: titulo || 'Realmente queires hacer esto?',
        icon: 'warning',
        showCancelButton: true,
        confirmButton: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Si',
        focusConfirm; true
        }).them((resultado) => {
            if (resultado.isConfirmed) {
                callBackAceptar();
            }
            else if (callBackCancelar){
                callBackCancelar();
            }
        })
}