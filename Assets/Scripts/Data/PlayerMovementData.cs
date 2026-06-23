using System;

namespace DiaToMas.Data
{
    [Serializable]
    public class PlayerMovementData : GameDataBase
    {
        public float moveSpeed;
        public float runMultiplier;
        public float groundCheckRadius;
    }
}
