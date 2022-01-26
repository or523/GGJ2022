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
            resourceMineralsLabel.text = string.Format("{0}", decision.m_resources_needed.m_minerals);
            resourceFoodLabel.text = string.Format("{0}", decision.m_resources_needed.m_food);
            resourceWoodLabel.text = string.Format("{0}", decision.m_resources_needed.m_wood);
            resourceWorkforceLabel.text = string.Format("{0}", decision.m_resources_needed.m_workforce);

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
