using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public float Speed { get; set; }
    public bool IsCrouching { get; set; }
    public bool IsGrounded { get; set; }

    Animator m_Animator;

	void Awake() => m_Animator = GetComponent<Animator>();

    // Update is called once per frame
    void Update()
    {
		m_Animator.SetFloat("Speed", Speed);
		m_Animator.SetBool("IsCrouching", IsCrouching);
		m_Animator.SetBool("IsGrounded", IsGrounded);
	}
}
