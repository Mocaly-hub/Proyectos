from django.db import models
from django.contrib.auth.models import User

# Cada registro pertenece a un usuario.
class Registro(models.Model):
    titulo = models.CharField(max_length=100)
    descripcion = models.TextField()
    propietario = models.ForeignKey(User, on_delete=models.CASCADE)

    def __str__(self):
        return self.titulo
