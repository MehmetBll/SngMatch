using UnityEngine;
using UnityEngine.UI;

public class SelectBG : MonoBehaviour
{
	public SpriteRenderer floorSpriteRenderer;
	public Image floorUIImage;
	public Image[] bgImages;

	public void SetFloorSpriteByIndex(int index)
	{
		if (bgImages == null) return;
		if (index < 0 || index >= bgImages.Length) return;

		Sprite s = bgImages[index]?.sprite;
		if (s == null) return;

		if (floorUIImage != null) floorUIImage.sprite = s;
		if (floorSpriteRenderer != null) floorSpriteRenderer.sprite = s;
	}

	public void SetFloorFromImage(Image img)
	{
		if (img == null || img.sprite == null) return;
		if (floorUIImage != null) floorUIImage.sprite = img.sprite;
		if (floorSpriteRenderer != null) floorSpriteRenderer.sprite = img.sprite;
	}
}
