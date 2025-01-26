using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;


public class SceneVisualizer : MonoBehaviour
{

    [SerializeField]
    private InputActionReference _togglePlanesAction;

    private ARPlaneManager _planeManager;
    private ARBoundingBoxManager _boundingBoxManager;

    private int _visualizationMode = 0;
    private int _numPlanesAddedOccurred = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _planeManager = GetComponent<ARPlaneManager>();

        if (_planeManager is null)
        {
            Debug.LogError("No plane manager dumbass nigga!");
        }

        _boundingBoxManager = GetComponent<ARBoundingBoxManager>();

        if (_boundingBoxManager is null)
        {
            Debug.LogError("No box manager dumbass nigga");
        }

        _togglePlanesAction.action.performed += OnTogglePlanesAction;
    }

    void OnDestroy()
    {
        _togglePlanesAction.action.performed -= OnTogglePlanesAction;

        _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        _boundingBoxManager.trackablesChanged.RemoveListener(OnBoxesChanged);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTogglePlanesAction(InputAction.CallbackContext obj)
    {
        _visualizationMode = (_visualizationMode + 1) % 3;

        SetVisualization(_visualizationMode);
    }

    private void SetVisualization(int mode)
    {
        switch (mode)
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

    private void PrintTrackableLabel(ARTrackable trackable)
    {
        string classifications = "";

        if (trackable is ARPlane plane) classifications = plane.classifications.ToString();
        if (trackable is ARBoundingBox box) classifications = box.classifications.ToString();

        Debug.Log($"Plane ID: {trackable.trackableId}, Label: {classifications}");
    }
}
