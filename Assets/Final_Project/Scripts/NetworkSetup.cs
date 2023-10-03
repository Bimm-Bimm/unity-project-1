using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkSetup : MonoBehaviour, INetworkRunnerCallbacks
{
    //[SerializeField]
    [SerializeField]
    private NetworkRunner _networkRunnerPrefab;

    private NetworkRunner _runner;
    [SerializeField]
    private NetworkPrefabRef _playerPrefab;
    //[SerializeField] private GameObject _sceneCam;
   // public GameObject _sceneCam;

    private Vector3 _prePos;
   // public GameObject _canvas;
    private Dictionary<PlayerRef, NetworkObject> _spawmedPlayer = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField]
    SessionListUIHandler sessionListUIHandler;
    //Test Game Scene

    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();
        sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);
        if (networkRunnerInScene != null) 
        {
            _runner = networkRunnerInScene;
            _runner.AddCallbacks(this);
        }
    }
    private void Start()
    {
        if (_runner == null) 
        {
            _runner = Instantiate(_networkRunnerPrefab);
            _runner.AddCallbacks(this);
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Join");
        if (_runner.IsServer)
        {
            
            
            NetworkObject networkPlayerObject = _runner.Spawn(_playerPrefab, RandomPosition(), Quaternion.identity, player);
            _spawmedPlayer.Add(player, networkPlayerObject);

            //Camera.main.gameObject.SetActive(false);
            //_canvas.SetActive(false);
        }
        
    }
    private Vector3 RandomPosition() 
    {
        Vector3 _pos = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), 50, UnityEngine.Random.Range(-10.0f, 10.0f));
        while (_pos == _prePos) 
        {
            _pos = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), 25, UnityEngine.Random.Range(-10.0f, 10.0f));
        }
        _prePos = _pos;
        return _pos;
    }
    /*protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, NetAddress address, SceneRef scene, Action<NetworkRunner> initialized) 
    {
        var sceneObjProvider = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneObjectProvider>().FirstOrDefault();
        if (sceneObjProvider == null) 
        {
            sceneObjProvider = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return runner.StartGame(new StartGameArgs
        {

        });
    
    }*/
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Left");
        if (_spawmedPlayer.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawmedPlayer.Remove(player);
           // _sceneCam.SetActive(true);
            //_canvas.SetActive(true);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
   
        data._isMouse0Press = false;
   

        data._isReloadPress = false;
        data._isMouse1Press = false;
        Vector2 viewInputVec = Vector2.zero;
        if (Input.GetKey(KeyCode.Space)) 
        {
            data._isJumpPressed = true;
        }
    
        if (Input.GetKey(KeyCode.W))
            data.movInput += Vector2.up;

        if (Input.GetKey(KeyCode.S))
            data.movInput += Vector2.down;

        if (Input.GetKey(KeyCode.A))
            data.movInput += Vector2.left;

        if (Input.GetKey(KeyCode.D))
            data.movInput += Vector2.right;

        if (Input.GetKey(KeyCode.Q))
            data._isQPress = true;

        if (Input.GetKey(KeyCode.R))
            data._isReloadPress = true;
        if (Input.GetKey(KeyCode.Mouse0))
                data._isMouse0Press = true;

        if (Input.GetKey(KeyCode.Mouse1))
            data._isMouse1Press = true;


        data.mouseInput.x = Input.GetAxis("Mouse X");
        data.mouseInput.y = Input.GetAxis("Mouse Y") * -1;

        input.Set(data);

    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
    {
        if (sessionListUIHandler == null)
            return;
        if (sessionList.Count == 0)
        {
            sessionListUIHandler.OnNoSessionFound();
        }
        else
        {
            sessionListUIHandler.ClearList();
            foreach (SessionInfo session in sessionList) 
            {
                sessionListUIHandler.AddToList(session);
            }

        }
    }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    async void StartGame(GameMode mode , string sessionName , SceneRef scene)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start or join (depends on gamemode) a session with a specific name
        _ = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            //
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            //Scene = SceneManager.GetActiveScene().buildIndex,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

    }

  

  /*  public void Host()
    {
        Debug.Log("Host");
        while (_runner == null) 
        {
            _sceneCam.SetActive(false);
            StartGame(GameMode.Host,"TestSession" , 1);
            
        }

    }
    public void Join()
    {
        while (_runner == null) 
        {
            _sceneCam.SetActive(false);
            StartGame(GameMode.Client, "TestSession" , 1);
 
        }
           
    }*/
    public void OnJoinLobby() 
    {
        var clientTask = JoinLobby();
    }
    private async Task JoinLobby() 
    {
        string lobbyID = "OurLobbyID";
        print("Join JoinLobby");
        var result = await _runner.JoinSessionLobby(SessionLobby.Custom, lobbyID);
    }
    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log("create game");
        Camera.main.gameObject.SetActive(false);
        StartGame(GameMode.Host, sessionName, 1);
       // StartGame(GameMode.Host, sessionName, SceneUtility.GetBuildIndexByScenePath($"scene/{sceneName}"));
        while (_runner == null)
        {
            Debug.Log("create game 1");
            Camera.main.gameObject.SetActive(false);
            //_sceneCam.SetActive(false);
            StartGame(GameMode.Host, sessionName, SceneUtility.GetBuildIndexByScenePath($"scene/{sceneName}"));

        }

    }

    public void CreateGame(string sessionName, int sceneRef)
    {
        Debug.Log("create game");
        Camera.main.gameObject.SetActive(false);
        StartGame(GameMode.Host, sessionName, sceneRef);
       

    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        StartGame(GameMode.Client, sessionInfo.Name, SceneManager.GetActiveScene().buildIndex);
        while (_runner == null)
        {
            Camera.main.gameObject.SetActive(false); 
            //_sceneCam.SetActive(false);
            StartGame(GameMode.Client, sessionInfo.Name, SceneManager.GetActiveScene().buildIndex);

        }

    }
}


    