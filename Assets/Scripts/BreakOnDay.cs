using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnDay : MonoBehaviour {

    public int DayToBreak = 0;

    bool hasBroken = false;
    bool hasChangedColour = false;
    bool hasCheckedThisDay = false;

    void Update() {
        //If marked to be destroyed and game is not paused
        if(hasBroken && !GameController.Current.bIsPaused) {
            //Destroy this
            Destroy(this.gameObject);
        }

        //If game is not paused and have not checked this day
        if (GameController.Current.bIsPaused && !hasCheckedThisDay) {
            hasCheckedThisDay = true;

            //If hasnt broken
            if (!hasBroken) {
                //If day before 
                if (!hasChangedColour && GameController.Current.GetDay() == DayToBreak) {
                    //Mark object to be destroyed 
                    hasBroken = true;
                    //Has now changed colour
                    hasChangedColour = true;
                    //Change colour
                    GetComponent<Renderer>().material = GameController.Current.RedTransparentMaterial;
                    //Change queue so it displays properly
                    SetRenderQueue(3030);
                    //Disable collider for water world height array
                    GetComponent<Collider>().enabled = false;
                }
            }
        }

        //If is not paused and have checked, reset as it means day has started
        else if(!GameController.Current.bIsPaused && hasCheckedThisDay) {
            hasCheckedThisDay = false;
        }
    }

    void SetRenderQueue(int queue) {
        int[] m_queues =  new int[] { queue };

        //Reapply to materials
        Material[] materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length && i < m_queues.Length; ++i) {
            materials[i].renderQueue = m_queues[i];
        }
    }
}
