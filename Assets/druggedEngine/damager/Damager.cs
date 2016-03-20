using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 오브젝트에 붙이면 충돌했을때 대미지를 준다.
    /// </summary>
	public class Damager : MonoBehaviour
    {
		public GameObject impactPrefab;
		public AudioClip impactSound;

        public int damage = 10;

		public LayerMask collisionMask;

		protected Transform mTr;
		protected GameObject mOwner;
		public GameObject Owner { get{ return mOwner; }}

		virtual protected void Awake()
		{
			mTr = transform;
			mOwner = this.gameObject;
		}

		virtual protected void Start()
		{

		}

        /// <summary>
        /// 캐릭터가 충돌하면 데미지를 주고 플레이어를 넉백 시킨다.
        /// </summary>
        public void OnTriggerEnter2D(Collider2D collider)
        {
			if (LayerUtil.Contains(collisionMask, collider.gameObject.layer) == false)
			{
				OnNotCollideWith(collider);
				return;
			}

			if( collider.gameObject == Owner )
			{
				OnCollideOwner();
				return;
			}

			IDamageable damageable = collider.GetComponent<IDamageable>();
			if (damageable != null)
			{
				OnCollideTakeDamage( collider, damageable );
				return;
			}

			OnCollideOther(collider);
        }

		/// <summary>
		/// 지정한 레이어가 아닌 다른 충돌체와 충돌.
		/// </summary>
		protected virtual void OnNotCollideWith(Collider2D collider)
		{
			//do nothing
		}

		/// <summary>
		/// 발사한 주인과 충돌( 여기를 이용해 부메랑등의 탄환을 구현 )
		/// </summary>
		protected virtual void OnCollideOwner()
		{
			//do nothing
		}

		/// <summary>
		/// 데미지를 입을 수 있는 객체와 충돌
		/// </summary>
		protected virtual void OnCollideTakeDamage( Collider2D collider, IDamageable damageable )
		{
			damageable.Hit( damage );

			float xforce = collider.transform.position.x > mTr.transform.position.x ? 1: -1;
			xforce *= Random.Range(3f,5f);

			float yforce = collider.transform.position.y >= mTr.transform.position.y ? 1: -1;
			yforce *= Random.Range(1f,2f);

			Vector2 force = new Vector2(xforce, yforce);
			HitData hitdata = new HitData( damage, force );

			damageable.Hit( hitdata );

			Impact( collider.transform.position, false );
		}

		/// <summary>
		/// 충돌은 허용했지만 데미지를 입을 수 없는 충돌체와 충돌
		/// </summary>
		protected virtual void OnCollideOther(Collider2D collider)
		{
			Impact( collider.transform.position );
		}

		protected void Impact( Vector3 point, bool destroy = true )
		{
			FXManager.Instance.SpawnFX( impactPrefab,point, mTr.rotation );
			SoundManager.Instance.PlaySFX( impactSound,1,1, point );

			if( destroy ) Destroy(gameObject);
		}
    }
}