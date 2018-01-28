using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rectangle : MonoBehaviour
{

    public List<Link> rectLinks; // все связи прямоугольника
	public SpriteRenderer spriteRenderer;

    public Bounds bounds 
	{
		get
		{
			return GetComponent<Renderer>().bounds;
		}
		set
		{
			bounds = value;
		}
	}

    void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void UpdateLinks()
	{
		foreach(Link lnk in rectLinks)
		{
			lnk.UpdateLinkByRects(); // Обновляет все связи прямоугольника
		}
	}

	public void DeleteRectangle()
	{
		foreach(Link lnk in rectLinks)
		{
			lnk.RemoveLinkOnRectDelete(this); // Удаляем у ссылку на связь с этим прямоугольником у других прямоугольников
		}
		Destroy(gameObject);
	}
}
