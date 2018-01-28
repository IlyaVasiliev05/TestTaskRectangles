using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleManager : MonoBehaviour {

#region vars
    Camera cam;
	public GameObject rect;
	public GameObject link;
	public SpriteRenderer pointerRect;
    public Vector2 [] cameraLimits = new Vector2[2]; // piece of hardcode

	List<Rectangle>rectsList = new List<Rectangle>();
	List<Link> linksList = new List<Link>();

	Vector2 currentCursorPosition;
	[Header("Visuals")]
	public Color greenPointerRectColor;
	public Color redPointerRectColor;
	public Texture2D normalCursorTexture;
	public Texture2D moveCursorTexture;


    Coroutine dragNDropRoutine;
	Rectangle currentRect;
	bool onMove = false;
	bool onLinkCreation = false;
    bool mouseIsOnLink = false;
	float lastClickTime;
#endregion

    void Start()
	{
		cam = Camera.main;
    }

	void Update()
	{
		currentCursorPosition = CursorPositionToWorld(cam.ScreenToWorldPoint(Input.mousePosition)); // Преобразуем позицию курсора в Vector2 в мировых координатах

		ManageCursor(currentCursorPosition); // Визуальная мешура к курсору
        CheckMouseOnLink(); // 

		if(Input.GetMouseButtonDown(0)) // Обработка начала клика ЛКМ
		{
			InstantiateNewRect(currentCursorPosition); // создаём новый прямоугольник
			if(Time.timeSinceLevelLoad - lastClickTime < 0.2f && currentRect != null) // Проверка на двойной клик
			{
				RemoveRect();
			}
			else
				if(currentRect != null)
					StartCoroutine(DragRectangle(currentRect.transform)); // если клик одинарный - запускаем корутину перемещения

			lastClickTime = Time.timeSinceLevelLoad;
		}

		if(Input.GetMouseButtonUp(1)) // Обработка конца клика ПКМ
		{
			if(currentRect != null && !onLinkCreation)
				StartCoroutine(ProcessLinkCreation()); // запуск процесса создания связи
		}

	}

    void CheckMouseOnLink() // Проверяем, находится ли мышь на линии связи
    {
        RaycastHit2D hit;
        hit = Physics2D.Raycast(currentCursorPosition, Vector2.up, 0.001f);
        if(hit.collider != null)
        {
            mouseIsOnLink = hit.collider.CompareTag("Link");
        }
        else
            mouseIsOnLink = false;
    }

	/// <summary>
	/// Создаёт новый прямоугольник, запускает процесс перемещения прямоугольника, чтобы сразу была возможность подстроить местоположение
	/// </summary>
	/// <param name="cursorPosition">позиция курсора для создания прямоугольника</param>
	void InstantiateNewRect(Vector2 cursorPosition)
	{
		if(!CanPlaceRect(cursorPosition) || onLinkCreation || mouseIsOnLink)
			return;
		if(rectsList.Count == 10)
			return;
		GameObject newRect = Instantiate(rect,new Vector2(cursorPosition.x, cursorPosition.y), Quaternion.identity);
		newRect.GetComponent<SpriteRenderer>().color = new Color(Random.Range(0,1.0f), Random.Range(0,1.0f), Random.Range(0,1.0f));
		rectsList.Add(newRect.GetComponent<Rectangle>());
		currentRect = newRect.GetComponent<Rectangle>();
		StartCoroutine(DragRectangle(currentRect.transform));
	}

	void RemoveRect()
	{
		rectsList.Remove(currentRect);
		currentRect.DeleteRectangle();
	}

	Vector2 CursorPositionToWorld(Vector3 _cursorPosition)
	{
		return new Vector2(_cursorPosition.x, _cursorPosition.y);
	}

	bool CanPlaceRect(Vector2 anticipatedPosition)
	{
		foreach(Rectangle theRect in rectsList)
		{
			Bounds theRectBounds = theRect.bounds;
			Vector2 vectorBetweenCenters = 
				new Vector2(Mathf.Abs(anticipatedPosition.x - theRectBounds.center.x), Mathf.Abs(anticipatedPosition.y - theRectBounds.center.y));
			
			if(vectorBetweenCenters.x < theRectBounds.size.x && vectorBetweenCenters.y < theRectBounds.size.y)
			{
				return false;
			}
		}
        return RectFitsCameraLimits(anticipatedPosition, rect.GetComponent<Renderer>().bounds);
	}

    bool RectFitsCameraLimits(Vector2 anticipatedPos, Bounds rectBounds)
    {
        if (anticipatedPos.x + rectBounds.extents.x > cameraLimits[0].x || anticipatedPos.y + rectBounds.extents.y > cameraLimits[0].y)
            return false;
        else
            if (anticipatedPos.x - rectBounds.extents.x < cameraLimits[1].x || anticipatedPos.y - rectBounds.extents.y < cameraLimits[1].y)
            return false;
        else
            return true;

    }

	bool CanMoveRect(Vector2 anticipatedPosition, Rectangle movingRect)
	{
		foreach(Rectangle theRect in rectsList)
		{
			if(theRect == movingRect)
				continue;
			Bounds theRectBounds = theRect.bounds;
			Vector2 vectorBetweenCenters = 
				new Vector2(Mathf.Abs(anticipatedPosition.x - theRectBounds.center.x), Mathf.Abs(anticipatedPosition.y - theRectBounds.center.y));

			if(vectorBetweenCenters.x < theRectBounds.size.x && vectorBetweenCenters.y < theRectBounds.size.y)
			{
				return false;
			}
		}
		return RectFitsCameraLimits(anticipatedPosition, rect.GetComponent<Renderer>().bounds);
    }

	bool CursorIsOnRect(Vector2 cursorPosition)
	{
		foreach(Rectangle theRect in rectsList)
		{
			Bounds theRectBounds = theRect.bounds;

			if(theRectBounds.Contains(cursorPosition))
			{
				currentRect = theRect.GetComponent<Rectangle>();
				return true;
			}
		}
		currentRect = null;
		return false;
	}

	IEnumerator DragRectangle(Transform rect)
	{
		onMove = true;
		Vector2 clickOffset = (Vector2)rect.position - currentCursorPosition;
		Vector2 lastApropriatePosition = currentCursorPosition;
		while(Input.GetMouseButton(0))
		{
			yield return null;
			if(CanMoveRect(currentCursorPosition + clickOffset, rect.GetComponent<Rectangle>()))
				lastApropriatePosition = currentCursorPosition + clickOffset;
			rect.position = Vector3.Lerp(rect.position, lastApropriatePosition, Time.deltaTime * 25f);
            rect.GetComponent<Rectangle>().UpdateLinks();
        }
		if(CanMoveRect(currentCursorPosition + clickOffset, rect.GetComponent<Rectangle>()))
			rect.position = lastApropriatePosition;
        rect.GetComponent<Rectangle>().UpdateLinks();
        onMove = false;
		yield break;
	}

	IEnumerator ProcessLinkCreation()
	{
		Rectangle startRect = currentRect.GetComponent<Rectangle>();
		Link theLink = Instantiate(link, Vector3.zero, Quaternion.identity).GetComponent<Link>();
		while(!Input.GetMouseButtonDown(1))
		{
			onLinkCreation = true;
			theLink.UpdateLinkByVectors(Link.FindClosestCenterPoint(startRect.bounds, currentCursorPosition), currentCursorPosition);
			yield return null;
		}

		if(currentRect != null)
		{
			if(currentRect.GetComponent<Rectangle>() != startRect)
				AddNewLink(startRect, currentRect.GetComponent<Rectangle>(), theLink);
			else
				Destroy(theLink.gameObject);
		}
		else Destroy(theLink.gameObject);

		while(!Input.GetMouseButtonUp(1))
		{
			yield return null;
		}
		onLinkCreation = false;
		yield break;
	}

	void AddNewLink(Rectangle startRect, Rectangle endRect, Link theLink)
	{
		theLink.linkedRects[0] = startRect;
		theLink.linkedRects[1] = endRect;
		startRect.rectLinks.Add(theLink);
		endRect.rectLinks.Add(theLink);

		theLink.UpdateLinkByRects();
	}

	void ManageCursor(Vector2 cursorPosition)
	{
		pointerRect.transform.position = cursorPosition;

		if(CursorIsOnRect(cursorPosition) || onMove)
		{
			Cursor.SetCursor(moveCursorTexture, Vector2.zero, CursorMode.Auto);
			pointerRect.enabled = false;
		}
		else
		{
			Cursor.SetCursor(normalCursorTexture,Vector2.zero, CursorMode.Auto);
			pointerRect.enabled = true;
			if(CanPlaceRect(cursorPosition))
				pointerRect.color = greenPointerRectColor;
			else
				pointerRect.color = redPointerRectColor;
		}

		if(onLinkCreation || mouseIsOnLink)
		{
			pointerRect.enabled = false;
		}
	}

}
