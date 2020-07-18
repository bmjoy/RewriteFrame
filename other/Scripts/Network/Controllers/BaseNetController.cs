namespace Game.Frame.Net
{
	public class BaseNetController
	{
		public BaseNetController()
		{
			NetworkManager.Instance._AddController(this);
		}
	}
}