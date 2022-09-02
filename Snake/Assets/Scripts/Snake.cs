using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
	public bool usingAI = true;
	public FindPath AI;
	public Text pointsTxt;

	private Vector2 direction = Vector2.zero;
	static public List<Transform> segments = new List<Transform>();
	public Transform segmentPrefab;
	public int initialSize = 2;

	private void Start()
	{
		if(initialSize < 2)
		{
			initialSize = 2;
		}

		ResetState();
		if(usingAI)
		{
			AI.BeginSearch(new MapLocation((int)this.transform.position.x, (int)this.transform.position.y), new MapLocation((int)AI.food.transform.position.x, (int)AI.food.transform.position.y));
		}
	}

	private void Update()
	{
		if(!usingAI)
		{
			if(Input.GetKeyDown(KeyCode.W) && direction != Vector2.down)
			{
				direction = Vector2.up;
			}
			else if(Input.GetKeyDown(KeyCode.S) && direction != Vector2.up)
			{
				direction = Vector2.down;
			}
			else if(Input.GetKeyDown(KeyCode.A) && direction != Vector2.right)
			{
				direction = Vector2.left;
			}
			else if(Input.GetKeyDown(KeyCode.D) && direction != Vector2.left)
			{
				direction = Vector2.right;
			}
		}
	}

	private void FixedUpdate()
	{
		if(usingAI)
		{
			direction = AI.GetNextMove(segments);
		}

		for(int i = segments.Count - 1; i > 0; i--)
		{
			segments[i].position = segments[i - 1].position;
		}

		this.transform.position = new Vector3(
			Mathf.Round(this.transform.position.x + direction.x),
			Mathf.Round(this.transform.position.y + direction.y),
			0.0f
		);
	}

	private void Grow()
	{
		Transform segment = Instantiate(this.segmentPrefab);
		segment.position = segments[segments.Count - 1].position;

		segments.Add(segment);
	}

	private void ResetState()
	{
		for(int i = 1; i < segments.Count; i++)
		{
			Destroy(segments[i].gameObject);
		}

		segments.Clear();
		segments.Add(this.transform);

		for(int i = 1; i < initialSize; i++)
		{
			segments.Add(Instantiate(this.segmentPrefab));
		}

		this.transform.position = Vector3.zero;
		direction = Vector2.zero;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Food")
		{
			Grow();
			pointsTxt.text = $"Lenght: {segments.Count}";

			int areaLenght = GameArea.cornerPos[1].x - GameArea.cornerPos[0].x + 1;
			if(segments.Count == Mathf.Pow(areaLenght, 2))
			{
				Debug.Break();
			}

			if(usingAI)
			{
				AI.BeginSearch(new MapLocation((int)this.transform.position.x, (int)this.transform.position.y), new MapLocation((int)other.transform.position.x, (int)other.transform.position.y));
			}
		}
		else if(other.tag == "Obstacle")
		{
			ResetState();
			if(usingAI)
			{
				AI.BeginSearch(new MapLocation((int)this.transform.position.x, (int)this.transform.position.y), new MapLocation((int)AI.food.transform.position.x, (int)AI.food.transform.position.y));
			}
		}
	}
}