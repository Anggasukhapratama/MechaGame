using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;          // Player
    public float smoothTime = 0.3f;   // Waktu transisi smooth
    public Vector2 offset = new Vector2(2f, 1f);  // Offset dari player
    public float lookAheadDistance = 2f;          // Seberapa jauh kamera mengintip ke depan
    public float moveThreshold = 0.1f;            // Batas perubahan gerakan agar kamera ikut

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastTargetPosition;
    private Vector3 lookAheadPos;

    void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Hitung perubahan posisi target
        float xMoveDelta = target.position.x - lastTargetPosition.x;

        // Tambahkan look-ahead jika bergerak cukup jauh
        if (Mathf.Abs(xMoveDelta) > moveThreshold)
        {
            lookAheadPos = new Vector3(lookAheadDistance * Mathf.Sign(xMoveDelta), 0, 0);
        }
        else
        {
            // Smoothly reset lookahead jika player diam
            lookAheadPos = Vector3.Lerp(lookAheadPos, Vector3.zero, Time.deltaTime * 2f);
        }

        // Posisi kamera yang diinginkan
        Vector3 targetPos = target.position + new Vector3(offset.x, offset.y, -10f) + lookAheadPos;

        // Smooth follow ke posisi target
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        lastTargetPosition = target.position;
    }
}
