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

    // Start is called before the first frame update
    void Start()
    {
        m_decisions = new List<Decision>();
        GetComponent<UIDocument>().visualTreeAsset = playerUI;

        var root = GetComponent<UIDocument>().rootVisualElement;
        playerReadyButton = root.Q<Button>("ready-button");
        playerSecretMissionsLabel = root.Q<Label>("secret-mission-text");
        playerDecisionsListView = root.Q<ListView>("decisions-list");

        playerReadyButton.clicked += PlayerReadyButtonClicked;

        // ListView binding
        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        Func<VisualElement> makeItem = () => new Label();

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = m_decisions[i].ToString();

        playerDecisionsListView.makeItem = makeItem;
        playerDecisionsListView.bindItem = bindItem;
        playerDecisionsListView.itemsSource = m_decisions;
        playerDecisionsListView.selectionType = SelectionType.Multiple;

        // Callback invoked when the user double clicks an item
        playerDecisionsListView.onItemsChosen += Debug.Log;

        // Callback invoked when the user changes the selection inside the ListView
        playerDecisionsListView.onSelectionChange += Debug.Log;
    }

    public void PlayerReadyButtonClicked()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdatePlayerDecisions(List<Decision> decisions)
    {
        // Update decisions
        m_decisions.Clear();
        m_decisions.AddRange(decisions);
    }
}
