using UnityEngine;

public class StampAnimation : MonoBehaviour
{
    void Start()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(0); // 0 = デフォルトステート
        }
    }
}
