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
		SetKeyTangentLinear(m_MotionCurve, 0);
		SetKeyTangentLinear(m_MotionCurve, 1);
	}

	void ClampCurveValues()
	{
		(float, float) k0 = GetKeyTangent(m_MotionCurve, 0);
		(float, float) k1 = GetKeyTangent(m_MotionCurve, 1);
		m_MotionCurve.keys = new Keyframe[] { new Keyframe(m_MotionCurve[0].time, 0f), new Keyframe(m_MotionCurve[1].time, 1f) };
		SetKeyTangent(m_MotionCurve, 0, k0);
		SetKeyTangent(m_MotionCurve, 1, k1);
	}

	void SetKeyTangentLinear(AnimationCurve animationCurve, int index)
	{
		Keyframe[] keys = animationCurve.keys;
		float inTangent = index > 0 ? ((keys[index].value - keys[index - 1].value) / (keys[index].time - keys[index - 1].time)) : 0f;
		float outTangent = index < keys.Length - 1 ? ((keys[index + 1].value - keys[index].value) / (keys[index + 1].time - keys[index].time)) : 0f;
		keys[index].inTangent = inTangent;
		keys[index].outTangent = outTangent;
		animationCurve.keys = keys;
	}

	(float, float) GetKeyTangent(AnimationCurve animationCurve, int index) => (animationCurve[index].inTangent, animationCurve[index].outTangent);

	void SetKeyTangent(AnimationCurve animationCurve, int index, (float inTangent, float outTangent) tangent)
	{
		animationCurve.keys[index].inTangent = tangent.inTangent;
		animationCurve.keys[index].outTangent = tangent.outTangent;
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
