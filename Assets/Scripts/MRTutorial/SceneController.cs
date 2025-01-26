using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class SceneController : MonoBehaviour
{

    [SerializeField]
    private InputActionReference _togglePlanesAction;

    [SerializeField]
    private InputActionReference _rightActivateAction;

    [SerializeField]
    private InputActionReference _leftActivateAction;

    [SerializeField]
    private XRRayInteractor _leftRayInteractor;

    [SerializeField]
    private GameObject _grabbableCube;

    [SerializeField]
    private GameObject _prefab;

    private ARPlaneManager _planeManager;
    private ARBoundingBoxManager _boundingBoxManager;
    private ARAnchorManager _anchorManager;

    private int _visualizationMode = 0;
    private int _numPlanesAddedOccurred = 0;
    private List<ARAnchor> _anchors = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _planeManager = GetComponent<ARPlaneManager>();

        if (_planeManager is null)
        {
            Debug.LogError("Kill yoself!");
        }

        _boundingBoxManager = GetComponent<ARBoundingBoxManager>();

        if (_boundingBoxManager is null)
        {
            Debug.LogError("Kill yoself box style!");
        }

        _anchorManager = GetComponent<ARAnchorManager>();

        if (_anchorManager is null)
        {
            Debug.LogError("Kill yoself again!");
        }

        _togglePlanesAction.action.performed += OnTogglePlanesAction;
        _rightActivateAction.action.performed += OnRightActivateAction;
        _leftActivateAction.action.performed += OnLeftActivateAction;
    }

    void OnDestroy()
    {
        Debug.Log("-> Destroyyyyy");

        _togglePlanesAction.action.performed -= OnTogglePlanesAction;
        _rightActivateAction.action.performed -= OnRightActivateAction;
        _leftActivateAction.action.performed -= OnLeftActivateAction;

        _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        _boundingBoxManager.trackablesChanged.RemoveListener(OnBoxesChanged);
        _anchorManager.trackablesChanged.RemoveListener(OnAnchorsChanged);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnRightActivateAction(InputAction.CallbackContext obj)
    {
        SpawnGrabbableCube();
    }

    private void OnLeftActivateAction(InputAction.CallbackContext obj)
    {
        CheckIfRayHitsCollider();
    }

    private void CheckIfRayHitsCollider()
    {
        if (_leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log("-> Hit detected: " + hit.transform.name);
            Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            Pose pose = new Pose(hit.point, rotation);
            CreateAnchorAsync(pose);
        }
        else
        {
            Debug.Log("-> No hit detected");
        }
    }

    private async void CreateAnchorAsync(Pose pose)
    {
        var result = await _anchorManager.TryAddAnchorAsync(pose);

        if (result.status.IsSuccess())
        {
            ARAnchor anchor = result.value;
            _anchors.Add(anchor);

            GameObject instance = Instantiate(_prefab, anchor.pose.position, anchor.pose.rotation);
            instance.transform.SetParent(anchor.transform);
        }
    }

    private void SpawnGrabbableCube()
    {
        Debug.Log("-> Spawning nigga cube");

        Vector3 spawnPosition;

        foreach (var plane in _planeManager.trackables)
        {
            if (plane.classifications == PlaneClassifications.Table)
            {
                spawnPosition = plane.transform.position;
                spawnPosition.y += 0.3f;
                Instantiate(_grabbableCube, spawnPosition, Random.rotation);
            }
        }
    }

    private void OnTogglePlanesAction(InputAction.CallbackContext obj)
    {
        _visualizationMode = (_visualizationMode + 1) % 3;

        SetVisualization(_visualizationMode);
    }

    private void SetVisualization(int mode)
    {
        switch (_visualizationMode)
        {
            case 1: // Planes
                SetPlanesVisibility(true);
                SetBoxesVisibility(false);
                break;
            case 2: // Boxes
                SetPlanesVisibility(false);
                SetBoxesVisibility(true);
                break;
            default: // Nuffin
                SetPlanesVisibility(false);
                SetBoxesVisibility(false);
                break;
        }
    }

    private void SetPlanesVisibility(bool isVisible)
    {
        var fillAlpha = isVisible ? 0.3f : 0f;
        var lineAlpha = isVisible ? 1.0f : 0f;

        foreach (var plane in _planeManager.trackables)
        {
            SetTrackableAlpha(plane, fillAlpha, lineAlpha);
        }
    }

    private void SetBoxesVisibility(bool isVisible)
    {
        var fillAlpha = isVisible ? 0.3f : 0f;
        var lineAlpha = isVisible ? 1.0f : 0f;

        foreach (var box in _boundingBoxManager.trackables)
        {
            SetTrackableAlpha(box, fillAlpha, lineAlpha);
        }
    }

    private void SetTrackableAlpha(ARTrackable trackable, float fillAlpha, float lineAlpha)
    {
        var meshRenderer = trackable.GetComponentInChildren<MeshRenderer>();
        var lineRenderer = trackable.GetComponentInChildren<LineRenderer>();

        if (meshRenderer != null)
        {
            Color color = meshRenderer.material.color;
            color.a = fillAlpha;
            meshRenderer.material.color = color;
        }

        if (lineRenderer != null)
        {
            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;

            startColor.a = lineAlpha;
            endColor.a = lineAlpha;

            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }


    public void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
    {
        if (changes.added.Count > 0)
        {
            _numPlanesAddedOccurred++;

            foreach (var plane in _planeManager.trackables)
            {
                PrintTrackableLabel(plane);
            }

            Debug.Log("-> Number of planes: " + _planeManager.trackables.count);
            Debug.Log("-> Number of Planes Added Occurred: " + _numPlanesAddedOccurred);
        }

        SetVisualization(_visualizationMode);
    }

    public void OnBoxesChanged(ARTrackablesChangedEventArgs<ARBoundingBox> changes)
    {
        if (changes.added.Count > 0)
        {
            foreach (var box in _boundingBoxManager.trackables)
            {
                PrintTrackableLabel(box);
            }

            Debug.Log("-> Number of boxes: " + _boundingBoxManager.trackables.count);
        }

        SetVisualization(_visualizationMode);
    }

    public void OnAnchorsChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
    {
        foreach (var removedAnchor in changes.removed)
        {
            if (removedAnchor.Value != null)
            {
                _anchors.Remove(removedAnchor.Value);
                Destroy(removedAnchor.Value.gameObject);
                Debug.Log("-> removed anchor" + removedAnchor.Value.gameObject.name);
            }
        }
    }

    private void PrintTrackableLabel(ARTrackable trackable)
    {
        string classifications = "";

        if (trackable is ARPlane plane) classifications = plane.classifications.ToString();
        if (trackable is ARBoundingBox box) classifications = box.classifications.ToString();

        Debug.Log($"Plane ID: {trackable.trackableId}, Label: {classifications}");
    }
}
