using UnityEngine;

public class InventoryCam : MonoBehaviour
{
	public Camera cam2D;

	public Transform bag;

	public Transform rootOfBag;

	private void Update()
	{
		SpriteGui component = rootOfBag.transform.GetComponent<SpriteGui>();
		Rect inventoryFrame = ((IViewportFrame)component).GetInventoryFrame();
		Vector3 vector = cam2D.WorldToScreenPoint(bag.position);
		float left = vector.x + inventoryFrame.x;
		float top = vector.y - inventoryFrame.y - inventoryFrame.height;
		Camera component2 = base.transform.GetComponent<Camera>();
		component2.pixelRect = new Rect(left, top, inventoryFrame.width + 3f, inventoryFrame.height + 3f);
	}
}
