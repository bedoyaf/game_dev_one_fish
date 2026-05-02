using System;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// Helper class providing info about the grid tiles
/// </summary>
[Serializable]
public class ComponentGridTile {
    /// <summary>
    /// The component in this tile
    /// </summary>
    public ShipComponentController component;

    /// <summary>
    /// The x coordinate of the tile
    /// </summary>
    public int x;

    /// <summary>
    /// The z coordinate of the tile
    /// </summary>
    public int z;

    /// <summary>
    /// Grid offset to the position of the left side of the component
    /// </summary>
    public int offsetX;

    /// <summary>
    /// Grid offset to the position of the bottom side of the component
    /// </summary>
    public int offsetZ;

    /// <summary>
    /// How many components are blocking this tile
    /// </summary>
    [SerializeField]
    private int blocked;

    public int GetBlock => blocked;

    public bool isPlaceholder; // TODO

    /// <summary>
    /// Whether the placement offset is not zero, i.e. this is not the origin tile of the component
    /// </summary>
    public bool hasOffset => offsetX != 0 || offsetZ != 0;

    public bool isBlocked => blocked > 0;

    public int ComponentHeight => component.placementRules.height;
    public int ComponentWidth => component.placementRules.width;

    //public bool worksWithPrefabs;

    /// <summary>
    /// For placeholders, if it is currently visible
    /// </summary>
    private bool visible;

    [FormerlySerializedAs("instantiatedComponent")]
    [SerializeField]
    private bool _instantiatedComponent;
    private bool instantiatedComponent {
        get {
            //Debug.Log($"Returning {_instantiatedComponent}");
            return _instantiatedComponent;
        }
        set {
            //Debug.Log($"Setting to {value}");
            _instantiatedComponent = value;
        }
    }
    public bool shouldDestroy;

    // Old system
    //[SerializeField] private bool isSolid;
    //public bool IsSolid => isSolid || (component != null && component.placementRules.solid);

    /// <summary>
    /// Current solid system - being solid is a property of a component, not of a grid tile
    /// Returns whether the component attached to this tile is solid
    /// </summary>
    public bool IsSolid => component != null && component.placementRules.solid;


    //[SerializeReference]
    //private ComponentGrid componentGrid;

    /// <summary>
    /// Blocking is set to false, and no offset
    /// </summary>
    public ComponentGridTile(int x, int z) {
        this.x = x;
        this.z = z;
    }


    public void DestroyCurrentComponent() {
        if (!instantiatedComponent) return;

        if (component != null && component.gameObject != null) {
            component.gameObject.SmartDestroy();
        }
    }

    /// <summary>
    /// Do not use for placing normal components!!!
    /// </summary>
    public void PlacePlaceholder(ShipComponentController placeholder, bool isInstantiated, bool shouldDestroy) {
        if (shouldDestroy)
            DestroyCurrentComponent();
        component = placeholder;
        if (!isPlaceholder) {
            isPlaceholder = true;
            ChangeBlock(false);
        }
        offsetX = 0;
        offsetZ = 0;

        instantiatedComponent = isInstantiated;
        if (!isInstantiated) return;
        placeholder.placementRules.connectedTile = this;
    }

    /// <summary>
    /// Do not use for placing placeholders!
    /// Stores the component in the grid
    /// </summary>
    public void PlaceComponent(ShipComponentController component, bool isInstantiated, int offsetX = 0, int offsetZ = 0) {
        DestroyCurrentComponent();
        this.component = component;
        if (isPlaceholder) {
            isPlaceholder = false;
            ChangeBlock(true);
        }
        this.offsetX = offsetX;
        this.offsetZ = offsetZ;

        instantiatedComponent = isInstantiated;
        if (!isInstantiated) return;
        component.placementRules.connectedTile = this;
    }

    public void ToggleVisibility(bool toggle) {
        component.GetComponentInChildren<MeshRenderer>().enabled = toggle;
        visible = toggle;
    }

    /// <summary>
    /// Visualizes if the placeholder is blocked by some other component.
    /// </summary>
    private void PlaceholderColor() {
        if (!visible || !isPlaceholder) return;

        if (isBlocked) {
            component.GetComponentInChildren<MeshRenderer>().material.color = new Color(1, 0.95f, 0.95f);
        }
        else {
            component.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
    }

    /// <summary>
    /// Now useless, because the solid system changed
    /// </summary>
    public void SetSolid(bool solid) {
        if (isPlaceholder) return;

        //isSolid = solid;
        ShowSolid();
    }

    /// <summary>
    /// Now useless
    /// </summary>
    public void ToggleSolid() {
        if (isPlaceholder) return;

        //isSolid = !isSolid;
        ShowSolid();
    }

    /// <summary>
    /// Kinda useless
    /// </summary>
    public void ShowSolid() {
        if (instantiatedComponent) {
            //var mat = component.GetComponentInChildren<MeshRenderer>().material;
            //var color = mat.color;
            //var inverseColor = new Color(1 - color.r, 1 - color.g, 1 - color.b);
            //mat.SetColor("_EmissionColor", isSolid ? inverseColor : Color.black);
            //var pos = component.transform.position;
            //pos.y = IsSolid ? 0.2f : 0f;
            //component.transform.position = pos;
        }
    }

    public void AddBlock() {
        blocked++;
    }

    public void RemoveBlock() {
        blocked = Mathf.Max(blocked - 1, 0);
    }
    public void ChangeBlock(bool add) {
        if (add) AddBlock();
        else RemoveBlock();

        PlaceholderColor();
    }
    public void ChangeBlock(int add) {
        blocked += add;

        PlaceholderColor();
    }

    public override string ToString() {
        return $"Tile {x} {z} {component} instantiated: {instantiatedComponent}";
    }
}