using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
	public BoxCollider2D gridArea;
	public Transform snake;

	private void Start()
	{
		RandomizePositions();
	}

	private void RandomizePositions()
	{
		Bounds bounds = gridArea.bounds;
		bool onSnake;
		float x;
		float y;

		do
		{
			onSnake = false;
			x = Mathf.Round(Random.Range(bounds.min.x, bounds.max.x));
			y = Mathf.Round(Random.Range(bounds.min.y, bounds.max.y));

			GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

			foreach(GameObject p in obstacles)
			{
				if((x == p.transform.position.x && y == p.transform.position.y) || (x == snake.position.x && y == snake.position.y))
				{
					onSnake = true;
					break;
				}
			}
		} while(onSnake);

		this.transform.position = new Vector3(x, y, 0.0f);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Player")
		{
			RandomizePositions();
		}
	}
}
