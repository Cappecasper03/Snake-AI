using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameArea : MonoBehaviour
{
	public Camera mainCamera;
	public BoxCollider2D gridArea;
	public Transform rightWall;
	public Transform leftWall;
	public Transform topWall;
	public Transform botWall;

	public int areaLenght = 10;
	static public MapLocation[] cornerPos = new MapLocation[4];

	private void Awake()
	{
		if(areaLenght < 4)
		{
			areaLenght = 4;
		}
		else if(areaLenght % 2 != 0)
		{
			areaLenght++;
		}

		float halfArea = areaLenght / 2f + 0.5f;
		const int edgeLenght = 2;

		mainCamera.orthographicSize = halfArea + 1;
		gridArea.size = new Vector2(areaLenght - 1, areaLenght - 1);

		rightWall.localPosition = new Vector3(halfArea, 0, 0);
		rightWall.localScale = new Vector3(1, areaLenght + edgeLenght, 1);

		leftWall.localPosition = new Vector3(-halfArea, 0, 0);
		leftWall.localScale = new Vector3(1, areaLenght + edgeLenght, 1);

		topWall.localPosition = new Vector3(0, halfArea, 0);
		topWall.localScale = new Vector3(areaLenght + edgeLenght, 1, 1);

		botWall.localPosition = new Vector3(0, -halfArea, 0);
		botWall.localScale = new Vector3(areaLenght + edgeLenght, 1, 1);

		int lenghtFromOrigo = 0;
		for(int i = 4; i <= areaLenght; i++)
		{
			if(i % 2 == 0)
			{
				lenghtFromOrigo++;
			}
		}
		cornerPos[0] = new MapLocation(-lenghtFromOrigo, -lenghtFromOrigo); // Bottom left corner
		cornerPos[1] = new MapLocation(cornerPos[0].x + areaLenght - 1, cornerPos[0].y); // Bottom right corner
		cornerPos[2] = new MapLocation(cornerPos[0].x, cornerPos[0].y + areaLenght - 1); // Top left corner
		cornerPos[3] = new MapLocation(cornerPos[0].x + areaLenght - 1, cornerPos[0].y + areaLenght - 1); // Top right corner
	}
}
