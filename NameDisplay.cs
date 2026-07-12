using Revenants.Options;
using Revenants.Services;
using ThunderRoad;
using UnityEngine;
using TMPro;

namespace Revenants;

public class NameDisplay : ThunderBehaviour
{
    private Creature _creature;
    private string _name;
    private TMP_Text _nameTxt;
    private bool _displayName = true;
    private readonly RevenantLevelManager _revenantLevelManager = new RevenantLevelManager();

    public void Dispose()
    {
        Destroy(_nameTxt);
        Destroy(this);
    }

    public void Awake()
    {
        _creature = GetComponent<Creature>();
    }

    public override ManagedLoops EnabledManagedLoops => ManagedLoops.LateUpdate;

    protected override void ManagedOnEnable()
    {
        base.ManagedOnEnable();
        _creature.OnDespawnEvent -= CreatureOnOnDespawnEvent;
        _creature.OnDespawnEvent += CreatureOnOnDespawnEvent;
    }
    
    protected override void ManagedOnDisable()
    {
        _creature.OnDespawnEvent -= CreatureOnOnDespawnEvent;
    }

    public void Init(string name)
    {
        _name = name;
        CreateNameDisplay();
    }

    private void CreatureOnOnDespawnEvent(EventTime eventTime)
    {
        if (eventTime == EventTime.OnEnd) return;
        Dispose();
    }

    private void CreateNameDisplay()
    {
        //_revenantNameGO = new GameObject("RevenantName");
        //_revenantNameGO.gameObject.AddComponent<Canvas>();
        //_revenantNameGO.gameObject.AddComponent<CanvasRenderer>();
        //_revenantNameGO.transform.SetParent(_creature.transform, false);
        //_revenantNameGO.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
        //_revenantNameGO.transform.rotation *= Quaternion.LookRotation(-_creature.transform.right, Vector3.up);
        //_revenantNameGO.transform.localScale = Vector3.one * .4f;
        //_revenantNameGO.transform.localPosition = Vector3.up * 2.5f;
        //_nameTxt.transform.SetParent(_revenantNameGO.transform, false);
        _nameTxt = new GameObject().AddComponent<TextMeshPro>();
        _nameTxt.transform.position = _creature.GetTorso().transform.position + Vector3.up * 1.0f;
        //_nameTxt.transform.SetParent(_creature.ragdoll.rootPart.transform, true);
        _nameTxt.text = _name;
        _nameTxt.fontSize = 1;
        _nameTxt.alignment = TextAlignmentOptions.Center;
    }

    protected override void ManagedLateUpdate()
    {
        base.ManagedLateUpdate();
        // Should display only if the modoption allow to display and the creature isn't hidden
        bool shouldDisplay = ModOptions.DisplayName && !_creature.hidden; 
        if (_displayName != shouldDisplay)
        {
            _displayName = shouldDisplay;
            _nameTxt.gameObject.SetActive(shouldDisplay);
        }
        RefreshRotation();
    }

    private void RefreshRotation()
    {
        if (_nameTxt == null || Camera.main == null) return;
        _nameTxt.transform.position = _creature.GetTorso().transform.position + Vector3.up * 1.0f;
        _nameTxt.transform.forward = Camera.main.transform.forward;
    }
}