using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//* A*
public class MapLocation
{
	public int x;
	public int y;

	public MapLocation(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2 ToVector()
	{
		return new Vector2(x, y);
	}

	public static MapLocation operator +(MapLocation a, MapLocation b)
			=> new MapLocation(a.x + b.x, a.y + b.y);

	public override bool Equals(object obj)
	{
		if((obj == null) || !this.GetType().Equals(obj.GetType()))
		{
			return false;
		}
		else
		{
			return x == ((MapLocation)obj).x && y == ((MapLocation)obj).y;
		}
	}

	public override int GetHashCode()
	{
		return 0;
	}
}

public class PathMarker
{
	public MapLocation location;
	public float G;
	public float H;
	public float F;
	public GameObject marker;
	public PathMarker parent;

	public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
	{
		location = l;
		G = g;
		H = h;
		F = f;
		this.marker = marker;
		parent = p;
	}

	public override bool Equals(object obj)
	{
		if((obj == null) || !this.GetType().Equals(obj.GetType()))
		{
			return false;
		}
		else
		{
			return location.Equals(((PathMarker)obj).location);
		}
	}

	public override int GetHashCode()
	{
		return 0;
	}
}

public class FindPath : MonoBehaviour
{
	public BoxCollider2D gridArea;

	private List<PathMarker> open = new List<PathMarker>();
	private List<PathMarker> closed = new List<PathMarker>();
	private List<MapLocation> directions = new List<MapLocation>() {
											new MapLocation(1,0),
											new MapLocation(0,1),
											new MapLocation(-1,0),
											new MapLocation(0,-1) };

	private List<List<MapLocation>> snakeClones = new List<List<MapLocation>>();
	private List<Vector2> moves = new List<Vector2>();

	public GameObject snake;
	public GameObject food;
	public GameObject pathP;

	private PathMarker startNode;
	private PathMarker goalNode;

	private PathMarker lastPos;
	private bool done = false;

	private bool hasMoved = false;

	private void RemoveAllMarkers()
	{
		GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
		foreach(GameObject m in markers)
		{
			Destroy(m);
		}
	}

	public void BeginSearch(MapLocation playerPos, MapLocation foodPos)
	{
		done = false;
		RemoveAllMarkers();

		startNode = new PathMarker(new MapLocation(playerPos.x, playerPos.y), 0, 0, 0, snake, null);
		goalNode = new PathMarker(new MapLocation(foodPos.x, foodPos.y), 0, 0, 0, food, null);
		//goalNode = new PathMarker(new MapLocation(playerPos.x, playerPos.y), 0, 0, 0, food, null);

		open.Clear();
		closed.Clear();
		open.Add(startNode);
		lastPos = startNode;
	}

	public void Search(PathMarker playerNode)
	{
		if(playerNode.Equals(goalNode) && hasMoved)
		{
			done = true;
			return;
		}

		foreach(MapLocation dir in directions)
		{
			MapLocation neighbourNode = dir + playerNode.location;

			bool hitsItself = false;
			int currentSnake = 0;

			for(int i = 0; i < snakeClones.Count; i++)
			{
				if(snakeClones[i][0].ToVector() == playerNode.location.ToVector())
				{
					snakeClones.Add(new List<MapLocation>(snakeClones[i]));
					MoveSnakeClone(dir);
					currentSnake = i;
					break;
				}
			}

			if(neighbourNode.x < -gridArea.size.x / 2 || neighbourNode.x > (gridArea.size.x + 1) / 2 ||
				neighbourNode.y < -gridArea.size.y / 2 || neighbourNode.y > (gridArea.size.y + 1) / 2)
			{
				continue;
			}

			if(isClosed(neighbourNode))
			{
				continue;
			}

			foreach(MapLocation snakePart in snakeClones[currentSnake])
			{
				if(neighbourNode.Equals(snakePart) && !neighbourNode.Equals(startNode.location))
				{
					hitsItself = true;
					break;
				}
			}

			if(hitsItself)
			{
				continue;
			}

			float G = Vector2.Distance(playerNode.location.ToVector(), neighbourNode.ToVector()) + playerNode.G;
			float H = Vector2.Distance(neighbourNode.ToVector(), goalNode.location.ToVector());
			float F = G + H;

			if(!UpdateMarker(neighbourNode, G, H, F, playerNode))
			{
				GameObject pathBlock = Instantiate(pathP, new Vector3(neighbourNode.x, neighbourNode.y, 0), Quaternion.identity);
				open.Add(new PathMarker(neighbourNode, G, H, F, pathBlock, playerNode));
			}
		}

		if(open.Count == 0)
		{
			done = true;
			return;
		}

		if(closed.Count == 0)
		{
			closed.Add(open.ElementAt(0));
			open.RemoveAt(0);
			hasMoved = true;
		}

		open = open.OrderBy(p => p.F).ToList<PathMarker>();
		//open = open.OrderByDescending(p => p.F).ToList<PathMarker>();

		if(open.Count > 0)
		{
			PathMarker pm = open.ElementAt(0);
			closed.Add(pm);
			open.RemoveAt(0);

			if(!pm.Equals(startNode))
			{
				pm.marker.GetComponent<SpriteRenderer>().color = Color.magenta;
			}

			lastPos = pm;
		}
	}

