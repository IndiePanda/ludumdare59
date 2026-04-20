using UnityEngine;
using Zenject;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private Transform _characterMesh;
    private float _moveSpeedCalculated;

    private TriggerPopupHandler _triggerPopupHandler;
    private EnergySystem _energySystem;

    [Inject]
    private void Construct(
        TriggerPopupHandler triggerPopupHandler,
        EnergySystem energySystem
        )
    {
        _triggerPopupHandler = triggerPopupHandler;
        _energySystem = energySystem;
        _energySystem.ChangeEnergy += OnEnergyChanged;
    }

    private void OnEnergyChanged(int value)
    {
        Debug.Log(value);
        _moveSpeedCalculated = _moveSpeed - (4 - value) * 0.1f;
    }

    private void Awake()
    {
        if (_characterMesh == null && transform.childCount > 0)
        {
            _characterMesh = transform.GetChild(0);
        }
    }

    private void Update()
    {
        if (_triggerPopupHandler != null && _triggerPopupHandler.IsPopupOpen)
        {
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        var input = new Vector3(horizontal, 0f, vertical);
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        transform.position += input * (_moveSpeedCalculated * Time.deltaTime);


        if (_characterMesh != null && input.sqrMagnitude > 0.0001f)
        {
            var targetRotation = Quaternion.LookRotation(input, Vector3.up);
            _characterMesh.rotation = Quaternion.Lerp(_characterMesh.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }

    }

}
