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
		if (m_MotionCurve[0].value != 0f || m_MotionCurve[1].value != 1f) ClampCurveValues();
	}

	void CreateLinearMotion()
	{
		m_MotionCurve.keys = new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
		AnimationUtility.SetKeyLeftTangentMode(m_MotionCurve, 0, AnimationUtility.TangentMode.Linear);
		AnimationUtility.SetKeyRightTangentMode(m_MotionCurve, 0, AnimationUtility.TangentMode.Linear);
		AnimationUtility.SetKeyLeftTangentMode(m_MotionCurve, 1, AnimationUtility.TangentMode.Linear);
		AnimationUtility.SetKeyRightTangentMode(m_MotionCurve, 1, AnimationUtility.TangentMode.Linear);
	}

	void ClampCurveValues()
	{
		AnimationUtility.TangentMode k0L = AnimationUtility.GetKeyLeftTangentMode(m_MotionCurve, 0);
		AnimationUtility.TangentMode k0R = AnimationUtility.GetKeyRightTangentMode(m_MotionCurve, 0);
		AnimationUtility.TangentMode k1L = AnimationUtility.GetKeyLeftTangentMode(m_MotionCurve, 1);
		AnimationUtility.TangentMode k1R = AnimationUtility.GetKeyRightTangentMode(m_MotionCurve, 1);
		m_MotionCurve.keys = new Keyframe[] { new Keyframe(m_MotionCurve[0].time, 0f), new Keyframe(m_MotionCurve[1].time, 1f) };
		AnimationUtility.SetKeyLeftTangentMode(m_MotionCurve, 0, k0L);
		AnimationUtility.SetKeyRightTangentMode(m_MotionCurve, 0, k0R);
		AnimationUtility.SetKeyLeftTangentMode(m_MotionCurve, 1, k1L);
		AnimationUtility.SetKeyRightTangentMode(m_MotionCurve, 1, k1R);
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