	private bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
	{
		foreach(PathMarker p in open)
		{
			if(p.location.Equals(pos))
			{
				p.G = g;
				p.H = h;
				p.F = f;
				p.parent = prt;
				return true;
			}
		}

		return false;
	}

	private bool isClosed(MapLocation marker)
	{
		foreach(PathMarker p in closed)
		{
			if(p.location.Equals(marker))
			{
				return true;
			}
		}

		return false;
	}

	void GetPath()
	{
		PathMarker path = lastPos;

		while(path != null && !path.Equals(startNode))
		{
			MovesToList(path);

			Instantiate(pathP, new Vector3(path.location.x, path.location.y, 1), Quaternion.identity);
			path.marker.GetComponent<SpriteRenderer>().color = Color.yellow;
			path = path.parent;
		}
	}

	//* Hardcoded Hamiltonian Cycle
	Vector2 HamiltonianCycle()
	{
		bool goUp = false;
		bool goDown = false;
		if((snake.transform.position.x - GameArea.cornerPos[0].x) % 2 == 0)
		{
			goUp = false;
			goDown = true;
		}
		else
		{
			goUp = true;
			goDown = false;
		}

		if((snake.transform.position.x == GameArea.cornerPos[1].x && snake.transform.position.y >= GameArea.cornerPos[0].y && snake.transform.position.y < GameArea.cornerPos[2].y) ||
				(snake.transform.position.y >= GameArea.cornerPos[0].y + 1 && snake.transform.position.y < GameArea.cornerPos[2].y && goUp))
		{
			return Vector2.up;
		}
		else if((snake.transform.position.x == GameArea.cornerPos[0].x && snake.transform.position.y <= GameArea.cornerPos[2].y && snake.transform.position.y > GameArea.cornerPos[0].y) ||
						(snake.transform.position.y > GameArea.cornerPos[0].y + 1 && snake.transform.position.y <= GameArea.cornerPos[2].y && goDown))
		{
			return Vector2.down;
		}
		else if(snake.transform.position.y == GameArea.cornerPos[2].y || snake.transform.position.y == GameArea.cornerPos[0].y + 1)
		{
			return Vector2.left;
		}

		return Vector2.right;
	}

	//* AI Move
	public Vector2 GetNextMove(List<Transform> segments)
	{
		if(!done)
		{
			BeginSearch(new MapLocation((int)snake.transform.position.x, (int)snake.transform.position.y), new MapLocation((int)food.transform.position.x, (int)food.transform.position.y));
			snakeClones.Clear();
			moves.Clear();

			snakeClones.Add(new List<MapLocation>());
			foreach(Transform segment in segments)
			{
				snakeClones[0].Add(new MapLocation((int)segment.position.x, (int)segment.position.y));
			}

			do
			{
				Search(lastPos);
			} while(!done);
		}

		if(done && moves.Count == 0)
		{
			GetPath();
		}

		if(done && moves.Count > 0)
		{
			Vector2 move = moves[moves.Count - 1];
			moves.RemoveAt(moves.Count - 1);

			return move;
		}

		return HamiltonianCycle();
	}

	private void MovesToList(PathMarker path)
	{
		if(path.location.x > path.parent.location.x)
		{
			moves.Add(Vector2.right);
		}
		else if(path.location.x < path.parent.location.x)
		{
			moves.Add(Vector2.left);
		}
		else if(path.location.y > path.parent.location.y)
		{
			moves.Add(Vector2.up);
		}
		else if(path.location.y < path.parent.location.y)
		{
			moves.Add(Vector2.down);
		}
	}

	private void MoveSnakeClone(MapLocation dir)
	{
		for(int i = snakeClones[snakeClones.Count - 1].Count - 1; i > 0; i--)
		{
			snakeClones[snakeClones.Count - 1][i] = snakeClones[snakeClones.Count - 1][i - 1];
		}

		snakeClones[snakeClones.Count - 1][0] += dir;
	}
}