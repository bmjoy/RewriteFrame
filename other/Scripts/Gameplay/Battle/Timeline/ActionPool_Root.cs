using Eternity.FlatBuffer.Enums;

namespace Gameplay.Battle.Timeline
{
    public delegate IActionPool CreateActionPool(ActionID actionID);

    public static partial class ActionPool
    {
        public static CreateActionPool Creater { get; set; }
    }
}
