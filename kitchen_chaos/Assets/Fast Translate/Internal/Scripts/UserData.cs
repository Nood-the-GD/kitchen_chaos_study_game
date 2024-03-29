using UnityEngine;
using System.Collections.Generic;
namespace FastTranslate.Internal
{
	//[CreateAssetMenu(fileName = "UserData", menuName = "Gridly/UserData", order = 1)]

	[System.Serializable]
	public class RecordTemp
    {

		public string viewID;
		public ActionRecord actionRecord;
		public Record record;
    }
	public enum ActionRecord
	{
		Add,
		Delete
	}
	public class UserData : ScriptableObject
	{
		static UserData _singleton;
		public Languages mainLangEditor = Languages.enUS;
		public static UserData singleton
		{
            set
            {
				Init();
				_singleton = value;
            }
			get 
			{
				Init();
				return _singleton;
			}
			
		}
	
		public string keyAPI;

		public bool showServerMess;
		static void Init()
        {
			if (_singleton == null)
			{
				_singleton = Resources.Load<UserData>("GridlyUserData");
				if(_singleton == null)
                {
					CreateAsset();
					_singleton = Resources.Load<UserData>("GridlyUserData");
				}

			}
		}

		static void CreateAsset()
		{
#if UNITY_EDITOR
			var sample = Resources.Load<UserData>("SampleGridlyUserData");
			var p = ScriptableObject.CreateInstance<UserData>();
			p.keyAPI = sample.keyAPI;
			p.mainLangEditor = sample.mainLangEditor;
			p.showServerMess = sample.showServerMess;

			string path = "Assets/Resources/GridlyUserData.asset";

			if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
			{
				UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
			}

			UnityEditor.AssetDatabase.CreateAsset(p, path);
			UnityEditor.AssetDatabase.SaveAssets();
			UnityEditor.AssetDatabase.Refresh();
			Debug.Log("GRIDLY USER DATA CREATED");
#endif
		}


	}

}
