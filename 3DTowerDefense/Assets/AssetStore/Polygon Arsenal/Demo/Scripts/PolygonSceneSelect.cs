using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace PolygonArsenal
{

public class PolygonSceneSelect : MonoBehaviour
{
	[FormerlySerializedAs("GUIHide")] public bool guiHide = false;
	[FormerlySerializedAs("GUIHide2")] public bool guiHide2 = false;
	[FormerlySerializedAs("GUIHide3")] public bool guiHide3 = false;
	[FormerlySerializedAs("GUIHide4")] public bool guiHide4 = false;
	[FormerlySerializedAs("GUIHide5")] public bool guiHide5 = false;
	
	//Combat Scenes
	
    public void CbLoadSceneMissiles()		{ SceneManager.LoadScene("PolyMissiles");	}
	public void CbLoadSceneBeams()			{ SceneManager.LoadScene("PolyBeams"); 		}
	public void CbLoadSceneBeams2()			{ SceneManager.LoadScene("PolyBeams2"); 	}
	public void CbLoadSceneAura()			{ SceneManager.LoadScene("PolyAura"); 		}
	public void CbLoadSceneAura2()			{ SceneManager.LoadScene("PolyAura2");	 	}
	public void CbLoadSceneAura3()			{ SceneManager.LoadScene("PolyAura3"); 		}
	public void CbLoadSceneAura4()			{ SceneManager.LoadScene("PolyAura4"); 		}
	public void CbLoadSceneBarrage()		{ SceneManager.LoadScene("PolyBarrage"); 	}
	public void CbLoadSceneBarrage2()		{ SceneManager.LoadScene("PolyBarrage2"); 	}
	public void CbLoadSceneChains()			{ SceneManager.LoadScene("PolyChains"); 	}
	public void CbLoadSceneChains2()		{ SceneManager.LoadScene("PolyChains2"); 	}
	public void CbLoadSceneCleave()			{ SceneManager.LoadScene("PolyCleave"); 	}
	public void CbLoadSceneCombat01()		{ SceneManager.LoadScene("PolyCombat01"); 	}
	public void CbLoadSceneCombat02()		{ SceneManager.LoadScene("PolyCombat02"); 	}
	public void CbLoadSceneCurses()			{ SceneManager.LoadScene("PolyCurses"); 	}
	public void CbLoadSceneDeath()			{ SceneManager.LoadScene("PolyDeath");	 	}
	public void CbLoadSceneEnchant()		{ SceneManager.LoadScene("PolyEnchant"); 	}
	public void CbLoadSceneExploMini()		{ SceneManager.LoadScene("PolyExploMini"); 	}
	public void CbLoadSceneGore()			{ SceneManager.LoadScene("PolyGore"); 		}
	public void CbLoadSceneHitscan()		{ SceneManager.LoadScene("PolyHitscan"); 	}
	public void CbLoadSceneNecromancy()		{ SceneManager.LoadScene("PolyNecromancy");	}
	public void CbLoadSceneNova()			{ SceneManager.LoadScene("PolyNova"); 		}
	public void CbLoadSceneOrbitalBeam()	{ SceneManager.LoadScene("PolyOrbitalBeam");}
	public void CbLoadSceneSpikes()			{ SceneManager.LoadScene("PolySpikes"); 	}
	public void CbLoadSceneSpikes2()		{ SceneManager.LoadScene("PolySpikes2"); 	}
	public void CbLoadSceneSpikes3()		{ SceneManager.LoadScene("PolySpikes3"); 	}
	public void CbLoadSceneSpikes4()		{ SceneManager.LoadScene("PolySpikes4"); 	}
	public void CbLoadSceneSurfaceDmg()		{ SceneManager.LoadScene("PolySurfaceDmg");	}
	public void CbLoadSceneSword()			{ SceneManager.LoadScene("PolySword"); 		}
	public void CbLoadSceneSwordTrail()		{ SceneManager.LoadScene("PolySwordTrail");	}
	
	//Environment Scenes
	
	public void EnvLoadSceneConfetti()		{ SceneManager.LoadScene("PolyConfetti"); 	}
	public void EnvLoadSceneEnvironment()	{ SceneManager.LoadScene("PolyEnvironment");}
	public void EnvLoadSceneFire()			{ SceneManager.LoadScene("PolyFire"); 		}
	public void EnvLoadSceneFire2()			{ SceneManager.LoadScene("PolyFire2"); 		}
	public void EnvLoadSceneFireflies()		{ SceneManager.LoadScene("PolyFireflies"); 	}
	public void EnvLoadSceneFireworks()		{ SceneManager.LoadScene("PolyFireworks"); 	}
	public void EnvLoadSceneLiquid()		{ SceneManager.LoadScene("PolyLiquid"); 	}
	public void EnvLoadSceneLiquid2()		{ SceneManager.LoadScene("PolyLiquid2"); 	}
	public void EnvLoadSceneRocks()			{ SceneManager.LoadScene("PolyRocks"); 		}
	public void EnvLoadSceneSparks()		{ SceneManager.LoadScene("PolySparks"); 	}
	public void EnvLoadSceneTornado()		{ SceneManager.LoadScene("PolyTornado"); 	}
	public void EnvLoadSceneWeather()		{ SceneManager.LoadScene("PolyWeather"); 	}
	
	//Interactive Scenes
	
	public void INTLoadSceneBeamUp()		{ SceneManager.LoadScene("PolyBeamUp"); 	}
	public void INTLoadSceneBlackHole()		{ SceneManager.LoadScene("PolyBlackHole"); 	}
	public void INTLoadSceneHeal()			{ SceneManager.LoadScene("PolyHeal"); 		}
	public void INTLoadSceneJets()			{ SceneManager.LoadScene("PolyJets"); 		}
	public void INTLoadSceneLoot()			{ SceneManager.LoadScene("PolyLoot"); 		}
	public void INTLoadScenePortal()		{ SceneManager.LoadScene("PolyPortal"); 	}
	public void INTLoadScenePortal2()		{ SceneManager.LoadScene("PolyPortal2"); 	}
	public void INTLoadScenePowerupIcon()	{ SceneManager.LoadScene("PolyPowerupIcon");}
	public void INTLoadSceneSpawn()			{ SceneManager.LoadScene("PolySpawn"); 		}
	public void INTLoadSceneTrails()		{ SceneManager.LoadScene("PolyTrails"); 	}
	public void INTLoadSceneTreasure()		{ SceneManager.LoadScene("PolyTreasure"); 	}
	public void INTLoadSceneTreasure2()		{ SceneManager.LoadScene("PolyTreasure2"); 	}
	public void INTLoadSceneZones()			{ SceneManager.LoadScene("PolyZones"); 		}
	
	 void Update ()
	 {
 
     if(Input.GetKeyDown(KeyCode.L))
	 {
         guiHide = !guiHide;
     
         if (guiHide)
		 {
             GameObject.Find("CanvasSceneSelectCom").GetComponent<Canvas> ().enabled = false;
         }
		 
		 else
		 {
             GameObject.Find("CanvasSceneSelectCom").GetComponent<Canvas> ().enabled = true;
         }
     }
	      if(Input.GetKeyDown(KeyCode.J))
	 {
         guiHide2 = !guiHide2;
     
         if (guiHide2)
		 {
             GameObject.Find("CanvasMissiles").GetComponent<Canvas> ().enabled = false;
         }
		 else
		 {
             GameObject.Find("CanvasMissiles").GetComponent<Canvas> ().enabled = true;
         }
     }
		if(Input.GetKeyDown(KeyCode.H))
	 {
         guiHide3 = !guiHide3;
     
         if (guiHide3)
		 {
             GameObject.Find("CanvasTips").GetComponent<Canvas> ().enabled = false;
         }
		 else
		 {
             GameObject.Find("CanvasTips").GetComponent<Canvas> ().enabled = true;
         }
     }
	 if(Input.GetKeyDown(KeyCode.M))
	 {
         guiHide4 = !guiHide4;
     
         if (guiHide4)
		 {
             GameObject.Find("CanvasSceneSelectInt").GetComponent<Canvas> ().enabled = false;
         }
		 
		 else
		 {
             GameObject.Find("CanvasSceneSelectInt").GetComponent<Canvas> ().enabled = true;
         }
     }
	 if(Input.GetKeyDown(KeyCode.N))
	 {
         guiHide5 = !guiHide5;
     
         if (guiHide5)
		 {
             GameObject.Find("CanvasSceneSelectEnv").GetComponent<Canvas> ().enabled = false;
         }
		 
		 else
		 {
             GameObject.Find("CanvasSceneSelectEnv").GetComponent<Canvas> ().enabled = true;
         }
     }
	}
}

}