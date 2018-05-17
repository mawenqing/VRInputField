using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections;

namespace Qiyi.UI.InputField
{
	[RequireComponent (typeof(LayoutElement))]
	[RequireComponent (typeof(CanvasRenderer))]
	[RequireComponent (typeof(RectTransform))]
	public class DefaultCaret : MonoBehaviour, ICaret
	{
		private Mesh _mesh;
		private int _selectionAnchorIndex = 0;
		private int _index = 0;
        private bool _isVisible;

        private Coroutine _blinkCoroutine;

        public IInputFieldController InputFieldController { set; get; }

        private void Start()
        {
            GetComponent<LayoutElement>().ignoreLayout = true;
            AlignPosition(transform.parent.GetChild(0).GetComponent<RectTransform>(), GetComponent<RectTransform>());
			CaretRenderer.SetMaterial (Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
            gameObject.layer = transform.parent.gameObject.layer;
            transform.SetAsFirstSibling();
        }

        private void OnDisable()
        {
            DestroyCaret();
        }

        private void OnEnable()
        {
            _mesh = new Mesh();
            CaretRenderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
        }

        public CanvasRenderer CaretRenderer
        {
            get { return GetComponent<CanvasRenderer> (); }
		}

		public void ActivateCaret ()
		{
            _isVisible = true;
            _blinkCoroutine = StartCoroutine(CaretBlink());
        }

		public void DeactivateCaret ()
		{
            if (_blinkCoroutine != null) {
                StopCoroutine (_blinkCoroutine);
                _blinkCoroutine = null;
            }

            _isVisible = false;
            InputFieldController.MarkGeometryAsDirty();
		}

		public void DestroyCaret ()
		{
            CaretRenderer.Clear();
            DestroyImmediate(_mesh);
            _mesh = null;
		}

		private bool HasSelection ()
		{
			return _index != _selectionAnchorIndex;
		}

		public int GetIndex ()
		{
			return _index;
		}

		public void MoveTo (int index, bool withSelection)
		{
			_selectionAnchorIndex = withSelection ? _selectionAnchorIndex : index;
			_index = index;
		}

		private void SetupCursorVertsPositions (ref UIVertex[] verts, Rect drawRect)
		{
            Assert.IsNotNull (verts);
			Assert.IsTrue (verts.Length >= 4);

			verts [0].position = new Vector3 (drawRect.xMin, drawRect.yMin, 0.0f);
			verts [1].position = new Vector3 (drawRect.xMax, drawRect.yMin, 0.0f);
			verts [2].position = new Vector3 (drawRect.xMax, drawRect.yMax, 0.0f);
			verts [3].position = new Vector3 (drawRect.xMin, drawRect.yMax, 0.0f);
		}

		public bool IsVisible ()
		{
            return _isVisible;
		}

        public void Draw(Rect drawRect, Color color, VertexHelper helper)
        {
            if (!gameObject.activeInHierarchy) {
                return;
            }

            if (IsVisible())
            {
                GenerateCursorOrSelection(helper, drawRect, color);
            }
            helper.FillMesh(_mesh);
            CaretRenderer.SetMesh(_mesh);
        }

        private void GenerateCursorOrSelection(VertexHelper helper, Rect drawRect, Color color)
        {
            var verts = new UIVertex[4];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = UIVertex.simpleVert;
                verts[i].uv0 = Vector2.zero;
                verts[i].color = color;
            }

            SetupCursorVertsPositions(ref verts, drawRect);
            helper.AddUIVertexQuad(verts);
        }

        private void AlignPosition(RectTransform textTransform, RectTransform caretTransform)
        {
            Assert.IsNotNull(textTransform);
            Assert.IsNotNull(caretTransform);

            caretTransform.localPosition = textTransform.localPosition;
            caretTransform.localRotation = textTransform.localRotation;
            caretTransform.localScale = textTransform.localScale;
            caretTransform.anchorMin = textTransform.anchorMin;
            caretTransform.anchorMax = textTransform.anchorMax;
            caretTransform.anchoredPosition = textTransform.anchoredPosition;
            caretTransform.sizeDelta = textTransform.sizeDelta;
            caretTransform.pivot = textTransform.pivot;
        }

        private IEnumerator CaretBlink()
        {
            int timer = 0;
            while (true)
            {
                if (!HasSelection())
                {
                    _isVisible = Mathf.Sin(timer++ * 0.08f) < 0;
                    InputFieldController.MarkGeometryAsDirty();
                }
                else
                {
                    _isVisible = true;
                }
                yield return null;
            }
        }

        public int GetSelectionIndex()
        {
            return _selectionAnchorIndex;
        }
    }
}

