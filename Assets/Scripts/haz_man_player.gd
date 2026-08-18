extends CharacterBody2D

const tile_size: Vector2 = Vector2(16, 16)
var sprite_node_pos_tween: Tween

#why are comments like python :(
#Player Stats
var HP = 3
var State = IDLE

enum {IDLE, TALKING, PUNCHING, I_FRAMES, MOBILE, DEAD}

func _ready():
	pass
	
func _physics_process(_delta) -> void:
	
	#State Machine Goes Here
	match State :
		IDLE:
			$AnimationPlayer.queue("Idle Down")
			pass
		TALKING:
			$AnimationPlayer.queue("Idle Down")
			pass
		PUNCHING:
			pass
		I_FRAMES:
			# add damage animation here
			pass
		MOBILE:
			$AnimationPlayer.play("Walk Down")
			pass 
		DEAD:
			pass
	
	
	if (!sprite_node_pos_tween or !sprite_node_pos_tween.is_running()) and (State == IDLE or State == MOBILE or State == PUNCHING):
		if Input.is_action_pressed("space") :
			if State != PUNCHING :
				State = PUNCHING
				$AnimationPlayer.queue("Punch Down Init")
			else:
				$AnimationPlayer.queue("Punch Down Windup")
		else :
			if Input.is_action_just_released("space") and State == PUNCHING:
					#pass for now
				$AnimationPlayer.play("Punch Down Finish")
				# call punch script to summon punch with a given direction
			if Input.is_action_pressed("up") and !$up.is_colliding():
				State = MOBILE
				_move(Vector2(0, -1))
			elif Input.is_action_pressed("down") and !$down.is_colliding():
				State = MOBILE
				_move(Vector2(0, 1))
			elif Input.is_action_pressed("left") and !$left.is_colliding():
				State = MOBILE
				_move(Vector2(-1, 0))
			elif Input.is_action_pressed("right") and !$right.is_colliding():
				State = MOBILE
				_move(Vector2(1, 0))	
			
func _move(dir: Vector2):
	global_position += dir * tile_size
	$AnimationPosition.global_position -= dir * tile_size
	
	if sprite_node_pos_tween:
		sprite_node_pos_tween.kill()
	sprite_node_pos_tween = create_tween()
	sprite_node_pos_tween.set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	sprite_node_pos_tween.tween_property($AnimationPosition, "global_position", global_position, 0.18).set_trans(Tween.TRANS_SINE)
