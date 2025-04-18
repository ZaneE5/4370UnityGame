using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

//Credit to rollaball tutorial for basic movement code

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private float movementX;
    private float movementY;
    private float currSpeed;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private GameObject cam;

    public UIDocument uiDocument;
    private ProgressBar healthbar;
    private Label warningText;
    private bool warningEnabled;
    private float warningHideTime = 100;
    private Label coinCounter;
    public float maxHP = 100;
    private float currHP;
    private int coinCount = 0;
    private int coinsLost = 0;

    private float fps;
    private float maxIframes;
    private float currIframes;

    private PlayerInput playerInput;
    private InputAction sprintAction;

    void Start() {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MazeScene"));

        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

        healthbar = uiDocument.rootVisualElement.Query<ProgressBar>().First();
        coinCounter = uiDocument.rootVisualElement.Query<Label>().First();
        warningText = uiDocument.rootVisualElement.Query<Label>().Name("warningText");
        warningText.style.display = DisplayStyle.None;
        coinCount = 0;
        coinsLost = 0;
        UpdateCoinCounter();
        currHP = maxHP;
        UpdateHealthBar();

        fps = 1.0f / Time.deltaTime;
        Debug.Log("Stored FPS as " + fps);
        maxIframes = fps * 2;
        Debug.Log("Maximum Iframes: " + maxIframes);
        currIframes = maxIframes;

        playerInput = GetComponent<PlayerInput>();
        sprintAction = playerInput.actions["Sprint"];
        sprintSpeed = 2 * movementSpeed;
        currSpeed = movementSpeed;

        Camera.SetupCurrent(GetComponent<Camera>());
    }

    void OnMove(InputValue movementValue) {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void FixedUpdate() {
        if (sprintAction.IsPressed()) {
            currSpeed = sprintSpeed;
        } else {
            currSpeed = movementSpeed;
        }
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);

        Vector3 cameraForward = cam.transform.forward;
        Vector3 cameraRight = cam.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 desiredDirection = cameraForward * movement.z + cameraRight * movement.x;
        Vector3 newPos = rb.position + desiredDirection * currSpeed;
        rb.MovePosition(newPos);

        rb.rotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);

        warningHideTime--;
        if (warningHideTime <= 0) {
            if (warningEnabled) {
                warningText.style.display = DisplayStyle.None;
            }
        } 
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Enemy")) {
            GetHit();
        }
        if(collision.gameObject.CompareTag("Exit")) {
            if (coinCount + coinsLost < 22) {
                warningText.style.display = DisplayStyle.Flex;
                warningEnabled = true;
                warningHideTime = 400;
            } else {
                SceneManager.LoadScene("Connect4");
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Stalker")) {
            if (currIframes <= 0) {
                GetHit();
                currIframes = maxIframes;
            }
            else {
                currIframes -= 1;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Coin")) {
            other.gameObject.SetActive(false);
            coinCount = coinCount + 1;
            UpdateCoinCounter();
        }
    }

    private void GetHit() {
        if (coinCount > 0) {
            coinCount -= 1;
            coinsLost += 1;
            UpdateCoinCounter();
        } else {
            currHP -= 10;
            UpdateHealthBar();
            if (currHP == 0) {
                Lose();
            }
        }
    }

    private void Lose() {
        warningText.text = "You Lose!";
        warningText.style.display = DisplayStyle.Flex;
        Destroy(gameObject);
    }

    private void UpdateHealthBar() {
        if (healthbar != null) {
            healthbar.value = currHP / maxHP * 100;
        }
        else {
            Debug.Log("Health bar not found");
        }
    }

    private void UpdateCoinCounter() {
        if (coinCounter != null) {
            coinCounter.text = "Count: " + (coinCount + coinsLost) + "/22";
        }
        else {
            Debug.Log("Coin Counter not found");
        }
    }
}