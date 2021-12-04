using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.AI;
using Debug = System.Diagnostics.Debug;

public class WallPlacementTest : MonoBehaviour
{
    public WallCheck bot;
    public WallCheck insideBot;

    [SerializeField] private float radius;
    [SerializeField] private float size;
    [SerializeField] private Vector3 offset;

    [SerializeField] private GameObject objectFirst;
    
    [SerializeField] private List<Transform> objects;
    [SerializeField] private List<Transform> bestTargets;
    [SerializeField] private float snapDistance;
    
    [SerializeField] private Camera camera;

    private Transform _currentNode;

    private Ray _cameraRay;
    
    private Quaternion _newRotation;
    
    private void Update()
    {
        Move();
        Rotate();
        PlaceWall();
    }

    private void PlaceWall()
    {
        var reach = CanReach();
        
        if (!reach)
        {
            return;
        }
        
        if (CurrentNodeInWall()) return;

        if (!Input.GetMouseButtonDown(0)) return;
        
        var transform1 = transform;
        
        var position = _currentNode == default ? transform1.position : _currentNode.position;
        var rotation = _currentNode == default ? transform1.rotation : _currentNode.rotation;
        
        var newObj = Instantiate(objectFirst, position, rotation);
        
        if (newObj is null) return;

        CheckStrays();

        objects.Add(newObj.transform);
    }

    private void Rotate()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        var y = 0;

        var transform1 = transform;

        switch (transform1.eulerAngles.y)
        {
            case 0:
                y = 90;
                offset.x = size / 2;
                offset.z = size / 2;
                break;
            case 90:
                y = 0;
                offset.x = 0;
                offset.z = 0;
                break;
        }

        _newRotation = Quaternion.Euler(0, y, 0);
    }

    private void Move()
    {
        var plane = new Plane(Vector3.down, transform.forward);
        _cameraRay = camera.ScreenPointToRay(Input.mousePosition);

        ClosestTargets();

        NodeInNode();

        if (!plane.Raycast(_cameraRay, out var enter)) return;
        
        var pos = _cameraRay.GetPoint(enter);

        var snappedPos = GridSnappedPos(pos);
        
        _currentNode = default;
        
        if (Points() != default)
        {
            CurrentNodePosition(pos);
        }
        
        var transform1 = transform;
        
        transform1.position = _currentNode == default ? snappedPos : _currentNode.position;
        transform1.rotation = _currentNode == default ? _newRotation : _currentNode.rotation;

    }

    private Vector3 GridSnappedPos(Vector3 pos)
    {
        var newPos = pos;

        newPos -= offset;

        var xCount = Mathf.RoundToInt(newPos.x / size);
        var zCount = Mathf.RoundToInt(newPos.z / size);

        var snappedPos = new Vector3(xCount * size, pos.y, zCount * size);

        snappedPos += offset;
        return snappedPos;
    }

    private void CheckStrays()
    {
        var reach = CanReach();

        if (reach) return;

        if (_currentNode == default) return;

        if (objects[objects.Count - 1].transform.position != _currentNode.transform.position) return;
        
        var parent = _currentNode.parent;
        
        objects.Remove(parent);

        Destroy(parent.gameObject);
    }
    
    private void CurrentNodePosition(Vector3 pos)
    {
        foreach (var node in Points())
        {
            var direction = node.position - pos;

            var sqr = direction.sqrMagnitude;

            if (sqr >= snapDistance) continue;

            pos = node.position;

            _currentNode = node;
        }
    }

    private void NodeInNode()
    {
        foreach (var node in Points())
        {
            var position = node.position;

            var results = new Collider[1];
            var i = Physics.OverlapSphereNonAlloc(position, 0.4f, results);

            for (var index = 0; index < i; index++)
            {
                var hitCollider = results[index];
                
                if (hitCollider == default) continue;

                if (!hitCollider.CompareTag($"Node")) continue;

                if (hitCollider.transform == node.transform) continue;

                Destroy(node.gameObject);
            }
        }
    }

    private void ClosestTargets()
    {
        var position1 = transform.position;

        const int maxColliders = 4;
        var results = new Collider[maxColliders];
        var i = Physics.OverlapSphereNonAlloc(position1, radius, results);

        for (var index = 0; index < i; index++)
        {
            var hitCollider = results[index];
            
            if (!hitCollider.transform.CompareTag($"Wall")) continue;
            
            if (bestTargets.Contains(hitCollider.transform)) continue;
            
            bestTargets.Add(hitCollider.transform);
        }

        if (bestTargets.Count >= 10) bestTargets.RemoveAt(0);
    }

    private bool CanReach()
    {
        return insideBot.path.status == NavMeshPathStatus.PathComplete &&
               bot.path.status == NavMeshPathStatus.PathComplete;
    }

    private GameObject _currentWall;
    
    private bool CurrentNodeInWall()
    {
        if (_currentNode == default) return false;
        
        var position = _currentNode.position;
        var top = new Vector3(position.x, position.y, position.z);

        const int maxColliders = 4;
        var results = new Collider[maxColliders];
        var i = Physics.OverlapSphereNonAlloc(top, 0.2f, results);

        for (var index = 0; index < i; index++)
        {
            var hitCollider = results[index];

            if (!hitCollider.transform.CompareTag($"Wall")) continue;

            _currentWall = hitCollider.gameObject;

            return _currentWall.transform.position == _currentNode.position;
        }

        return false;
    }

    private List<Transform> Points()
    {
        if (bestTargets == default) return new List<Transform>();

        bestTargets = bestTargets.Where(x => x != null).ToList();

        var points = new List<Transform>();
        
        foreach (var t in bestTargets)
        {
            var j = 0;
            for (; j < t.childCount; j++)
            {
                points.Add(t.GetChild(j));
            }
        }
        
        return points;
    }

    private void OnDrawGizmos()
    {
        foreach (var node in Points())
        {
            var position = node.position;
            var top = new Vector3(position.x, position.y, position.z);

            var ray = new Ray(top, Vector3.down * 50);
            Gizmos.DrawRay(ray);
            
            Gizmos.DrawSphere(top, 0.5f);
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.black;
        
        var n = _currentNode == default ? $"None" : $"{_currentNode.name}, {_currentNode.eulerAngles.y}, {_currentWall.name} {_currentNode.transform.position == _currentWall.transform.position}";

        GUI.Label(new Rect(100, 50, 400, 50), n);
        
        GUI.Label(new Rect(100, 100, 100, 50), Points().Count.ToString());
        
        if (bestTargets == default) return;
        
        GUI.Label(new Rect(140, 0, 400, 50), bestTargets.Count.ToString());
    }
}
