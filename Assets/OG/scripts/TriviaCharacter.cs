using UnityEngine;
using UnityEngine.UIElements;

public class TriviaCharacter : MonoBehaviour
{
    public enum AnimationType { Float, Pendulum }

    [Header("Character Sprite")]
    [SerializeField] public Sprite sprite;

    [Header("Position & Size (pixels at 1080x1920 reference)")]
    [SerializeField] private float _left   = 350f;
    [SerializeField] private float _top    = 850f;
    [SerializeField] private float _width  = 600f;
    [SerializeField] private float _height = 800f;

    [Header("Animation Type")]
    [SerializeField] private AnimationType _animationType = AnimationType.Float;

    [Header("Float Settings")]
    [SerializeField] private float _amplitude   = 25f;
    [SerializeField] private float _speed       = 1.8f;
    [SerializeField] private float _phaseOffset = 0f;

    [Header("Pendulum Settings")]
    [SerializeField] private float _pendulumAngle = 30f;
    [SerializeField] private float _pendulumSpeed = 1.5f;

    private VisualElement _element;
    private float         _time;
    private bool          _active;

    public void SetElement(VisualElement el)
    {
        _element = el;
        if (_element == null) return;

        _element.style.left   = _left;
        _element.style.top    = _top;
        _element.style.width  = _width;
        _element.style.height = _height;

        // Pendulum pivots from top-right corner
        if (_animationType == AnimationType.Pendulum)
        {
            _element.style.transformOrigin = new StyleTransformOrigin(
                new TransformOrigin(new Length(100, LengthUnit.Percent),
                                    new Length(0,   LengthUnit.Percent), 0f));
        }
    }

    public void Show()
    {
        if (_element == null) return;
        _time   = _phaseOffset;
        _active = true;
        _element.style.display   = DisplayStyle.Flex;
        _element.style.translate = new StyleTranslate(new Translate(0, 0));
        _element.style.rotate    = new StyleRotate(new Rotate(0));
    }

    public void Hide()
    {
        if (_element == null) return;
        _active = false;
        _element.style.display = DisplayStyle.None;
    }

    public void Tick(float deltaTime)
    {
        if (!_active || _element == null) return;
        _time += deltaTime;

        switch (_animationType)
        {
            case AnimationType.Float:
                float offsetY = Mathf.Sin(_time * _speed + _phaseOffset) * _amplitude;
                // UI Toolkit Y goes downward, so negate to match the "float up" feeling
                _element.style.translate = new StyleTranslate(new Translate(0, -offsetY));
                break;

            case AnimationType.Pendulum:
                float angle = Mathf.Sin(_time * _pendulumSpeed) * _pendulumAngle;
                _element.style.rotate = new StyleRotate(new Rotate(angle));
                break;
        }
    }
}
