using UnityEngine;

public class CharacterSwitchManager : MonoBehaviour
{
    public GameObject characterA; // Piki (rápida)
    public GameObject characterB; // Heavy (fuerte)
    public Camera mainCamera;
    public KeyCode switchKey = KeyCode.Tab;
    private GameObject active;
    private GameObject inactive;

    void Start()
    {
        active = characterA;
        inactive = characterB;
        SetActiveCharacter(active);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            Swap();
        }
    }

    void Swap()
    {
        var tmp = active;
        active = inactive;
        inactive = tmp;
        SetActiveCharacter(active);
    }

    void SetActiveCharacter(GameObject go)
    {
        // Enable PlayerLocal on active, disable on inactive
        characterA.GetComponent<PlayerLocal>().enabled = false;
        characterB.GetComponent<PlayerLocal>().enabled = false;

        go.GetComponent<PlayerLocal>().enabled = true;

        // Opcional: activar/desactivar CharacterController collisions o Rigidbody
        // Ajustar la cámara para seguir el personaje activo
        if (mainCamera != null)
        {
            mainCamera.transform.SetParent(go.transform);
            mainCamera.transform.localPosition = new Vector3(0, 2.5f, -4f); // ajustar offset
            mainCamera.transform.localEulerAngles = Vector3.zero;
        }
    }
}
