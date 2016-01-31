using UnityEngine;
using System.Collections;

public class AnimatedFXHelper : MonoBehaviour
{
    public bool destroyWhenFinished;
    public float autoDestructDelay;
    public bool setAnimationSpeed;
    public float animationSpeed;
    public Vector3 translate;
    public Space translateSpace;
    public Vector3 rotate;
    public Space rotateSpace;
    public Vector3 scale;

    public Vector2 surfaceCast;
    public Vector2 surfaceOffset;
    public LayerMask surfaceMask;


    public void PlaySound(string sound)
    {
        SoundPalette.PlaySound(sound, 1, 1, transform.position);
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    public void RemoveObject(Object obj)
    {
        Destroy(obj);
    }

    void Start()
    {
        if (setAnimationSpeed)
        {
            Animation animation = GetComponent<Animation>();
            if (animation)
            {
                foreach (AnimationState state in animation)
                {
                    state.speed = animationSpeed;
                }
            }
        }

        if (destroyWhenFinished) StartCoroutine(DestroyWhenFinished());

        if (autoDestructDelay > 0)
            Destroy(gameObject, autoDestructDelay);

        if (surfaceCast.sqrMagnitude > 0)
        {

            Vector3 dir = surfaceCast;
            dir.x *= Mathf.Sign(transform.localScale.x);
            dir.y *= Mathf.Sign(transform.localScale.y);
            dir = transform.TransformDirection(dir);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, surfaceCast.magnitude, surfaceMask);
            if (hit.collider != null)
            {
                transform.position = hit.point;
                transform.Translate(surfaceOffset);
            }
        }
    }

    void Update()
    {
        if (translate.sqrMagnitude > 0)
            transform.Translate(translate * Time.deltaTime, translateSpace);

        if (rotate.sqrMagnitude > 0)
            transform.Rotate(rotate * Time.deltaTime, rotateSpace);

        if (scale.sqrMagnitude > 0)
            transform.localScale += scale * Time.deltaTime;
    }

    IEnumerator DestroyWhenFinished()
    {
        yield return new WaitForSeconds(0.1f);
        Animation animation = GetComponent<Animation>();
        
        if( animation == null )
        {
            yield return new WaitForSeconds( 0.3f );
        }
        else
        {
            while (animation.isPlaying) yield return null;
        }
        
        Remove();
    }
}
