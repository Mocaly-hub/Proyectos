
extends Area2D

@onready var musica = get_node_or_null("MountainSoundEffects")

var tween_musica: Tween


func _on_body_entered(body: Node2D) -> void:
	if body.name == "Jugador":
		print("ENTRÓ")
		body.zoom_objetivo = body.zoom_escalera

		if musica != null:
			iniciar_musica()


func _on_body_exited(body: Node2D) -> void:
	if body.name == "Jugador":
		print("SALIÓ")
		body.zoom_objetivo = body.zoom_normal

		if musica != null:
			detener_musica()


func iniciar_musica() -> void:
	if tween_musica:
		tween_musica.kill()

	if not musica.playing:
		musica.volume_db = -40.0
		musica.play()

	tween_musica = create_tween()
	tween_musica.tween_property(
		musica, "volume_db", -18.0, 3.0
	)


func detener_musica() -> void:
	if tween_musica:
		tween_musica.kill()

	if not musica.playing:
		return

	tween_musica = create_tween()
	tween_musica.tween_property(
		musica, "volume_db", -40.0, 3.0
	)
	tween_musica.tween_callback(musica.stop)
