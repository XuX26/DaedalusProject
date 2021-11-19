using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


/// <summary>
/// Player component. Manages inputs, character states and associated game flow.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour {

    public static Player Instance = null;

    [System.Serializable]
    public class MovementParameters
    {
        public float speedMax = 2.0f;
        public float acceleration = 12.0f;
        public float friction = 12.0f;
    }


    // Possible orientation for player aiming : 4 direction, 8 direction or free direction (for analogic joysticks)
    public enum ORIENTATION
    {
        FREE,
        DPAD_8,
        DPAD_4
    }

    // Character can only be at one state at a time. For example, he can't attack and be stunned at the same time.
    public enum STATE
    {
        IDLE = 0,
        ATTACKING = 1,
        STUNNED = 2,
        DEAD = 3,
    }


    // Life and hit related attributes
    [Header("Life")]
    [HideInInspector] public int life = 3;
    public int lifeMax = 3;
    public float invincibilityDuration = 1.0f;
    public float invincibilityBlinkPeriod = 0.2f;
    public LayerMask hitLayers;
    public float knockbackSpeed = 3.0f;
    public float knockbackDuration = 0.5f;

    private float _lastHitTime = float.MinValue;
    private List<SpriteRenderer> _spriteRenderers = new List<SpriteRenderer>();
    private Coroutine _blinkCoroutine = null;


    // Movement attributes
    [Header("Movement")]
    public MovementParameters defaultMovement = new MovementParameters();
    public MovementParameters stunnedMovement = new MovementParameters();

    private Rigidbody2D _body = null;
    private Vector2 _direction = Vector2.zero;
    private MovementParameters _currentMovement = null;

    // Attack attributes
    [Header("Attack")]
    public GameObject attackPrefab = null;
    public GameObject attackSpawnPoint = null;
    public float attackCooldown = 0.3f;
    public ORIENTATION orientation = ORIENTATION.FREE;

    private float lastAttackTime = float.MinValue;

    [Header("Experience")]
    public float maxExperience = 100;
    public float currentExperience = 0;
    public float fivePercent = 0;
    private int nextMilestone = 1;

    private int nbKeyPiece = 0;
    [HideInInspector] public bool hasSecretKey = false;
    bool alreadyHealed = false;
    bool[] keyPieceUnlocked = new bool[3] { false, false, false };
    public int attackPower = 1;


    // Input attributes
    [Header("Input")]
    [Range(0.0f, 1.0f)]
    public float controllerDeadZone = 0.3f;

    // State attributes
    private STATE _state = STATE.IDLE;

    // Collectible attributes
    private int _keyCount;
    public int KeyCount { get { return _keyCount; } set { _keyCount = value; } }

	// Dungeon position
	private Room _room = null;
	public Room Room { get { return _room; } }


	// Use this for initialization
	private void Awake () {
        Instance = this;
        life = lifeMax;
        _body = GetComponent<Rigidbody2D>();
        GetComponentsInChildren<SpriteRenderer>(true, _spriteRenderers);
    }

    private void Start()
    {
        SetState(STATE.IDLE);
        fivePercent = (maxExperience * 5) / 100;
        currentExperience = fivePercent;
    }

    // Update is called once per frame
    private void Update () {
        UpdateState();
        UpdateInputs();
	}

    // Update physics on FixedUpdate (FixedUpdate can be called multiple times a frame).
    private void FixedUpdate()
    {
        FixedUpdateMovement();
		FixedUpdateRoom();
	}

	private void FixedUpdateRoom()
	{
		Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
		if(colliders != null && colliders.Length > 0)
		{
			foreach(Collider2D collider in colliders)
			{
				if(collider.gameObject.tag == "Door")
				{
					Room room = collider.gameObject.GetComponentInParent<Room>();
					if(room && room != _room)
					{
                        Door thisDoor = collider.GetComponent<Door>();
                            room.OnEnterRoom();
					}
				}
			}
		}
	}

	// Update inputs
	private void UpdateInputs()
    {
        if (CanMove())
        {
            _direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_direction.magnitude < controllerDeadZone)
            {
                _direction = Vector2.zero;
            } else {
                _direction.Normalize();
            }
            if(Input.GetButtonDown("Fire1")) {
                Attack();
            }
        } else {
            _direction = Vector2.zero;
        }
    }

    // Update states
    private void UpdateState()
    {
        switch(_state)
        {
			case STATE.ATTACKING:
				SpawnAttackPrefab();
				SetState(STATE.IDLE);
				break;
			default: break;
        }
    }

    // Set state exits previous state, change state then enter new state. Instructions related to exiting and entering a state are in the two "switch(_state){...}" of this method.
    private void SetState(STATE state)
    {
        // Exit previous state
        // switch (_state)
        //{
        //}

        _state = state;
        // Enter new state
        switch (_state)
        {
            case STATE.STUNNED: _currentMovement = stunnedMovement; break;
            case STATE.DEAD:
                {
                    EndBlink();
                    Room startRoom = Room.allRooms.Find(x => x.position == Vector2Int.zero);
                    Player.Instance.transform.position = startRoom.GetWorldRoomBounds().center;
                    DungeonGenerator.instance.ReturnDoorToState();
                    startRoom.OnEnterRoom();
                    FullHeal();
                    SetState(STATE.IDLE);
                }
                break;
            default: _currentMovement = defaultMovement; break;
        }

        // Reset direction if player cannot move in this state
        if (!CanMove())
        {
            _direction = Vector2.zero;
        }
    }

    void FullHeal()
    {
        life = lifeMax;
        for (int i = 0; i < life; ++i)
        {
            Hud.Instance.AddHearth();
        }
    }


    // Update movement and friction
    void FixedUpdateMovement()
    {
        if (_direction.magnitude > Mathf.Epsilon) // magnitude > 0
        {
            // If direction magnitude > 0, Accelerate in direction, then clamp velocity to max speed. Do not apply friction if character is moving toward a direction.
            _body.velocity += _direction * _currentMovement.acceleration * Time.fixedDeltaTime;
            _body.velocity = Vector2.ClampMagnitude(_body.velocity, _currentMovement.speedMax);
            transform.eulerAngles = new Vector3(0.0f, 0.0f, ComputeOrientationAngle(_direction));
        } else {
            // If direction magnitude == 0, Apply friction
            float frictionMagnitude = _currentMovement.friction * Time.fixedDeltaTime;
            if (_body.velocity.magnitude > frictionMagnitude)
            {
                _body.velocity -= _body.velocity.normalized * frictionMagnitude;
            } else {
                _body.velocity = Vector2.zero;
            }
        }
    }

	// Attack method sets player in attack state. Attack prefab is spawned by calling SpawnAttackPrefab method.
	private void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        lastAttackTime = Time.time;
        SetState(STATE.ATTACKING);
    }

    // This method spawns the associated prefab on attackSpawnPoint.
    private void SpawnAttackPrefab()
    {
        if (attackPrefab == null)
            return;

        // transform used for spawn is attackSpawnPoint.transform if attackSpawnPoint is not null. Else it's transform.
        Transform spawnTransform = attackSpawnPoint ? attackSpawnPoint.transform : transform;
        Attack attack = Instantiate(attackPrefab, spawnTransform.position, spawnTransform.rotation).GetComponent<Attack>();
        attack.damages = attackPower;
    }

    // Applyhit is called when player touches an enemy hitbox or any hazard.
    public void ApplyHit(Attack attack)
    {
        if (Time.time - _lastHitTime < invincibilityDuration)
            return;
        _lastHitTime = Time.time;

        life -= (attack != null ? attack.damages : 1);
        if (life <= 0)
        {
            SetState(STATE.DEAD);
        } else {
            if (attack != null && attack.knockbackDuration > 0.0f)
            {
                StartCoroutine(ApplyKnockBackCoroutine(attack.knockbackDuration, attack.transform.right * attack.knockbackSpeed));
            }
            EndBlink();
            _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }
    }

    // ApplyKnockBackCoroutine puts player in STUNNED state and sets a velocity to knockback player. It resume to IDLE state after a fixed duration. STUNNED state has his own movement parameters that allow to redefine frictions when character is knocked.
    private IEnumerator ApplyKnockBackCoroutine(float duration, Vector3 velocity)
    {
        SetState(STATE.STUNNED);
        _body.velocity = velocity;
        yield return new WaitForSeconds(duration);
        SetState(STATE.IDLE);
    }

    // BlinkCoroutine makes all sprite renderers in the player hierarchy blink from enabled to disabled with a fixed period over a fixed time.  
    private IEnumerator BlinkCoroutine()
    {
        float invincibilityTimer = 0;
        while(invincibilityTimer < invincibilityDuration)
        {
            invincibilityTimer += Time.deltaTime;
            bool isVisible = ((int)(invincibilityTimer / invincibilityBlinkPeriod)) % 2 == 1;
            foreach(SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.enabled = isVisible;
            }
            yield return null; // wait next frame
        }
        EndBlink();
    }

    // Stops current blink coroutine if any is started and set all sprite renderers to enabled.
    private void EndBlink()
    {
        if (_blinkCoroutine == null)
            return;
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.enabled = true;
        }
        StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = null;

    }

    // Transforms the orientation vector into a discrete angle.
    private float ComputeOrientationAngle(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        switch(orientation)
        {
            case ORIENTATION.DPAD_8: return Utils.DiscreteAngle(angle, 45); // Only 0 45 90 135 180 225 270 315
            case ORIENTATION.DPAD_4: return Utils.DiscreteAngle(angle, 90); // Only 0 90 180 270
            default: return angle;
        }
    }

    // Can player moves or attack
    private bool CanMove()
    {
        return _state == STATE.IDLE;
    }

	public void EnterRoom(Room room)
	{
        if (room.gameObject.name == "BaseRoom(Clone)")
        {
            bool nodeExists = DungeonManager.instance.allNodes.TryGetValue(room.position, out DungeonManager.instance.currentNode);
            if (!nodeExists)
            {
                print("there's a problem");
            }
            if (_room != null)
            {
                if (_room.GetComponent<Configuration>().diffucultyLevel < 4)
                {
                    float amountToLoose = (((_room.GetComponent<Configuration>().diffucultyLevel * 1 / 3) * fivePercent) - fivePercent) + (fivePercent * 0.5f);
                    currentExperience = Mathf.Clamp(currentExperience - amountToLoose, 0, maxExperience);
                    while (currentExperience <= (nextMilestone-1) * (maxExperience * (100 / 13) / 100))
                    {
                        RevertReward(nextMilestone);
                        --nextMilestone;
                    }
                    if(nextMilestone == 0)
                    {
                        nextMilestone++;
                    }
                }
                else
                {
                    currentExperience = Mathf.Clamp(currentExperience + ((_room.GetComponent<Configuration>().diffucultyLevel * (fivePercent * 0.5f)) / 3), 0, maxExperience);

                    while (currentExperience >= nextMilestone * (maxExperience * (100 / 13) / 100))
                    {
                        UnlockReward(nextMilestone);
                        ++nextMilestone;
                    }
                }
                transform.position += (room.GetWorldRoomBounds().center - _room.GetWorldRoomBounds().center).normalized;
            }
            InterfaceManager.instance.ShowSelectionPanel(true);
        }
        else
        {
            foreach (Transform child in room.transform.GetChild(1))
            {
                if (child.CompareTag("Enemy"))
                {
                    transform.position += (room.GetWorldRoomBounds().center - _room.GetWorldRoomBounds().center).normalized;

                    foreach (Transform floor in room.transform.GetChild(0))
                    {
                        if (floor.CompareTag("Door"))
                        {
                            floor.GetComponent<Door>().previousState = floor.GetComponent<Door>().State;
                            floor.GetComponent<Door>().SetState(Door.STATE.WALL);
                        }
                    }

                    break;
                }
            }
        }
        DungeonGenerator.instance.currentRoom = room;
        _room = room;
	}

    void RevertReward(int rewardIndex)
    {
        switch (rewardIndex)
        {
            case 3:
            case 8:
            case 11:
                attackCooldown += 0.1f;
                break;
            case 7:
            case 10:
                {
                    lifeMax--;
                    if(life > lifeMax)
                    {
                        life = lifeMax;
                    }
                }
                break;
            case 1:
            case 12:
                defaultMovement.speedMax -= 0.5f;
                break;
            default:
                print("pranked, there's nothing for you to loose");
                break;
        }
    }

    void UnlockReward(int rewardIndex)
    {
        switch (rewardIndex)
        {
            case 5:
                if (!alreadyHealed)
                {
                    FullHeal();
                    alreadyHealed = true;
                }
                break;
            case 3 :
            case 8 :
            case 11:
                attackCooldown -= 0.1f;
                break;
            case 2:
            case 4:
            case 6:
                UnlockKeyPiece(rewardIndex);
                break;
            case 7:
            case 10:
                {
                    lifeMax++;
                    FullHeal();
                }
                break;
            case 1:
            case 12:
                defaultMovement.speedMax += 1.5f;
                break;
            default:
                print("pranked, there's nothing for you to win");
                break;
        }
    }

    void UnlockKeyPiece(int index)
    {
        switch (index)
        {
            case 3:
                {
                    if (!keyPieceUnlocked[0])
                    {
                        nbKeyPiece++;
                        keyPieceUnlocked[0] = true;
                    }
                }
                break;
            case 7:
                {
                    if (!keyPieceUnlocked[1])
                    {
                        nbKeyPiece++;
                        keyPieceUnlocked[1] = true;
                    }
                }
                break;
            case 11:
                {
                    if (!keyPieceUnlocked[2])
                    {
                        nbKeyPiece++;
                        keyPieceUnlocked[2] = true;
                    }
                    if (nbKeyPiece >= 3)
                    {
                        hasSecretKey = true;
                    }
                }
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( ((1 << collision.gameObject.layer) & hitLayers) != 0 )
        {
            // Collided with hitbox
            Attack attack = collision.gameObject.GetComponent<Attack>();
            ApplyHit(attack);
        }
    }
}
