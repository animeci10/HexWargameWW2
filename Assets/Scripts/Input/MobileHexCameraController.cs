using HexWargame.Core;
using UnityEngine;
using System;

namespace HexWargame.InputControl
{
    /// <summary>
    /// Android dokunmatik ekranlar için optimize edilmiş 2.5D İzometrik Kamera Kontrolcüsü.
    /// Tek parmak: Haritayı kaydırma (Pan)
    /// İki parmak: Yakınlaşma / Uzaklaşma (Pinch-to-zoom)
    /// Dokunma (Tap): Hex seçimi
    /// </summary>
    public class MobileHexCameraController : MonoBehaviour
    {
        [Header("Kamera Ayarları")]
        [SerializeField] private Camera cam;
        [SerializeField] private float panSpeed = 0.015f;
        [SerializeField] private float zoomSpeed = 0.05f;
        [SerializeField] private float minOrthoSize = 4f;
        [SerializeField] private float maxOrthoSize = 18f;

        [Header("Harita Sınırları")]
        [SerializeField] private Vector2 minBounds = new Vector2(-20f, -20f);
        [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

        [Header("Hex Parametresi")]
        [SerializeField] private float hexRadius = 1.0f;

        public event Action<HexCoordinates> OnHexTapped;

        private Vector2 lastTouchPos;
        private bool isDragging = false;
        private float tapStartTime;
        private const float MAX_TAP_DURATION = 0.25f;
        private const float MAX_TAP_MOVEMENT = 15f;
        private Vector2 tapStartPos;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            HandleTouchInput();
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#endif
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    lastTouchPos = touch.position;
                    tapStartPos = touch.position;
                    tapStartTime = Time.time;
                    isDragging = true;
                }
                else if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    Vector2 delta = touch.deltaPosition;
                    Vector3 move = new Vector3(-delta.x, 0, -delta.y) * panSpeed * (cam.orthographicSize / 5f);
                    transform.position = ClampPosition(transform.position + move);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    isDragging = false;
                    float duration = Time.time - tapStartTime;
                    float dist = Vector2.Distance(touch.position, tapStartPos);

                    if (duration < MAX_TAP_DURATION && dist < MAX_TAP_MOVEMENT)
                    {
                        ProcessTap(touch.position);
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                // Pinch to Zoom
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 prevPos0 = t0.position - t0.deltaPosition;
                Vector2 prevPos1 = t1.position - t1.deltaPosition;

                float prevDistance = Vector2.Distance(prevPos0, prevPos1);
                float currentDistance = Vector2.Distance(t0.position, t1.position);

                float deltaDistance = currentDistance - prevDistance;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - deltaDistance * zoomSpeed, minOrthoSize, maxOrthoSize);
            }
        }

        private void HandleMouseInput()
        {
            // Editörde ve PC'de test için fare desteği
            if (Input.GetMouseButtonDown(0))
            {
                tapStartPos = Input.mousePosition;
                tapStartTime = Time.time;
            }

            if (Input.GetMouseButton(0))
            {
                float h = -Input.GetAxis("Mouse X") * panSpeed * 30f;
                float v = -Input.GetAxis("Mouse Y") * panSpeed * 30f;
                transform.position = ClampPosition(transform.position + new Vector3(h, 0, v));
            }

            if (Input.GetMouseButtonUp(0))
            {
                float duration = Time.time - tapStartTime;
                float dist = Vector2.Distance(Input.mousePosition, tapStartPos);
                if (duration < MAX_TAP_DURATION && dist < MAX_TAP_MOVEMENT)
                {
                    ProcessTap(Input.mousePosition);
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * 5f, minOrthoSize, maxOrthoSize);
            }
        }

        private void ProcessTap(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                HexCoordinates hex = HexCoordinates.FromWorldPosition(hitPoint, hexRadius);
                OnHexTapped?.Invoke(hex);
            }
        }

        private Vector3 ClampPosition(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.z = Mathf.Clamp(pos.z, minBounds.y, maxBounds.y);
            return pos;
        }
    }
}
