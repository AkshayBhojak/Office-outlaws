using System.Linq;
using UnityEngine;
using Utilities;
using Utils;

namespace Game
{
    public sealed class CircleRadarView : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private float _radius = 3;
        [SerializeField] private int _segments = 10;

        private Vector3[] _vertices;
        private Vector2[] _uvs;
        private float[] _distances;
        private float[] _tempDistances;
        private Vector3[] _tempVertices;
        private Vector2[] _tempUvs;
        private float[] _linecastDistances;

        private bool _isEven;
        private Mesh _mesh;
        private int _layerMask;
        private int _victimLayerMask;

        private void OnEnable()
        {
            if (null != _vertices)
                return;

            _vertices = new Vector3[_segments + 1];
            _uvs = new Vector2[_segments + 1];

            _uvs[0] = new Vector2(.5f, .5f);

            for (int i = 1; i <= _segments; i++)
            {
                float angle = Mathf.Lerp(0, Mathf.PI * 2, (i - 1f) / (_segments));
                _vertices[i] = new Vector3(Mathf.Sin(angle) * _radius, 0, Mathf.Cos(angle) * _radius);
                _uvs[i] = new Vector2(Mathf.Sin(angle) * .5f + .5f, Mathf.Cos(angle) * .5f + .5f);
            }

            _distances = new float[_vertices.Length];

            _tempDistances = new float[_vertices.Length];
            _tempVertices = (Vector3[])_vertices.Clone();
            _tempUvs = new Vector2[_vertices.Length];
            _linecastDistances = new float[_vertices.Length];

            _layerMask = LayerUtils.GetRadarAllLayerMask();
            _victimLayerMask = LayerUtils.GetVictimLayerMask();

            _isEven = MathUtil.RandomBool;

            var triangles = new int[_segments * 3];
            int index = 0;
            for (int i = 1; i < _vertices.Length - 1; i++)
            {
                triangles[index++] = 0;
                triangles[index++] = i;
                triangles[index++] = i + 1;

                _distances[i] = _vertices[i].magnitude;
            }

            triangles[index++] = 0;
            triangles[index++] = _vertices.Length - 1;
            triangles[index++] = 1;

            _distances[_vertices.Length - 1] = _vertices[_vertices.Length - 1].magnitude;

            _mesh = new Mesh();

            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            _mesh.triangles = triangles;
            _meshFilter.mesh = _mesh;
        }

        public Collider GetSeenVictim()
        {
            if (!Physics.CheckSphere(transform.position, _radius, _victimLayerMask))
                return null;

            RaycastHit hit;

            for (int i = 1; i < _vertices.Length; i++)
            {
                if (Physics.Linecast(transform.position, Convert(_tempVertices[i]), out hit, _victimLayerMask))
                {
                    return hit.collider;
                }
            }

            return null;
        }

        private void LateUpdate()
        {
            if (_isEven && Time.frameCount % 2 == 0)
                return;

            if (!_isEven && Time.frameCount % 2 != 0)
                return;

            var position = transform.position;

            if (!Physics.CheckSphere(position, _radius, _layerMask) && _linecastDistances.Sum() <= 0)
            {
                return;
            }

            bool isEquals = true;

            for (int i = 1; i < _vertices.Length; i++)
            {
                if (Physics.Linecast(position, Convert(_vertices[i]), out var hitInfo, _layerMask))
                {
                    _linecastDistances[i] = hitInfo.distance;
                }
                else
                {
                    _linecastDistances[i] = 0;
                }

                if (!Mathf.Approximately(_linecastDistances[i], _tempDistances[i]))
                {
                    isEquals = false;
                    _tempDistances[i] = _linecastDistances[i];
                }
            }

            if (isEquals)
                return;

            for (int i = 1; i < _vertices.Length; i++)
            {
                var distance = _linecastDistances[i];

                if (distance > 0 && distance < _distances[i])
                {
                    var factor = distance / _distances[i];

                    _tempVertices[i] = _vertices[i] * factor;
                    _tempUvs[i] = new Vector2(_tempVertices[i].x / _radius * .5f + .5f, _tempVertices[i].z / _radius * .5f + .5f);
                }
                else
                {
                    _tempVertices[i] = _vertices[i];
                    _tempUvs[i] = _uvs[i];
                }
            }

            _tempUvs[0] = new Vector2(.5f, .5f);

            _mesh.uv = _tempUvs;
            _mesh.vertices = _tempVertices;
            _meshFilter.mesh = _mesh;
        }

        private Vector3 Convert(Vector3 v)
        {
            return transform.localToWorldMatrix.MultiplyPoint(v);
        }
    }
}