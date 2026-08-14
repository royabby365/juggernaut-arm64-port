public interface ITouchscreen
{
	event TouchStartD OnTouchStart;

	event TouchEndD OnTouchEnd;

	event TouchMoveD OnTouchMove;

	event ZoomD OnZoom;

	void Update();

	void Clear();
}
