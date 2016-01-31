/// <summary>
/// 플레이어의 리스폰에 반응해야 하는 객체들의 인터페이스
/// LevelManager 에서 Start 시 거리에 따라 CheckPoint 객체에 AssignObjectToCheckPoint 메소드로 등록된다.
/// </summary>
namespace druggedcode.engine
{
    public interface IPlayerRespawnListener
    {
        void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, DECharacter player);
    }
}
