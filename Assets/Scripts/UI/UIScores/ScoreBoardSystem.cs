using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using IT4080C;

partial class ScoreBoardSystem : SystemBase
{
    private EntityQuery ghostQuery;
    private UIDocument uiDocument;
    private MultiColumnListView scoreboard;
    private GameObject uiObject;

    PlayerNameManager pNameManager;
    int netId = 999;
    protected override void OnCreate()
    {


        ghostQuery = GetEntityQuery(typeof(GhostInstance), typeof(HealthComponent));


  


        // Find UIDocument in scene
        uiObject = GameObject.FindWithTag("UIManager");
        if (uiObject != null)
        {
            Debug.Log("Found Score UIManager");
            uiDocument = uiObject.GetComponent<UIDocument>();
            scoreboard = uiDocument.rootVisualElement.Q<MultiColumnListView>("ScoreboardMultiColListView");
            scoreboard.columns.Add(new Column()
            {
                title = "PlayerName",
                makeCell = MakeCellLabel,
                bindCell = BindNameToCell,
                stretchable = true,
            });
            scoreboard.columns.Add(new Column()
            {
                title = "Deaths",
                makeCell = MakeCellLabel,
                bindCell = BindDeathsToCell,
                stretchable = true,
            });
            scoreboard.columns.Add(new Column()
            {
                title = "Kills",
                makeCell = MakeCellLabel,
                bindCell = BindKillsToCell,
                stretchable = true,
            });
            
        }
        else
        {
            Debug.Log("No Score UIManager");
        }
        pNameManager = new PlayerNameManager();
        base.OnCreate();
    }

    private void Instance_OnPlayerNameChanged(string obj)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnUpdate()
    {

        //Find the netID 
        EntityQuery connectionQuery = GetEntityQuery(
            ComponentType.ReadOnly<NetworkStreamInGame>(),
            ComponentType.ReadOnly<NetworkId>()
        );
        if (connectionQuery.IsEmpty)
        {
           // Debug.Log("No Conn");
        }
        else if(netId > 10)
        {
            var connectionEntity = connectionQuery.GetSingletonEntity();
            netId = EntityManager.GetComponentData<NetworkId>(connectionEntity).Value;
            Debug.Log("Conn: " + netId);
        }
        else
        {
            //nothing we have a netid
        }


        if (uiObject == null)
        {
            // Debug.Log("No UIManager, searching");
            uiObject = GameObject.FindWithTag("UIScoreManager");

        }
        else if (uiObject != null && uiDocument == null)
        {
            // Debug.Log("UIManager found, searching for UIdoc");
            uiDocument = uiObject.GetComponent<UIDocument>();

        }
        else if (uiObject != null && uiDocument != null && scoreboard == null)
        {
            // Debug.Log("Found UIDoc, searching for HealthSlider!");
            scoreboard = uiDocument.rootVisualElement.Q<MultiColumnListView>("ScoreboardMultiColListView");

            return;
        }
        else
        {
            //Debug.Log("Lets go!");
        }

        var scores = new List<PlayerScore>();
        int cnt = 0;
        Entities.ForEach((ref HealthComponent healthComp) =>
        //.WithAll<GhostInstance>().ForEach((ref HealthComponent health, ref GhostOwner ghostOwner, ref GhostOwnerIsLocal gol) =>
        {
          //  Debug.Log("NID: " + netId + "HPNID: "+healthComp.ownerNetworkID);
            if (netId == healthComp.ownerNetworkID)
            {
                healthComp.playerName = pNameManager.GetPlayerName();
                //Debug.Log("NID: " + netId + "HPNID: " + healthComp.ownerNetworkID + " hname: " + healthComp.playerName);
            }
           
            cnt++;
            PlayerScore tmp = new PlayerScore(""+ healthComp.playerName, (int)healthComp.kills, (int)healthComp.deaths);
            scores.Add(tmp);
            

        }).WithoutBurst().Run();
        if (scores.Count > 0)
        {
            ApplyPersons(scores);
        }
        if (scoreboard != null)
        {
            scoreboard.Rebuild();
        }
    }
    public void ApplyPersons(List<PlayerScore> scores)
    {
        scoreboard.itemsSource = scores;
    }
    private Label MakeCellLabel() => new();

    private void BindNameToCell(VisualElement element, int index)
    {
        var label = (Label)element;
        var person = (PlayerScore)scoreboard.viewController.GetItemForIndex(index);
        label.text = person.playerName;
    }

    private void BindDeathsToCell(VisualElement element, int index)
    {
        var label = (Label)element;
        var playerScore = (PlayerScore)scoreboard.viewController.GetItemForIndex(index);
        label.text = "" + playerScore.deaths;
    }

    private void BindKillsToCell(VisualElement element, int index)
    {
        var label = (Label)element;
        var playerScore = (PlayerScore)scoreboard.viewController.GetItemForIndex(index);
        label.text = "" + playerScore.kills;
    }
}
public class PlayerScore
{
    public string playerName;
    public int kills;
    public int deaths;

    public PlayerScore(string pName, int kill, int death)
    {
        playerName = pName;
        kills = kill;
        deaths = death;
    }
}
