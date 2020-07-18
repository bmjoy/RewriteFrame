/// <summary>
/// 因为IconBase 为抽象类
/// 所以不需要自定义派生类的可以使用这个类
/// </summary>
public class VoidIcon : AbstractIconBase
{
	public override void Initialize() { }
	public override void Release() { }
}