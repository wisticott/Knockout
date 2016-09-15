using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class CloudManager : MonoBehaviour {

	public string PlayFabId;

	public bool IsDesktop;

	public GameObject markerPrefab;

	public Transform rootMarker;

	bool loadedFirst = false;

	bool setFirst = false;

	private Transform firstSetMarker;

	private Transform firstGetMarker;

	private Vector3 firstCloudMarkerPos;

	private Quaternion firstCloudMarkerRot;

	private Dictionary<string, string> data = new Dictionary<string, string> ();

	private int markerCount = 0;

	void Login(string titleId)
	{
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			TitleId = titleId,
			CreateAccount = false,
			CustomId = "HackathonUser1"
		};

		PlayFabClientAPI.LoginWithCustomID(request, (result) => {
			PlayFabId = result.PlayFabId;
			Debug.Log("Got PlayFabID: " + PlayFabId);

			if(result.NewlyCreated)
			{
				Debug.Log("(new account)");
			}
			else
			{
				Debug.Log("(existing account)");
			}
		},
			(error) => {
				Debug.Log("Error logging in player with custom ID:");
				Debug.Log(error.ErrorMessage);
			});
	}

	public void ClientGetTitleData()
	{
		var getRequest = new PlayFab.ClientModels.GetTitleDataRequest();
		PlayFabClientAPI.GetTitleData(getRequest, (result) => {
			Debug.Log("Got the following titleData:");
			foreach (var entry in result.Data)
			{
				Debug.Log(entry.Key + ": " + entry.Value);
			}
		},
			(error) => {
				Debug.Log("Got error getting titleData:");
				Debug.Log(error.ErrorMessage);
			});
	}

	void SetUserData(string key, string value)
	{
		UpdateUserDataRequest request = new UpdateUserDataRequest()
		{
			Data = new Dictionary<string, string>(){
				{key, value},
			}
		};

		PlayFabClientAPI.UpdateUserData(request, (result) =>
			{
				Debug.Log("Successfully updated user data");
			}, (error) =>
			{
				Debug.Log("Got error setting user data Ancestor to Arthur");
				Debug.Log(error.ErrorDetails);
			});
	}

	// Use this for initialization
	void Start () {
		PlayFabSettings.TitleId = "A76E";
		Login ("A76E");
		if (IsDesktop) {
			StartCoroutine ("DesktopPoll");
		}
	}

	string EncodeTransform(Vector3 pos, Quaternion rot) {
		string output = "";
		output += pos.x + ",";
		output += pos.y + ",";
		output += pos.z + ",";
		output += rot.x + ",";
		output += rot.y + ",";
		output += rot.z + ",";
		output += rot.w;
		return output;
	}

	void DecodeTransform(Transform t, string data) {
		string[] dataArr = data.Split (',');
		t.position = new Vector3(float.Parse(dataArr [0]), float.Parse(dataArr[1]), float.Parse(dataArr[2]));
		t.rotation = new Quaternion(float.Parse(dataArr [3]), float.Parse(dataArr[4]), float.Parse(dataArr[5]), float.Parse(dataArr[6]));
		if (!loadedFirst) {
			loadedFirst = true;
			t.position = new Vector3(float.Parse(dataArr [0]), float.Parse(dataArr[1]), float.Parse(dataArr[2]));
			t.rotation = new Quaternion(float.Parse(dataArr [3]), float.Parse(dataArr[4]), float.Parse(dataArr[5]), float.Parse(dataArr[6]));

			firstCloudMarkerPos = t.position;
			firstCloudMarkerRot = t.rotation;
			t.position = rootMarker.position;
			t.rotation = rootMarker.rotation;
			firstGetMarker = t;
		} else {
			t.parent = firstGetMarker;
			t.localPosition = new Vector3(float.Parse(dataArr [0]), float.Parse(dataArr[1]), float.Parse(dataArr[2]));
			t.localRotation = new Quaternion(float.Parse(dataArr [3]), float.Parse(dataArr[4]), float.Parse(dataArr[5]), float.Parse(dataArr[6]));

		}
	}

	public void CloudSaveMarker(Transform t) {
		//System.Guid myGUID = System.Guid.NewGuid();
		if (!setFirst) {
			setFirst = true;
			firstSetMarker = t;
			SetUserData ("" + markerCount, EncodeTransform (t.position, t.rotation));
		} else {
			t.parent = firstSetMarker;
			SetUserData ("" + markerCount, EncodeTransform (t.localPosition, t.localRotation));
		}
		markerCount ++;
	}




	void GetUserData()
	{
		GetUserDataRequest request = new GetUserDataRequest()
		{
			PlayFabId = PlayFabId,
			Keys = null
		};


		PlayFabClientAPI.GetUserData(request,(result) => {
			Debug.Log("Got user data:");
			if ((result.Data == null) || (result.Data.Count == 0))
			{
				Debug.Log("No user data available");
			}
			else
			{
				foreach (var item in result.Data)
				{
					Debug.Log("    " + item.Key + " == " + item.Value.Value);
					//if (data.Keys item.Key
					data[item.Key] = item.Value.Value;
				}
			}
		}, (error) => {
			Debug.Log("Got error retrieving user data:");
			Debug.Log(error.ErrorMessage);
		});
	}

	IEnumerator CloudGetMarkers() {
		GetUserData ();
		yield return new WaitForSeconds (2.0f);

		GameObject go = (GameObject) GameObject.Instantiate (markerPrefab);
		DecodeTransform (go.transform, data["0"]);
		foreach (string key in data.Keys) {
			if (key == "0") {
				continue;
			}
			go = (GameObject) GameObject.Instantiate (markerPrefab);
			DecodeTransform (go.transform, data[key]);
		}
	}

	// Update is called once per frame
	void Update () {
		
	}

	IEnumerator DesktopPoll() {
		while (true) {
			yield return new WaitForSeconds (3.0f);
			StartCoroutine ("CloudGetMarkers");
			//yield return null;
//			if (Input.GetKeyUp(KeyCode.R)) {
//				StartCoroutine ("CloudGetMarkers");
//			}
			break;
		}
	}
}
