function agregarNuevaTareaAlListado() {
    tareaListViewModel.tareas.push(new tareaElementoListadoViewModel({ id=0, titulo='' }));
    $("[name=titulo-tarea]").last().focus();
}

function manejarFocusoutTituloTarea(tarea) {
    const titulo = tarea.titulo();
    if (!titulo) {
        tareaListViewModel.tareas.pop();
        return;
    }
    tareas.id(1);
}

