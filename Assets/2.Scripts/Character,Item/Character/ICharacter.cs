using UnityEngine;

// 케릭터 네임스페이스, 인터페이스
// 작성자 : 이상준
// 내용 : 게임에서 움직이는 케릭터(인간, 좀비)가 상속해서 재구현하는 인터페이스
// 마지막 수정일 : 2022.11.23
namespace Character
{
    public interface ICharacter
    {
        float RunSpd { get; set; }            // 이동 속도

        bool IsInvulerable { get; set; }      // 무적 상태

        void Run();                           // 달리기
        
        void TakeDamage(int _atkType, Vector3 BitePos ); // 피격 처리
    }
}