using UnityEditor;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
	[SerializeField]
	Transform m_PointA;
	[SerializeField]
	Transform m_PointB;
	[SerializeField, Tooltip("How the platform moves between the two points. Only allows 2 keys (1 for each point), forces the value of key[0] to be 0 (pointA) and forces the value of key[1] to be 1 (pointB)")]
	AnimationCurve m_MotionCurve;

	float m_CurrentTime = 0f;
	bool m_bMoveAToB = true;

	void EnsureValidCurve()
	{
		m_MotionCurve ??= new AnimationCurve();
		if (m_MotionCurve.length == 0 || m_MotionCurve.length > 2) CreateLinearMotion();
		if (m_MotionCurve[0].value != 0f) m_MotionCurve.keys[0].value = 0f;
		if (m_MotionCurve[1].value != 1f) m_MotionCurve.keys[1].value = 1f;
	}

	void CreateLinearMotion()
	{
		m_MotionCurve.keys = new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
		AnimationUtility.SetKeyLeftTangentMode(m_MotionCurve, 0, AnimationUtility.TangentMode.Auto);
		AnimationUtility.SetKeyLeftTangentMode(m_MotionCurve, 1, AnimationUtility.TangentMode.Auto);
	}

	void OnValidate() => EnsureValidCurve();

	void OnEnable() => EnsureValidCurve();

	void Awake()
	{
		transform.position = m_PointA.position;
		EnsureValidCurve();
	}

    void Update()
    {
		m_CurrentTime += Time.deltaTime;
		float minTime = m_MotionCurve[0].time;
		float minValue = m_MotionCurve[0].value;
		float maxTime = m_MotionCurve[1].time;
		float maxValue = m_MotionCurve[1].value;
		float clampedCurrentTime = Mathf.Clamp(m_CurrentTime, minTime, maxTime);
		float currentPoint = m_MotionCurve.Evaluate(clampedCurrentTime);
		float currentPoint01 = Mathf.Clamp01((currentPoint - minValue) / (maxValue - minValue));
		Vector3 currentPosition = Vector3.Lerp(m_bMoveAToB ? m_PointA.position : m_PointB.position, m_bMoveAToB ? m_PointB.position : m_PointA.position, currentPoint01);
		transform.position = currentPosition;
		if (clampedCurrentTime < maxTime) return;
		m_bMoveAToB = !m_bMoveAToB;
		m_CurrentTime = 0f;
	}
}
