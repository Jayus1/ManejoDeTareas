function agregarNuevaTareaAlListado() {
    tareaListViewModel.tareas.push(new tareaElementoListadoViewModel({ id=0, titulo='' }));
    $("[name=titulo-tarea]").last().focus();
}

async function manejarFocusoutTituloTarea(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListViewModel.tareas.pop();
        return;
    }
    const data = JSON.stringify(titulo);
    const respesta = await fetch(urlTarea, {
        method: 'POST',
        body: data,
        headers: {
            'Comtent-Type': 'application/json'
        }
    });

    if (respesta.ok) {
        const json = await respesta.json();
        tarea.id(json.id);
    }
    else {
        manejarErrorApi(respuesta);
    }
}

async function obtenerTareas() {
    tareaListadoViewModelfn.cargando(true);

    const respuesta = await fetch(urlTarea, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })

    if (!respuesta) {
        manejarErrorApi(respuesta);
        return;
    }

    const json = await respuesta.json();
    tareaListViewModel.tareas([]);

    json.forEach(valor => {
        tareaElementoListado.tarea.push(new tareaElementoListadoViewModel(valor));
    });

    tareaListadoViewModelfn.cargando(false);
}

async function actualizarOrdenTareas() {
    const ids = obtenerIdsTareas();
    await enviarIdsTareasAlBackend(ids);

    const arregloOrdenado = tareaListViewModel.tarea.sorted(function (a, b) {
        return ids.indexOf(a.id().toString()) - ids.indexOf(b.id().toString());
    });

    tareaListViewModel.tareas([]);
    tareaListViewModel.tareas(arregloOrdenado);

}

function obtenerIdsTareas() {
    const ids = $("[name=titulo-tarea]").map(function () {
        return $(this).attr("data-id");
    }).get();

    return ids;
}

async function enviarIdsTareasAlBackend(ids) {
    var data = JSON.stringify(ids);
    await fetch(`${urlTareas}/ordenas`.{
        method: 'POST',
        body: data,
        headers: {
            'Content-Type': 'application/json'
        }
    })
}

async function manejarClickTareas() {
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

    modalEditarTareaBootstrap.Show();


}

$(function () {
    $("reordenable").sortable({
        axis: 'y',
        stop: async function () {
            await actualizarOrdenTareas();
        }
    })
})