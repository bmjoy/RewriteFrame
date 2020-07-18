using UnityEngine;

namespace Map
{
    public class PlayerInfo
    {
        public const float NOTSET_POSITION = int.MinValue;

        public bool IsRealWorldChange;
        public Vector3 RealWorldPosition;
        public bool IsGameWorldChange;
        public Vector3 GameWorldPosition;
        public bool IsRealWorld2GameWorldChange;
        public Vector3 RealWorld2GameWorld;

        public void Reset()
        {
            IsRealWorldChange = true;
            RealWorldPosition = new Vector3(NOTSET_POSITION, NOTSET_POSITION, NOTSET_POSITION);
            IsGameWorldChange = true;
            GameWorldPosition = new Vector3(NOTSET_POSITION, NOTSET_POSITION, NOTSET_POSITION);
            IsRealWorld2GameWorldChange = false;
            RealWorld2GameWorld = new Vector3(NOTSET_POSITION, NOTSET_POSITION, NOTSET_POSITION);
        }

        public override string ToString()
        {
            return $"RealWorldPosition:({RealWorldPosition.x:F2}, {RealWorldPosition.y:F2}, {RealWorldPosition.z:F2}) GameWorldPosition:({GameWorldPosition.x:F2}, {GameWorldPosition.y:F2}, {GameWorldPosition.z:F2})";
        }
    }
}