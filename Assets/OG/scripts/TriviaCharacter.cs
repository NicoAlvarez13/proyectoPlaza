using UnityEngine;

public class TriviaCharacter : MonoBehaviour
{
    public enum AnimationType { Float, Pendulum }

    [Header("Animation Type")]
    [SerializeField] private AnimationType _animationType = AnimationType.Float;

    [Header("Float Settings")]
    [SerializeField] private float _amplitude   = 25f;
    [SerializeField] private float _speed       = 1.8f;
    [SerializeField] private float _phaseOffset = 0f;

    [Header("Pendulum Settings")]
    [SerializeField] private float _pendulumAngle = 30f;  // grados máximos de oscilación
    [SerializeField] private float _pendulumSpeed = 1.5f;

    private RectTransform _rect;
    private Vector2       _originPos;
    private float         _time;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (_rect != null)
        {
            _originPos = _rect.anchoredPosition;

            if (_animationType == AnimationType.Pendulum)
            {
                // Pivot a esquina superior derecha para que el ancla sea ese punto
                SetPivot(new Vector2(1f, 1f));
            }
        }

        _time = _phaseOffset;
    }

    private void OnDisable()
    {
        if (_rect == null) return;

        _rect.anchoredPosition = _originPos;
        _rect.localRotation    = Quaternion.identity;

        if (_animationType == AnimationType.Pendulum)
            SetPivot(new Vector2(0.5f, 0.5f));
    }

    private void Update()
    {
        if (_rect == null) return;

        _time += Time.deltaTime;

        switch (_animationType)
        {
            case AnimationType.Float:
                float offsetY = Mathf.Sin(_time * _speed + _phaseOffset) * _amplitude;
                _rect.anchoredPosition = new Vector2(_originPos.x, _originPos.y + offsetY);
                break;

            case AnimationType.Pendulum:
                float angle = Mathf.Sin(_time * _pendulumSpeed) * _pendulumAngle;
                _rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                break;
        }
    }

    /// <summary>
    /// Cambia el pivot sin desplazar visualmente el elemento.
    /// </summary>
    private void SetPivot(Vector2 newPivot)
    {
        Vector2 size      = _rect.rect.size;
        Vector2 deltaPivot = newPivot - _rect.pivot;
        Vector3 deltaPos  = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y, 0f);
        _rect.pivot            = newPivot;
        _rect.anchoredPosition += (Vector2)deltaPos;
        _originPos             = _rect.anchoredPosition;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
