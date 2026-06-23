using UnityEngine;

namespace DiaToMas.Interaction
{
    public class WorldItemHoverHighlightView : MonoBehaviour
    {
        [SerializeField] private Color _hoverColor = new(1f, 0.78f, 0.34f, 1f);

        private Renderer[] _renderers;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnMouseEnter()
        {
            ApplyHighlight();
        }

        private void OnMouseExit()
        {
            ClearHighlight();
        }

        private void ApplyHighlight()
        {
            foreach (Renderer targetRenderer in _renderers)
            {
                targetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor("_Color", _hoverColor);
                _propertyBlock.SetColor("_BaseColor", _hoverColor);
                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void ClearHighlight()
        {
            foreach (Renderer targetRenderer in _renderers)
            {
                targetRenderer.SetPropertyBlock(null);
            }
        }
    }
}
