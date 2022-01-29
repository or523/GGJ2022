using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ServerGameUIController : MonoBehaviour
{
    public static ServerGameUIController Instance;

    // Components
    public Label roundLabel;
    public Label eventLabel;

    public Label resourceEnergyLabel;
    public Label resourceMineralsLabel;
    public Label resourceFoodLabel;
    public Label resourceWoodLabel;
    public Label resourceWorkforceLabel;

    public VisualElement resourceEnergyReadyIndicator;
    public VisualElement resourceMineralsReadyIndicator;
    public VisualElement resourceFoodReadyIndicator;
    public VisualElement resourceWoodReadyIndicator;
    public VisualElement resourceWorkforceReadyIndicator;

    public ListView decisionsList;

    // Layouts
    public VisualTreeAsset serverGameUI;
    public VisualTreeAsset decisionCard;

    public List<Decision> m_decisions;

    public GameObject displayManager;

    void Awake()
    {
        ServerGameUIController.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // start ambient music
        AudioManager.Instance.PlayAmbience();

        // stop background animation and show buildings
        DisplayManager.Instance.ToggleBuildings(true);

        m_decisions = new List<Decision>();
        GetComponent<UIDocument>().visualTreeAsset = serverGameUI;

        var root = GetComponent<UIDocument>().rootVisualElement;

        roundLabel = root.Q<Label>("round-count-label");
        eventLabel = root.Q<Label>("event-label");
        resourceEnergyLabel = root.Q<Label>("energy-resource");
        resourceMineralsLabel = root.Q<Label>("minerals-resource");
        resourceFoodLabel = root.Q<Label>("food-resource");
        resourceWoodLabel = root.Q<Label>("wood-resource");
        resourceWorkforceLabel = root.Q<Label>("workforce-resource");
        resourceEnergyReadyIndicator = root.Q<VisualElement>("energy-ready");
        resourceMineralsReadyIndicator = root.Q<VisualElement>("minerals-ready");
        resourceFoodReadyIndicator = root.Q<VisualElement>("food-ready");
        resourceWoodReadyIndicator = root.Q<VisualElement>("wood-ready");
        resourceWorkforceReadyIndicator = root.Q<VisualElement>("workforce-ready");
        decisionsList = root.Q<ListView>("decisions-list");

        // Update upper HUD
        UpdateResourceCounts();
        UpdateRoundCount();
        UpdateEvent("Good Luck!");

        // Bind to decisions
        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        Func<VisualElement> makeItem = () =>
        {
            return decisionCard.CloneTree();
        };

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            VisualElement card = (e as VisualElement);
            Decision decision = m_decisions[i];

            // Resources
            Label resourceEnergyLabel = card.Q<Label>("energy-resource");
            Label resourceMineralsLabel = card.Q<Label>("minerals-resource");
            Label resourceFoodLabel = card.Q<Label>("food-resource");
            Label resourceWoodLabel = card.Q<Label>("wood-resource");
            Label resourceWorkforceLabel = card.Q<Label>("workforce-resource");

            resourceEnergyLabel.text = string.Format("{0}", decision.m_resources_needed.m_energy);
            resourceEnergyLabel.style.color = new StyleColor(
                decision.m_resources_allocated.m_energy == decision.m_resources_needed.m_energy ? Color.green : Color.red);

            resourceMineralsLabel.text = string.Format("{0}", decision.m_resources_needed.m_minerals);
            resourceMineralsLabel.style.color = new StyleColor(
                decision.m_resources_allocated.m_minerals == decision.m_resources_needed.m_minerals ? Color.green : Color.red);

            resourceFoodLabel.text = string.Format("{0}", decision.m_resources_needed.m_food);
            resourceFoodLabel.style.color = new StyleColor(
                decision.m_resources_allocated.m_food == decision.m_resources_needed.m_food ? Color.green : Color.red);

            resourceWoodLabel.text = string.Format("{0}", decision.m_resources_needed.m_wood);
            resourceWoodLabel.style.color = new StyleColor(
                decision.m_resources_allocated.m_wood == decision.m_resources_needed.m_wood ? Color.green : Color.red);

            resourceWorkforceLabel.text = string.Format("{0}", decision.m_resources_needed.m_workforce);
            resourceWorkforceLabel.style.color = new StyleColor(
                decision.m_resources_allocated.m_workforce == decision.m_resources_needed.m_workforce ? Color.green : Color.red);

            // Decision label (TODO: fix ToString)
            Label decisionLabel = card.Q<Label>("decision-description");
            decisionLabel.text = string.Format("{0}", decision.ToString());
        };

        decisionsList.makeItem = makeItem;
        decisionsList.bindItem = bindItem;
        decisionsList.itemsSource = m_decisions;
        decisionsList.selectionType = SelectionType.None;
    }

    public void UpdateResourceCounts()
    {
        resourceEnergyLabel.text = string.Format("{0}", ResourceManagerBehaviour.Instance.m_resources.m_energy);
        resourceMineralsLabel.text = string.Format("{0}", ResourceManagerBehaviour.Instance.m_resources.m_minerals);
        resourceFoodLabel.text = string.Format("{0}", ResourceManagerBehaviour.Instance.m_resources.m_food);
        resourceWoodLabel.text = string.Format("{0}", ResourceManagerBehaviour.Instance.m_resources.m_wood);
        resourceWorkforceLabel.text = string.Format("{0}", ResourceManagerBehaviour.Instance.m_resources.m_workforce);
    }

    public void UpdatePlayerReady(ResourceType type, bool newValue)
    {
        Debug.Log("UpdatePlayerReady(" + type + ", " + newValue + ")");
        switch (type)
        {
            case ResourceType.Energy:
                resourceEnergyReadyIndicator.visible = newValue;
                break;

            case ResourceType.Food:
                resourceFoodReadyIndicator.visible = newValue; 
                break;

            case ResourceType.Workforce:
                resourceWorkforceReadyIndicator.visible = newValue; 
                break;

            case ResourceType.Wood:
                resourceWoodReadyIndicator.visible = newValue; 
                break;

            case ResourceType.Minerals:
                resourceMineralsReadyIndicator.visible = newValue;
                break;
        }
    }

    public void UpdateRoundCount()
    {
        roundLabel.text = string.Format("Round {0}", GameManager.Instance.m_turn_counter + 1);
    }

    public void UpdateEvent(string eventName)
    {
        eventLabel.text = eventName;
    }
    public void UpdateServerDecisions(List<Decision> decisions)
    {
        // Update decisions
        Debug.Log(decisions);
        m_decisions.Clear();
        m_decisions.AddRange(decisions);
        decisionsList.RefreshItems();

        // Fix "List is empty" label
        if (m_decisions.Count == 0)
        {
            Label emptyLabel = decisionsList.Q<Label>(null, "unity-list-view__empty-label");
            emptyLabel.text = "No decisions this turn";
            emptyLabel.style.fontSize = 20; // This won't work because shared style-sheets. So just hardcode 20px
        }
    }

    public void UpdateGameEnd(bool won)
    {
        roundLabel.text = "Game Finished!";
        if (won)
        {
            eventLabel.text = "Team Won!";
        }
        else
        {
            eventLabel.text = "Team Lost!";
        }
    }

    void Update()
    {

    }
}
