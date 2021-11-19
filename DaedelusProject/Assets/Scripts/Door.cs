using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour {

    public enum STATE {
        OPEN = 0,
        CLOSED = 1,
        WALL = 2,
        SECRET = 3,
        END = 4,
    }

    public const string PLAYER_NAME = "Player";

    Utils.ORIENTATION _orientation = Utils.ORIENTATION.NONE;
	public Utils.ORIENTATION Orientation { get { return _orientation; } }

	STATE _state = STATE.OPEN;
	public STATE State { get { return _state; } }
	public GameObject closedGo = null;
    public GameObject openGo = null;
    public GameObject wallGo = null;
    public GameObject secretGo = null;
    public GameObject endGo = null;

	private Room _room = null;

    public STATE previousState;

	public void Awake()
	{
		_room = GetComponentInParent<Room>();
    }

	public void Start()
    {
		if(closedGo.gameObject.activeSelf)
		{
			SetState(STATE.CLOSED);
		} else if (openGo.gameObject.activeSelf)
		{
			SetState(STATE.OPEN);
		} else if (wallGo.gameObject.activeSelf)
		{
			SetState(STATE.WALL);
		} else if (secretGo.gameObject.activeSelf)
		{
			SetState(STATE.SECRET);
		}
        else if (endGo.gameObject.activeSelf)
        {
            SetState(STATE.END);
        }
    }

    public void SetOrientation()
    {
        Bounds roomBounds = _room.GetLocalRoomBounds();
        float ratio = roomBounds.size.x / roomBounds.size.y;
        Vector2 dir = transform.position - (_room.transform.position + roomBounds.center);
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y) * ratio)
        {
            _orientation = dir.x > 0 ? Utils.ORIENTATION.EAST : Utils.ORIENTATION.WEST;
        }
        else
        {
            _orientation = dir.y > 0 ? Utils.ORIENTATION.NORTH : Utils.ORIENTATION.SOUTH;
        }
        transform.rotation = Quaternion.Euler(0, 0, -Utils.OrientationToAngle(_orientation));
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent != Player.Instance.gameObject.transform)
            return;

        switch (_state) {
            case STATE.CLOSED:
                if (Player.Instance.KeyCount > 0)
                {
                    Player.Instance.KeyCount--;
                    SetState(STATE.OPEN);
					Room nextRoom = GetNextRoom();
					if(nextRoom)
					{
						Door[] doors = nextRoom.GetComponentsInChildren<Door>(true);
						foreach(Door door in doors)
						{
							if (_orientation == Utils.OppositeOrientation(door.Orientation) && door._state == STATE.CLOSED)
							{
								door.SetState(STATE.OPEN);
							}
						}
					}
				}
                break;
            case STATE.END:
                InterfaceManager.instance.ShowEndPanel();
                Time.timeScale = 0;
                break;
            case STATE.SECRET:
                if (Player.Instance.hasSecretKey)
                {
                    SetState(STATE.OPEN);
                }
                break;
        }
    }

	private Room GetNextRoom()
	{
		Vector2Int dir = Utils.OrientationToDir(_orientation);
		Room nextRoom = Room.allRooms.Find(x => x.position == _room.position + dir);
		return nextRoom;
	} 

    public void SetState(STATE state)
    {
        if (closedGo) { closedGo.SetActive(false); }
        if (openGo) { openGo.SetActive(false); }
        if (wallGo) { wallGo.SetActive(false); }
        if (secretGo) { secretGo.SetActive(false); }
        if (endGo) { endGo.SetActive(false); }
        _state = state;
        switch(_state)
        {
            case STATE.CLOSED:
                if (closedGo) { closedGo.SetActive(true); }
                break;
            case STATE.OPEN:
                if (openGo) { openGo.SetActive(true); }
                break;
            case STATE.WALL:
                if (wallGo) { wallGo.SetActive(true); }
                break;
            case STATE.SECRET:
                if (secretGo) { secretGo.SetActive(true); }
                break;
            case STATE.END:
                if (endGo) { endGo.SetActive(true); }
                break;
        }
    }

}
