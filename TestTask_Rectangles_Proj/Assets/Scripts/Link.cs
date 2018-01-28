using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link : MonoBehaviour 
{
	public Rectangle [] linkedRects = new Rectangle[2];
    LineRenderer lineRenderer;
    PolygonCollider2D polygonCollider;
    float timeSinceLastClick = 0;

    //Метод находит красивую точку, располагающиюся по центру стороны прямоугольника, ближайшую к point
    //Bounds.ClosestPoint не использован дабы избежать попадания начала и конца линии на углы
	public static Vector2 FindClosestCenterPoint(Bounds bounds, Vector2 point)
	{
		Vector2 [] boundsPoints = new Vector2[4]; // потенциальные точки
		boundsPoints[0] = (Vector2)bounds.center + Vector2.up * bounds.extents.y * 0.97f;
		boundsPoints[1] = (Vector2)bounds.center - Vector2.up * bounds.extents.y * 0.97f;
		boundsPoints[2] = (Vector2)bounds.center + Vector2.right * bounds.extents.x * 0.97f;
		boundsPoints[3] = (Vector2)bounds.center - Vector2.right * bounds.extents.x * 0.97f;
		float minDistance = float.MaxValue;
		Vector2 minPoint = Vector2.zero;
		foreach(Vector2 boundsPoint in boundsPoints)
		{
			if(Vector2.Distance(boundsPoint, point) < minDistance)
			{
				minDistance = Vector2.Distance(boundsPoint, point);
				minPoint = boundsPoint;
			}
		}
		return minPoint;
	}

	void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
	}
    // Метод вызывается в момент создания линии, принимает 2 вектора, не обновляет коллайдер
	public void UpdateLinkByVectors(Vector3 startPoint, Vector3 endPoint)
	{
		lineRenderer.SetPosition(0, startPoint);
		lineRenderer.SetPosition(1, endPoint);
	}
    
    //Обновляет линию, основываясь на прямоугольниках, которые она соединяет
	public void UpdateLinkByRects()
	{
		lineRenderer.SetPosition(0, FindClosestCenterPoint(linkedRects[0].bounds, linkedRects[1].transform.position));
		lineRenderer.SetPosition(1, FindClosestCenterPoint(linkedRects[1].bounds, linkedRects[0].transform.position));
        UpdateLinkCollider();
	}

    // Отслеживание нажатия (двойного). Используется для удаления линии
    void OnMouseUp()
    {
        if (Time.timeSinceLevelLoad - timeSinceLastClick < 0.2f)
        {
            DeleteLink();
        }
        timeSinceLastClick = Time.timeSinceLevelLoad;
    }

    /// <summary>
    /// Обновляет опорные точки PolygonCollider2D у связи
    /// </summary>
    public void UpdateLinkCollider()
    {
        Vector2 linkVectorNormalized = (lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)).normalized;//нормализованный вектор направляния связи
        Vector2 orthoVector = Quaternion.Euler(0, 0, 90) * (linkVectorNormalized * lineRenderer.startWidth/1.6f); // перпендикуляр вправо от направления

        Vector2[] newColliderpoints = new Vector2[4];
        newColliderpoints[0] = lineRenderer.GetPosition(0) - (Vector3)orthoVector;
        newColliderpoints[1] = lineRenderer.GetPosition(0) + (Vector3)orthoVector;
        newColliderpoints[2] = lineRenderer.GetPosition(1) + (Vector3)orthoVector;
        newColliderpoints[3] = lineRenderer.GetPosition(1) - (Vector3)orthoVector;
        polygonCollider.SetPath(0, newColliderpoints);
    }

    //Удаляет себя (связь) только из списка прямоугольника, который не находится в процессе удаления
	public void RemoveLinkOnRectDelete(Rectangle rect)
	{
		if(rect == linkedRects[0])
			linkedRects[1].rectLinks.Remove(this);
		else
			linkedRects[0].rectLinks.Remove(this);
		Destroy(gameObject);
	}

    //Полностью удаляем связь
	public void DeleteLink()
	{
		foreach(Rectangle rct in linkedRects) // Удаляем ссылку на связь у каждого из 2х прямоугольников
		{
			rct.rectLinks.Remove(this);
		}
		Destroy(gameObject);
	}

}
