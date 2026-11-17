from django.contrib import admin
from .models import Registro

# El modelo también aparecerá en /admin/
admin.site.register(Registro)
