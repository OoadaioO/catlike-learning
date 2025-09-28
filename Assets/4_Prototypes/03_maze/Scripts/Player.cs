
using UnityEngine;

namespace maze {
    public class Player : MonoBehaviour {

        [Min(0f)]
        [SerializeField] private float movementSpeed = 4f, rotationSpeed = 10f, mouseSensitivity = 5f;

        [SerializeField] private float startingVerticalEyeAngle = 10f;

        private CharacterController characterController;
        private Transform eye;
        private Vector2 eyeAngles;

        private void Awake() {
            characterController = GetComponent<CharacterController>();
            eye = transform.GetChild(0);
        }

        public void StartNewGame(Vector3 position) {
            eyeAngles.x = Random.Range(0f, 360f);
            eyeAngles.y = startingVerticalEyeAngle;

            characterController.enabled = false;
            transform.localPosition = position;
            characterController.enabled = true;

        }

        public Vector3 Move() {

            UpdateEyeAngles();
            UpdatePosition();

            return transform.localPosition;
        }

        private void UpdatePosition() {
            // Vector2 movement = new Vector2(
            //     Input.GetAxis("Horizontal"),
            //     Input.GetAxis("Vertical")
            // );
            float movement = Input.GetAxis("Vertical");
            movement = Mathf.Clamp(movement, -1f, 1f);

            movement *= movementSpeed;

            // Vector2 forward = new Vector2(
            //     Mathf.Sin(eyeAngles.x * Mathf.Deg2Rad),
            //     Mathf.Cos(eyeAngles.x * Mathf.Deg2Rad)
            // );
            // Vector2 right = new Vector2(forward.y, -forward.x);
            // movement = right * movement.x + forward * movement.y;
            // characterController.SimpleMove(new Vector3(movement.x, 0f, movement.y));
            characterController.SimpleMove(eye.forward * movement);
        }

        private void UpdateEyeAngles() {

            float rotationDelta = rotationSpeed * Time.deltaTime;
            eyeAngles.x += rotationDelta * Input.GetAxis("Horizontal");
            //eyeAngles.y -= rotationDelta * Input.GetAxis("Vertical View");

            if (mouseSensitivity > 0f) {
                float mouseDelta = rotationDelta * mouseSensitivity;
                eyeAngles.x += mouseDelta * Input.GetAxis("Mouse X");
                eyeAngles.y -= mouseDelta * Input.GetAxis("Mouse Y");
            }

            if (eyeAngles.x > 360f) {
                eyeAngles.x -= 360f;
            } else if (eyeAngles.x < 0f) {
                eyeAngles.x += 360f;
            }
            eyeAngles.y = Mathf.Clamp(eyeAngles.y, -45f, 45f);
            eye.localRotation = Quaternion.Euler(eyeAngles.y, eyeAngles.x, 0f);

        }

    }

}