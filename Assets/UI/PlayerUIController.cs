using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class DecisionButton : Button
{
    public Decision decision;
    public bool isSelected;

    public void flipSelection()
    {
        isSelected = !isSelected;
    }
}

public class PlayerUIController : MonoBehaviour
{
    // Components
    public Button playerReadyButton;
    public Label playerSecretMissionsLabel;
    public ListView playerDecisionsListView;

    // Layouts
    public VisualTreeAsset playerUI;

    public List<Decision> m_decisions;

    // Start is called before the first frame update
    void Start()
    {
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
            DecisionButton button = new DecisionButton();
            button.clickable.clickedWithEventInfo += PlayerDecisionButtonClicked;
            return button;
        };

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            DecisionButton button = e as DecisionButton;
            button.decision = m_decisions[i];
            button.isSelected = false;

            // TODO: represent the decision graphically 
            button.text = "[X] Decision - " + m_decisions[i].m_decision_id;
        };

        playerDecisionsListView.makeItem = makeItem;
        playerDecisionsListView.bindItem = bindItem;
        playerDecisionsListView.itemsSource = m_decisions;
        playerDecisionsListView.selectionType = SelectionType.None;
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

    public void PlayerDecisionButtonClicked(EventBase tab)
    {
        DecisionButton button = tab.target as DecisionButton;
        Decision decision = button.decision;

        // TODO: change button color
        button.flipSelection();
        if (button.isSelected)
        {
            button.text = "[V] Decision - " + decision.m_decision_id;
        }
        else
        {
            button.text = "[X] Decision - " + decision.m_decision_id;
        }

        NetworkPlayer.LocalInstance.UpdatePlayerDecisionServerRPC(
            decision.m_decision_id, NetworkPlayer.LocalInstance.playerResource.Value, button.isSelected);
    }
}
