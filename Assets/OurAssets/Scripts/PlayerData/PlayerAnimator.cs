using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
	[SerializeField, Range(0f, 1f), Tooltip("Adds smoothness between animation states in blend tree")]
	float m_DampTime = 0.1f;

	public float Speed { get; set; }
	public bool IsCrouching { get; set; }
	public bool IsGrounded { get; set; }

    Animator m_Animator;

	void Awake() => m_Animator = GetComponent<Animator>();

	void Start() => m_Animator.SetBool("IsGrounded", true);

	void Update()
    {
		m_Animator.SetFloat("Speed", Speed, m_DampTime, Time.deltaTime);
		m_Animator.SetBool("IsCrouching", IsCrouching);
		m_Animator.SetBool("IsGrounded", IsGrounded);
	}
}
