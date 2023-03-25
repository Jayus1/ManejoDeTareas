let inputArchivoTarea = document.getElementById('archivoATarea')

function manejarClickAgregarArchivoAdjunto() {
    inputArchivoTarea.click();

}

async function manejarSeleccionArchivoTarea(event) {
    const archivos = event.target.files;
    const archivosArreglos = Array.from(archivo);

    const idTarea = tareaEditarVM.id;
    const formData = new FormData();

    for (var i = 0; i < archivosArreglos.length; i++) {
        formData.append("archivos", archivosArreglos[i]);
    }

    const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
        body: formData,
        method: 'POST'
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    prepararArchivosAdjuntos(json);

    inputArchivoTarea.value = null;
}

function prepararArchivosAdjuntos(archivosAdjuntos) {
    archivosAdjuntos.forEach(archivosAdjuntos => {
        let fechaCreacion = archivosAdjuntos.fechaCreacion;
        if (archivosAdjuntos.fechaCreacion.indexOf('Z') === -1) {
        fechaCreacion += 'Z';
    }

    const fechaCreacionDT = new Date(fechaCreacion);
    archivosAdjuntos.publicado = fechaCreacionDT.toLocaleString();

    tareaEditarVM.archivosAdjuntos.push(new archivosAdjuntosViewModel({ ...archivosAdjuntos, modoEdicion: false }));
});

}


let tituloArchivoAdjuntoAnterior;
function manejarClickTituloArchivoAdjunto(archivoAdjunto)
{
    archivoAdjunto.modoEdicion(true);
    tituloArchivoAdjuntoAnterior = archivoAdjunto.titulo();
    $("[name='txtArchivoAdjuntoTitulo']:visible").focus();
}

async function manejarFocusoutTituloArchivoAdjunto(archivoAdjunto) {
    archivoAdjunto.modoEdicion(false);
    const idTarea = archivoAdjunto.id;

    if (!archivoAdjunto.titulo()) {
        archivoAdjunto.titulo(tituloArchivoAdjuntoAnterior);
    }

    if (archivoAdjunto.titulo() === tituloArchivoAdjuntoAnterior) {
        return;
    }

    const data = JSON.stringify(archivoAdjunto, titulo());

    const respuesta = await fetch(`${urlArchivos}/${idTarea}`, {
        body: data,
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!espuesta.ok) {
        manejarErrorApi(respuesta);
    }


}

function manejarClickBorrarArchivosAdjuntos(archivoAdjunto) {
    modalEditarTareaBootstrap.hide();

    confirmarAccion({
        callbackAceptar: ()=> 
        })
}