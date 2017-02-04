using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseOverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public BuildingType Building;

    string buildingName;
    string description;
    int cost;

    void Start() {
        switch (Building) {
            case BuildingType.Sandbags:
                buildingName = BuildingController.Current.SandbagsPrefab.GetComponent<Building>().buildingName;
                cost = BuildingController.Current.SandbagsPrefab.GetComponent<Building>().buildingCost;
                description = BuildingController.Current.SandbagsPrefab.GetComponent<Building>().buildingDescription;
                break;

            case BuildingType.Concrete:
                buildingName = BuildingController.Current.ConcretePrefab.GetComponent<Building>().buildingName;
                cost = BuildingController.Current.ConcretePrefab.GetComponent<Building>().buildingCost;
                description = BuildingController.Current.ConcretePrefab.GetComponent<Building>().buildingDescription;
                break;

            case BuildingType.Dam:
                buildingName = BuildingController.Current.DamPrefab.GetComponent<Building>().buildingName;
                cost = BuildingController.Current.DamPrefab.GetComponent<Building>().buildingCost;
                description = BuildingController.Current.DamPrefab.GetComponent<Building>().buildingDescription;
                break;

            case BuildingType.Ditch:
                buildingName = BuildingController.Current.DitchPrefab.GetComponent<Building>().buildingName;
                cost = BuildingController.Current.DitchPrefab.GetComponent<Building>().buildingCost;
                description = BuildingController.Current.DitchPrefab.GetComponent<Building>().buildingDescription;
                break;

            case BuildingType.Drain:
                buildingName = BuildingController.Current.DrainPrefab.GetComponent<Building>().buildingName;
                cost = BuildingController.Current.DrainPrefab.GetComponent<Building>().buildingCost;
                description = BuildingController.Current.DrainPrefab.GetComponent<Building>().buildingDescription;
                break;
        }

        description = description.Replace("BREAK", "\n");
    }

    public void OnPointerEnter(PointerEventData eventData) {
        GameController.Current.DisplayContextPanel(buildingName, description, cost);
    }

    public void OnPointerExit(PointerEventData eventData) {
        GameController.Current.HideContextPanel();
    }
}
