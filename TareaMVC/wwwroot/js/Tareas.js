function agregarNuevaTareaAlListado() {
    tareaListadoViewModel.tareas.push(new tareaElementoListadoViewModel({ id=0, titulo='' }));
    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusoutTituloTarea(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListadoViewModel.tareas.pop();
        return;
    }
    const data = JSON.stringify(titulo);
    const respesta = await fetch(urlTarea, {
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (respuesta.ok) {
        const json = await respesta.json();
        tarea.id(json.id);
    }
    else {
        manejarErrorApi(respuesta);
    }
}

async function obtenerTareas() {
    tareaListadoViewModel.cargando(true);

    const respuesta = await fetch(urlTarea, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    tareaListadoViewModel.tareas([]);

    json.forEach(valor => {
        tareaElementoListado.tareas.push(new tareaElementoListadoViewModel(valor));
    });

    tareaListadoViewModel.cargando(false);
}

async function actualizarOrdenTareas() {
    const ids = obtenerIdsTareas();
    await enviarIdsTareasAlBackend(ids);

    const arregloOrdenado = tareaListadoViewModel.tareas.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListadoViewModel.tareas([]);
    tareaListadoViewModel.tareas(arregloOrdenado);

}

function obtenerIdsTareas() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get();

    return ids;
}

async function enviarIdsTareasAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTarea}/ordenar`.{
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    })
}

async function manejarClickTareas(tarea) {
    if (tarea.esNuevo()) {
        return;
    }

    const respuesta = await fetch(`${urlTarea}/${tarea.id()}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();

    tareaEditarVM.id = json.id;
    tareaEditarVM.titulo(json.titulo);
    tareaEditarVM.descripcion(json.descripcion);

    modalEditarTareaBootstrap.show();
}

$(function () {
    $("reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    })
})

async function manejarCambioEditarTarea()
{
    const obj = {
        id: tareaEditarVM.id,
        titulo: tareaEditarVM.titulo(),
        descripcion: tareaEditarVM.descripcion()
    };

    if (!obj.titulo) {
        return;
    }

    await editarTareaCompleta(obj);

    const indice = tareaListadoViewModel.tareas().findIndex(t => t.id() === obj.id);
    const tarea = tareaListadoViewModel.tareas()[indice];
    tarea.titulo(obj.titulo)
}

async function editarTareaCompleta(tarea) {
    const data = JSON.stringify(tarea);

    const respuesta = await fetch(`${urlTarea}/${tarea.id}`, {
        method: 'PUT',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (!respuesta.ok) {
        manejarErrorApi(respuesta);
        throw "error";
    }
}

function intentarBorrarTarea(tarea) {
    modelEditarTareaBootstrap.hide();

    confirmarAccion({
        callBackAceptar: () => {
            borrarTarea(tarea)
        }, callBackCancelar: () => {
            modalEditarTareaBootstrap.show();
        },
        titulo: `Desea borrar la tarea ${tarea.titulo()}?`
        })
}

async function borrarTarea(tarea) {
    const idTarea = tarea.id;

    const respuesta = await fetch(`${urlTarea}/${idTarea}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json'
        }
    });

    if (respuesta.ok) {
        const indice = obtenerIndiceTareaEdicion();
        tareaListadoViewModel.tareas.splice(indice, 1);
    }
}

function obtenerIndiceTareaEdicion() {
    return tareaListadoViewModel.tareas().findIndex(t => t.id() == tareaEditarVM.id);
}