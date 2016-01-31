using UnityEngine;

namespace druggedcode.engine
{
    public class DECharacterState
    {
        public bool CanWallClinging { get; set; }
        //conditions
        public bool TriggerDead { get; set; }

        //현재상황
        public bool IsRun { get; set; }
        public bool IsLadderClimb { get; set; }
        public bool IsDead { get; set; }

        //jump
        public int JumpCount { get; set; }
        public int JumpLeft { get; set; }
        public float JumpLatestTime { get; set; }
        public float JumpElapsedTime { get { return Time.time - JumpLatestTime; } }

        public DECharacterState()
        {
            Reset();
        }
        public void Reset()
        {
            TriggerDead = false;
        }
    }
}
