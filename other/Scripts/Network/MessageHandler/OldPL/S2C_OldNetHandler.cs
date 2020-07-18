
namespace Crucis.Protocol
{
    [MessageHandler]
    public class G2C_OldNetHandler: AMHandler<OldPl>
    {
        protected override void Run(Connection session, OldPl message)
        {
            NetworkManager.Instance?.OnResiveMessage(message);
        }
    }
}