using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class EliminationGameManager : MonoBehaviour
{


    [SerializeField] private int maxSteps = 10001;
    [SerializeField] int matchtimer;

    [SerializeField] private int roundNumber = 0;

    [SerializeField] private int redTeamBotCount;
    [SerializeField] private int blueTeamBotCount;

    [SerializeField] private GameObject redTeamBotPrefab;
    [SerializeField] private GameObject blueTeamBotPrefab;

    [SerializeField] private GameObject[] levels;
    [SerializeField] private GameObject currentLevel; 
    [SerializeField] private LevelInfo currentLevelInfo;

    [SerializeField] private List<GameObject> redAgentObjects = new List<GameObject>();

    [SerializeField] private List<GameObject> blueAgentObjects = new List<GameObject>();

    private SimpleMultiAgentGroup redTeamAgents;
    private SimpleMultiAgentGroup blueTeamAgents;


    [SerializeField] private int redTeamScore;
    [SerializeField] private int blueTeamScore;


    // Start is called before the first frame update
    void Start()
    {

        NewRandomLevel();

        SpawnAgents();



        //Register bots to teams
        redTeamAgents = new SimpleMultiAgentGroup();
        blueTeamAgents = new SimpleMultiAgentGroup();

        for (int i = 0; i < redAgentObjects.Count; i++)
        {
            redTeamAgents.RegisterAgent(redAgentObjects[i].GetComponent<EliminationAgent>());
        }
        for (int i = 0; i < blueAgentObjects.Count; i++)
        {
            blueTeamAgents.RegisterAgent(blueAgentObjects[i].GetComponent<EliminationAgent>());
        }

    }

    void FixedUpdate()
    {
        //Enviroment Timer
        matchtimer += 1;
        if(matchtimer >= maxSteps)
        {
            Debug.Log("Time Limit Reached. Tie");

            redTeamAgents.GroupEpisodeInterrupted();
            blueTeamAgents.GroupEpisodeInterrupted();

            RoundStart();
        }

        if (redTeamScore == blueTeamBotCount)
        {
            Debug.Log("Red Team Won");
            RoundOver(redTeamAgents);
        }

        if (blueTeamScore == redTeamBotCount)
        {
            Debug.Log("Blue Team Won");
            RoundOver(blueTeamAgents);
        }


    }

    private void SpawnAgents()
    {

        List<int> occupiedSpawns = new List<int>();

        //Spawn in bots
        for (int i = 0; i < redTeamBotCount; i++)
        {
            int randomNo = Random.Range(0, currentLevelInfo.getSpawnPoints().Length);

            bool retrySpawn = false;

            //Check for a valid spawn point
            for (int j = 0; j < occupiedSpawns.Count; j++)
            {
                if (randomNo == occupiedSpawns[j])
                {
                    retrySpawn = true;
                }
            }

            if(retrySpawn == false)
            {
                GameObject agentObject;
                //On First Spawn Only//
                if (roundNumber == 0)
                {
                    agentObject = Instantiate(redTeamBotPrefab);

                    redAgentObjects.Add(agentObject);
                }
                else
                {
                    agentObject = redAgentObjects[i];
                    agentObject.SetActive(true);
                    redTeamAgents.RegisterAgent(agentObject.GetComponent<EliminationAgent>());
                }

                //Reset the agent values
                EliminationAgent _agent = agentObject.GetComponent<EliminationAgent>();
                Health _agentHealth = agentObject.GetComponent<Health>();

                
                _agent.setSpawn(currentLevelInfo.getSpawnPoints()[randomNo]);
                //Doing this in the manager rather than OnEpisodeBegin
                //_agent.SetReward(0);
                
                _agentHealth.setHealth(3);

                occupiedSpawns.Add(randomNo);
            }
            else if (retrySpawn == true)
            {
                i--;
            }
        }

        // ! So much for DRY code !
        // Should realy find a better solution.....
        for (int i = 0; i < blueTeamBotCount; i++)
        {
            int randomNo = Random.Range(0, currentLevelInfo.getSpawnPoints().Length);

            bool retrySpawn = false;

            //Check for a valid spawn point
            for (int j = 0; j < occupiedSpawns.Count; j++)
            {
                if (randomNo == occupiedSpawns[j])
                {
                    retrySpawn = true;
                }
            }

            if (retrySpawn == false)
            {
                GameObject agentObject;
                //On First Spawn Only//
                if (roundNumber == 0)
                {
                    agentObject = Instantiate(blueTeamBotPrefab);

                    blueAgentObjects.Add(agentObject);
                }
                else
                {
                    agentObject = blueAgentObjects[i];
                    agentObject.SetActive(true);
                    blueTeamAgents.RegisterAgent(agentObject.GetComponent<EliminationAgent>());
                }

                //Reset the agent values
                EliminationAgent _agent = agentObject.GetComponent<EliminationAgent>();
                Health _agentHealth = agentObject.GetComponent<Health>();

                _agent.setSpawn(currentLevelInfo.getSpawnPoints()[randomNo]);
                //Doing this in the manager rather than OnEpisodeBegin
                //_agent.SetReward(0);
                _agentHealth.setHealth(3);
                
                occupiedSpawns.Add(randomNo);              
            }
            else if (retrySpawn == true)
            {
                i--;
            }
        }


    }

    private void NewRandomLevel()
    {


















        int randomNo = Random.Range(0, levels.Length);

        currentLevel = Instantiate(levels[randomNo]);

        currentLevelInfo = currentLevel.GetComponent<LevelInfo>();

       // currentLevelInfo = levels[randomNo].GetComponent<LevelInfo>();

    }





   

    private void RoundStart()
    {
        Destroy(currentLevel);

        matchtimer = 0;
        redTeamScore = 0;
        blueTeamScore = 0;
        roundNumber++;

        NewRandomLevel();
        SpawnAgents();
        
    }


    private void RoundOver(SimpleMultiAgentGroup winningTeam)
    {

        winningTeam.AddGroupReward(1 - (float)matchtimer / maxSteps);


        Debug.Log(winningTeam + "with team score of" + (1 - (float)matchtimer / maxSteps));


        redTeamAgents.EndGroupEpisode();
        blueTeamAgents.EndGroupEpisode();

        RoundStart();

    }


    public int getRedScore()
    {
        return redTeamScore;
    }

    public void addRedScore()
    {
        redTeamScore++;
    }

    public int getBlueScore()
    {
        return blueTeamScore;
    }

    public void addBlueScore()
    {
        blueTeamScore ++;
    }

    public int getMaxStep()
    {
        return maxSteps;
    }

}
