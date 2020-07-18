using PureMVC.Interfaces;
using PureMVC.Patterns.Command;

public class InitializeModelCommand : SimpleCommand
{

	public override void Execute(INotification notification)
	{
		AddCfgProxy();
		AddRuntimeProxy();
		AddSingleton();
	}

	private void AddCfgProxy()
	{
		Facade.RegisterProxy(new CfgLanguageProxy());
		Facade.RegisterProxy(new CfgSettingProxy());
		Facade.RegisterProxy(new CfgEternityProxy());
		Facade.RegisterProxy(new CfgSkillSystemProxy());
		Facade.RegisterProxy(new GameLocalizationProxy());
		Facade.RegisterProxy(new CfgStarmapProxy());
	}

	private void AddRuntimeProxy()
	{
		Facade.RegisterProxy(new SceneShipProxy());
		Facade.RegisterProxy(new ChatProxy());
		Facade.RegisterProxy(new FoundryProxy());
		Facade.RegisterProxy(new FriendProxy());
		Facade.RegisterProxy(new LoginProxy());
		Facade.RegisterProxy(new MailProxy());
		Facade.RegisterProxy(new LogProxy());
        Facade.RegisterProxy(new ShipProxy());
        Facade.RegisterProxy(new PackageProxy());
        Facade.RegisterProxy(new ShopProxy());
		Facade.RegisterProxy(new PveProxy());
		Facade.RegisterProxy(new ServerListProxy());
		//Facade.RegisterProxy(new StoreHelperProxy());
		Facade.RegisterProxy(new TeamProxy());
		Facade.RegisterProxy(new GameplayProxy());
		Facade.RegisterProxy(new PlayerSkillProxy());

        Facade.RegisterProxy(new TaskTrackingProxy());
        Facade.RegisterProxy(new MSAIBossProxy());
        Facade.RegisterProxy(new TreasureHuntProxy());
		Facade.RegisterProxy(new ShipItemsProxy());

		Facade.RegisterProxy(new MissionProxy());
		Facade.RegisterProxy(new RaycastProxy());
		Facade.RegisterProxy(new AutoNavigationProxy());
		Facade.RegisterProxy(new TalentProxy());
	}

	private void AddSingleton()
	{
		//WwiseManager.Instance.Initialize();
	}

}