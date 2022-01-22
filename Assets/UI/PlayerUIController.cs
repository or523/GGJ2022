using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIController : MonoBehaviour
{
    // Components
    public Button playerReadyButton;
    public Label playerSecretMissionsLabel;
    public ListView playerDecisionsListView;

    // Layouts
    public VisualTreeAsset playerUI;

    public List<Decision> m_decisions;

    public VisualTreeAsset playerDecisionCard;

    public Sprite energyToggle;
    public Sprite mineralsToggle;
    public Sprite foodToggle;
    public Sprite woodToggle;

    // Start is called before the first frame update
    void Start()
    {
        // disable the game manager
        GameManager.Instance.GetComponent<GameManager>().enabled = false;
        
        m_decisions = new List<Decision>();

        GetComponent<UIDocument>().visualTreeAsset = playerUI;

        var root = GetComponent<UIDocument>().rootVisualElement;
        playerReadyButton = root.Q<Button>("ready-button");
        playerSecretMissionsLabel = root.Q<Label>("secret-mission-text");
        playerDecisionsListView = root.Q<ListView>("decisions-list");

        playerReadyButton.clickable.clickedWithEventInfo += PlayerReadyButtonClicked;
        playerReadyButton.userData = false;

        // ListView binding
        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        Func<VisualElement> makeItem = () =>
        {
            var card = playerDecisionCard.CloneTree();
            card.Q<Button>("decision-button").clickable.clickedWithEventInfo += PlayerDecisionButtonClicked;
            return card;
        };

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            Decision decision = m_decisions[i];
            TemplateContainer card = e as TemplateContainer;

            Button cardButton = card.Q<Button>("decision-button");
            cardButton.userData = decision;

            Label resourceEnergyLabel = card.Q<Label>("energy-resource");
            Label resourceMineralsLabel = card.Q<Label>("minerals-resource");
            Label resourceFoodLabel = card.Q<Label>("food-resource");
            Label resourceWoodLabel = card.Q<Label>("wood-resource");
            Label resourceWorkforceLabel = card.Q<Label>("workforce-resource");

            resourceEnergyLabel.text = string.Format("{0} En", decision.m_resources_needed.m_energy);
            resourceMineralsLabel.text = string.Format("{0} Mi", decision.m_resources_needed.m_minerals);
            resourceFoodLabel.text = string.Format("{0} Fo", decision.m_resources_needed.m_food);
            resourceWoodLabel.text = string.Format("{0} Wo", decision.m_resources_needed.m_wood);
            resourceWorkforceLabel.text = string.Format("{0} Po", decision.m_resources_needed.m_workforce);

            // Decision label (TODO: fix ToString)
            Label decisionLabel = card.Q<Label>("decision-description");
            decisionLabel.text = string.Format("{0}", decision.ToString());

            // Is it selected?
            VisualElement decision_toggle = card.Q<VisualElement>("decision-toggle");
            if (decision.m_is_selected)
            {
                decision_toggle.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
            }
            else
            {
                decision_toggle.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 0f));
            }
            
            switch (NetworkPlayer.LocalInstance.playerResource.Value)
            {
                case ResourceType.Energy:
                    decision_toggle.style.backgroundImage = new StyleBackground(energyToggle.texture);
                    break;
                case ResourceType.Food:
                    decision_toggle.style.backgroundImage = new StyleBackground(foodToggle.texture);
                    break;
                case ResourceType.Minerals:
                    decision_toggle.style.backgroundImage = new StyleBackground(mineralsToggle.texture);
                    break;
                case ResourceType.Wood:
                    decision_toggle.style.backgroundImage = new StyleBackground(woodToggle.texture);
                    break;
            }
        };

        playerDecisionsListView.makeItem = makeItem;
        playerDecisionsListView.bindItem = bindItem;
        playerDecisionsListView.itemsSource = m_decisions;
        playerDecisionsListView.selectionType = SelectionType.None;

        // Secret mission text
        playerSecretMissionsLabel.text = NetworkPlayer.LocalInstance.playerMission.ToString();
    }

    public void PlayerReadyButtonClicked(EventBase tab)
    {
        Button btn = tab.target as Button;

        bool state = (bool)btn.userData;
        
        NetworkPlayer.LocalInstance.UpdatePlayerIsReadyServerRPC(!state);
    }

    public void UpdatePlayerReadyButton(bool state)
    {
        playerReadyButton.userData = state;
        if (state)
        {
            playerReadyButton.text = "Ready!";
        }
        else
        {
            playerReadyButton.text = "Ready?";
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdatePlayerDecisions(List<Decision> decisions)
    {
        // Update decisions
        Debug.Log(decisions);
        m_decisions.Clear();
        m_decisions.AddRange(decisions);
        playerDecisionsListView.RefreshItems();
    }

    internal void UpdatePlayerMission(Mission playerMission)
    {
        playerSecretMissionsLabel.text = playerMission.ToString();
    }

    public void PlayerDecisionButtonClicked(EventBase tab)
    {
        Button button = tab.target as Button;
        Decision decision = button.userData as Decision;

        // TODO: change button color / stamp
        decision.m_is_selected = !decision.m_is_selected;

        VisualElement decision_toggle = button.parent.Q<VisualElement>("decision-toggle");
        if (decision.m_is_selected)
        {
            decision_toggle.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 1f));
        }
        else
        {
            decision_toggle.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 0f));
        }

        NetworkPlayer.LocalInstance.UpdatePlayerDecisionServerRPC(
            decision.m_decision_id, NetworkPlayer.LocalInstance.playerResource.Value, decision.m_is_selected);
    }

    public void UpdatePlayerWon(bool won)
    {
        playerReadyButton.SetEnabled(false);
        if (won)
        {
            playerReadyButton.text = "You Lost!";
        }
        else
        {
            playerReadyButton.text = "You Won!";
        }
    }
}
