using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace PolygonArsenal
{

public class PolygonBeamScript : MonoBehaviour {

    [Header("Prefabs")]
    public GameObject[] beamLineRendererPrefab;
    public GameObject[] beamStartPrefab;
    public GameObject[] beamEndPrefab;

    private int _currentBeam = 0;

    private GameObject _beamStart;
    private GameObject _beamEnd;
    private GameObject _beam;
    private LineRenderer _line;

    [Header("Adjustable Variables")]
    public float beamEndOffset = 1f; //How far from the raycast hit point the end effect is positioned
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
	public float textureLengthScale = 3; //Length of the beam texture

    [Header("Put Sliders here (Optional)")]
    public Slider endOffSetSlider; //Use UpdateEndOffset function on slider
    public Slider scrollSpeedSlider; //Use UpdateScrollSpeed function on slider

    [Header("Put UI Text object here to show beam name")]
    public Text textBeamName;

    // Use this for initialization
    void Start()
    {
        if (textBeamName)
            textBeamName.text = beamLineRendererPrefab[_currentBeam].name;
        if (endOffSetSlider)
            endOffSetSlider.value = beamEndOffset;
        if (scrollSpeedSlider)
            scrollSpeedSlider.value = textureScrollSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetMouseButtonDown(0))
        {
            _beamStart = Instantiate(beamStartPrefab[_currentBeam], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            _beamEnd = Instantiate(beamEndPrefab[_currentBeam], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            _beam = Instantiate(beamLineRendererPrefab[_currentBeam], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            _line = _beam.GetComponent<LineRenderer>();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Destroy(_beamStart);
            Destroy(_beamEnd);
            Destroy(_beam);
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                Vector3 tdir = hit.point - transform.position;
                ShootBeamInDir(transform.position, tdir);
            }
        }
		
		if (Input.GetKeyDown(KeyCode.RightArrow)) //4 next if commands are just hotkeys for cycling beams
        {
            NextBeam();
        }

		if (Input.GetKeyDown(KeyCode.D))
		{
			NextBeam();
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			PreviousBeam();
		}
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousBeam();
        }
		
    }

    public void NextBeam() // Next beam
    {
        if (_currentBeam < beamLineRendererPrefab.Length - 1)
            _currentBeam++;
        else
            _currentBeam = 0;

        if (textBeamName)
            textBeamName.text = beamLineRendererPrefab[_currentBeam].name;
    }
	
	    public void PreviousBeam() // Previous beam
    {
        if (_currentBeam > - 0)
            _currentBeam--;
        else
            _currentBeam = beamLineRendererPrefab.Length - 1;

        if (textBeamName)
            textBeamName.text = beamLineRendererPrefab[_currentBeam].name;
    }
	

    public void UpdateEndOffset()
    {
        beamEndOffset = endOffSetSlider.value;
    }

    public void UpdateScrollSpeed()
    {
        textureScrollSpeed = scrollSpeedSlider.value;
    }

    void ShootBeamInDir(Vector3 start, Vector3 dir)
    {
        _line.positionCount = 2;
        _line.SetPosition(0, start);
        _beamStart.transform.position = start;

        Vector3 end = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit))
            end = hit.point - (dir.normalized * beamEndOffset);
        else
            end = transform.position + (dir * 100);

        _beamEnd.transform.position = end;
        _line.SetPosition(1, end);

        _beamStart.transform.LookAt(_beamEnd.transform.position);
        _beamEnd.transform.LookAt(_beamStart.transform.position);

        float distance = Vector3.Distance(start, end);
        _line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        _line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }
}
}
