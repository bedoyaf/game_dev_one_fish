using UnityEngine;

public class BuilderComponentDescriptionShowingScript : MonoBehaviour
{

    [SerializeField] private ComponentDescriptionUI descriptionUI;


    
    private void Update()
    {
        if (GameManager.IsPaused)
            return;

        // check for ship components or builder components

        // components
        RaycastHit hit = new();
        var target1 = MouseController.Instance.GetMouseOver(ref hit);

        if (target1 != null)
        {
            var d = target1.GetDescription;
            
            // placeholder components in builder break the ui 
            if (!d.ignore)
                descriptionUI.ShowDescription(d);
            else
                descriptionUI.HideDescription();
        } else
        {
            hit = new();
            var target2 = MouseController.Instance.GetMouseOverDraggable(ref hit);

            if (target2 != null)
            {
                descriptionUI.ShowDescription(target2.componentPrefab.GetDescription());
            }
            else {
                descriptionUI.HideDescription();
            }
        }        

    }


}
