using SODynamics.Demo;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace InterpolationCurves.Second_Order_Dynamics
{
    [CustomEditor(typeof(SecondOrderDemo))]
    public class SecondOrderDemoInspector : Editor
    {
        private readonly float _defaultLenght = 2.0f;
        private readonly float _defaultValue = 1.0f;

        private readonly float _paddingLeft = 10f;
        private readonly float _paddingRight = 2f;
        private readonly float _paddingTop = 15f;
        private readonly float _paddingBottom = 15f;

        private readonly int _evaluationSteps = 300;

        private float _f, _f0, _z, _z0, _r, _r0;

        private SecondOrderDynamics _func;

        private Material _mat;

        private EvaluationData _evalData;

        private void OnEnable()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            _mat = new Material(shader);

            _evalData = new();

            InitFunction();
        }

        private void OnDisable()
        {
            _func = null;
            _evalData = null;

            _f = _f0 = _z = _z0 = _r = _r0 = float.NaN;

            DestroyImmediate(_mat);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UpdateInput();

            var rect = GUILayoutUtility.GetRect(10, 1000, 200, 200);
            
            if (Event.current.type != EventType.Repaint) return;
            
            GUI.BeginClip(rect);
            GL.PushMatrix();

            GL.Clear(true, false, Color.black);
            _mat.SetPass(0);

            var rectWidth = rect.width - _paddingLeft - _paddingRight;
            var rectHeight = rect.height - _paddingTop - _paddingBottom;

            var xAxisOffset = rectHeight * math.remap(_evalData.Y_min, _evalData.Y_max, 0, 1, 0);
            var defaultValueOffset = rectHeight * math.remap(_evalData.Y_min, _evalData.Y_max, 0, 1, 1);

            // draw base graph
            GL.Begin(GL.LINES);
            GL.Color(new Color(1, 1, 1, 1));
            // draw Y axis
            GL.Vertex3(_paddingLeft, _paddingTop, 0);
            GL.Vertex3(_paddingLeft, rect.height - _paddingBottom, 0);
            // draw X axis
            GL.Vertex3(_paddingLeft, rect.height - xAxisOffset - _paddingBottom, 0);
            GL.Vertex3(rect.width - _paddingRight, rect.height - xAxisOffset - _paddingBottom, 0);
            // draw default values
            GL.Color(Color.green);
            GL.Vertex3(_paddingLeft, rect.height - defaultValueOffset - _paddingBottom, 0);
            GL.Vertex3(rect.width - _paddingRight, rect.height - defaultValueOffset - _paddingBottom, 0);
            GL.End();

            // evaluate func values
            if (_evalData.IsEmpty) EvaluateFunction();

            // re-evaluate func values after input values changed
            if (!_f.Equals(_f0) || !_z.Equals(_z0) || !_r.Equals(_r0))
            {
                InitFunction();
                EvaluateFunction();
            }

            // draw graph
            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.cyan);
            for (var i = 0; i < _evalData.Length; i++)
            {
                var point = _evalData.GetItem(i);

                var xRemap = math.remap(_evalData.X_min, _evalData.X_max, 0, rectWidth, point.x);
                var yRemap = math.remap(_evalData.Y_min, _evalData.Y_max, 0, rectHeight, point.y);

                GL.Vertex3(_paddingLeft + xRemap, rect.height - yRemap - _paddingBottom, 0.0f);
            }

            GL.End();

            GL.PopMatrix();
            GUI.EndClip();

            // draw values
            const float squareSize = 10;
            EditorGUI.LabelField(
                new Rect(rect.x + _paddingLeft - squareSize,
                    rect.y + rect.height - defaultValueOffset - _paddingBottom - squareSize / 2, squareSize,
                    squareSize), "1"); // height "1" mark
            EditorGUI.LabelField(
                new Rect(rect.x + _paddingLeft - squareSize,
                    rect.y + rect.height - xAxisOffset - _paddingBottom + (squareSize * 0.2f), squareSize,
                    squareSize), "0"); // height "0" mark
            EditorGUI.LabelField(
                new Rect(rect.x + rect.width - _paddingRight - squareSize,
                    rect.y + rect.height - xAxisOffset - _paddingBottom + (squareSize * 0.2f), squareSize,
                    squareSize), "2"); // max lenght mark
        }

        private void UpdateInput()
        {
            _f = ((SecondOrderDemo)target).f;
            _z = ((SecondOrderDemo)target).z;
            _r = ((SecondOrderDemo)target).r;
        }

        private void InitFunction()
        {
            _f0 = _f = ((SecondOrderDemo)target).f;
            _z0 = _z = ((SecondOrderDemo)target).z;
            _r0 = _r = ((SecondOrderDemo)target).r;

            _func = new SecondOrderDynamics(_f, _z, _r, new Vector3(-_defaultLenght, 0, 0));
        }

        private void EvaluateFunction()
        {
            _evalData.Clear();

            for (var i = 0; i < _evaluationSteps; i++)
            {
                const float T = 0.016f; // constant deltaTime (60 frames per second)

                // input step function params
                var xInput = math.remap(0, _evaluationSteps - 1, -_defaultLenght, _defaultLenght, i);
                var yInput = xInput > 0 ? _defaultValue : 0;

                var funcValues = _func.Update(T, new Vector3(xInput, yInput, 0));

                if (xInput <= 0) continue; // data is gathered only after the Y value has changed

                if (funcValues != null) _evalData.Add(new Vector2(funcValues.Value.x, funcValues.Value.y));
            }
        }
    }
}