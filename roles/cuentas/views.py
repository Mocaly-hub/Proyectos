from django.shortcuts import render, redirect, get_object_or_404
from django.contrib.auth.decorators import login_required
from django.contrib.auth.models import User
from .models import Registro


# Es la misma idea de obtener_rol() del proyecto de clases.
def obtener_rol(usuario):
    if usuario.is_superuser:
        return 'Administrador'
    if usuario.groups.filter(name='Administrador').exists():
        return 'Administrador'
    if usuario.groups.filter(name='Usuario').exists():
        return 'Usuario'
    return 'Sin rol'


# Se usa cuando alguien intenta entrar a una pagina sin permiso.
def acceso_denegado(request, rol):
    return render(request, 'cuentas/403.html', {'rol': rol}, status=403)


@login_required
def inicio(request):
    rol = obtener_rol(request.user)
    return render(request, 'cuentas/inicio.html', {'rol': rol})


# Todos los usuarios con rol pueden ver la lista.
@login_required
def lista(request):
    rol = obtener_rol(request.user)
    if rol not in ['Administrador', 'Usuario']:
        return acceso_denegado(request, rol)

    registros = Registro.objects.all()
    return render(request, 'cuentas/lista.html', {
        'rol': rol,
        'registros': registros
    })


# Solo el administrador puede agregar registros.
@login_required
def crear(request):
    rol = obtener_rol(request.user)
    if rol != 'Administrador':
        return acceso_denegado(request, rol)

    error = ''
    if request.method == 'POST':
        titulo = request.POST.get('titulo', '').strip()
        descripcion = request.POST.get('descripcion', '').strip()
        id_propietario = request.POST.get('propietario', '')
        propietario = User.objects.filter(id=id_propietario).first() if id_propietario.isdigit() else None

        if titulo and len(titulo) <= 100 and propietario:
            Registro.objects.create(
                titulo=titulo,
                descripcion=descripcion,
                propietario=propietario
            )
            return redirect('lista')
        error = 'Completa el titulo y selecciona un propietario.'

    usuarios = User.objects.all()
    return render(request, 'cuentas/crear.html', {
        'rol': rol,
        'usuarios': usuarios,
        'error': error
    })


# El administrador edita todo; el usuario solo sus registros.
@login_required
def editar(request, id):
    rol = obtener_rol(request.user)
    registro = get_object_or_404(Registro, id=id)

    if rol != 'Administrador' and not (
        rol == 'Usuario' and registro.propietario_id == request.user.id
    ):
        return acceso_denegado(request, rol)

    error = ''
    if request.method == 'POST':
        titulo = request.POST.get('titulo', '').strip()
        descripcion = request.POST.get('descripcion', '').strip()

        if titulo and len(titulo) <= 100:
            registro.titulo = titulo
            registro.descripcion = descripcion
            registro.save()
            return redirect('lista')
        error = 'El titulo es obligatorio (maximo 100 caracteres).'

    return render(request, 'cuentas/editar.html', {
        'rol': rol,
        'registro': registro,
        'error': error
    })


# Solo el administrador elimina, y solo con un formulario POST.
@login_required
def eliminar(request, id):
    rol = obtener_rol(request.user)
    if rol != 'Administrador':
        return acceso_denegado(request, rol)

    registro = get_object_or_404(Registro, id=id)
    if request.method == 'POST':
        registro.delete()
        return redirect('lista')

    return render(request, 'cuentas/eliminar.html', {
        'rol': rol,
        'registro': registro
    })
