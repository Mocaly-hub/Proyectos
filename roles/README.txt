PREPRUEBA DJANGO - VERSION SENCILLA BASADA EN EL PROYECTO roles.zip
=====================================================================

NO HAY QUE INSTALAR NADA. Esta version usa Django ya configurado en el equipo de la clase.

PARA ABRIR:
1. Descomprime este ZIP.
2. Abre la carpeta roles en Visual Studio Code.
3. Ejecuta INICIAR.bat, o desde la terminal dentro de roles:
   py manage.py migrate
   py manage.py runserver
4. Abre http://127.0.0.1:8000/login/

El comando migrate NO instala nada; crea la tabla Registro en la base de datos SQLite incluida.
Despues de la primera vez, basta con py manage.py runserver.
Si en clases usas python en lugar de py, reemplaza py por python.

CUENTAS PARA PROBAR EN LOCAL:
admin / Clase2026! (superusuario, puede entrar a /admin/)
administrador / Clase2026! (grupo Administrador)
usuario / Clase2026! (grupo Usuario)

FUNCIONES:
- Login y logout originales
- Grupos Administrador y Usuario creados en /admin/
- CRUD de registros
- Administrador: crear, listar, editar y eliminar
- Usuario: listar y editar solo sus propios registros

ESTRUCTURA CONSERVADA DE TU EJEMPLO:
config/, cuentas/, views.py con obtener_rol(), @login_required, templates/cuentas/

Solo para pruebas locales; no publicar con estas credenciales de demostracion.
