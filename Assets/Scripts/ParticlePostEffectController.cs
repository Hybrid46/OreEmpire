using UnityEngine;

public class ParticlePostEffectController : MonoBehaviour
{
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotation = Quaternion.identity;

    private Vector3 motionPosition = Vector3.zero;
    private Vector3 motionRotation = Vector3.zero;

    [SerializeField] private Vector2 lastMotionVector = Vector2.zero;
    [SerializeField] private Vector2 motionVector = Vector2.zero;

    private Transform cameraTransform;
    private Transform m_Transform;
    public bool debugData = false;
    public Material particlePostEffectMaterial;

    private int motionVectorID;

    public void Start()
    {
        if (debugData) m_Transform = transform;
        UpdateCameraTransform();
        UpdateLastData();

        motionVectorID = Shader.PropertyToID("_MotionVector");
    }

    public void Update()
    {
        //uncomment if camera changes runtime 
        //UpdateCameraTransform();
        UpdateMotionData();
        UpdateLastData();
        if (debugData) DebugMotionData();
        UpdateShaderVariables();
    }

    public void UpdateCameraTransform()
    {
        cameraTransform = Camera.main.transform;
    }

    private void UpdateLastData()
    {
        lastPosition = cameraTransform.position;
        lastRotation = cameraTransform.rotation;
    }

    private void UpdateMotionData()
    {
        motionPosition = cameraTransform.position - lastPosition;
        //Quaternion difference
        //motionRotation = cameraTransform.rotation.Diff(lastRotation).eulerAngles;

        //Euler difference
        float signedAngleDiffX = Vector3.SignedAngle(lastRotation.eulerAngles, cameraTransform.rotation.eulerAngles, Vector3.right);
        float signedAngleDiffY = Vector3.SignedAngle(lastRotation.eulerAngles, cameraTransform.rotation.eulerAngles, Vector3.up);
        float signedAngleDiffZ = Vector3.SignedAngle(lastRotation.eulerAngles, cameraTransform.rotation.eulerAngles, Vector3.forward);
        Vector3 signedRotationDifference = new Vector3(signedAngleDiffX, signedAngleDiffY, signedAngleDiffZ);

        motionRotation = signedRotationDifference;

        float motionX = signedAngleDiffX + motionPosition.x;

        float motionY = signedAngleDiffY + signedAngleDiffZ +
                        motionPosition.y + motionPosition.z;

        //Truncate -> too high precision
        motionX = Mathf.RoundToInt(motionX * 10000f) / 10000f;
        motionY = Mathf.RoundToInt(motionY * 10000f) / 10000f;

        lastMotionVector.Set(motionX, motionY);
        motionVector += lastMotionVector;
    }

    private void DebugMotionData()
    {
        m_Transform.position += motionPosition;
        m_Transform.rotation.Add(Quaternion.Euler(motionRotation));
    }

    private void UpdateShaderVariables()
    {
        Vector4 expandedMotionVector = new Vector4(motionVector.x, motionVector.y, 0f, 0f);

        particlePostEffectMaterial.SetVector(motionVectorID, expandedMotionVector);
    }
}
