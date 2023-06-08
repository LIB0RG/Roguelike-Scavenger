using UnityEngine;
using System.Collections;
using System.Collections.Generic;        //Позволяет нам использовать списки.
using UnityEngine.UI;                    //Позволяет нам использовать пользовательский интерфейс.

public class GameManager : MonoBehaviour
{
    //Время ожидания перед началом уровня, int секунд.
    public float levelStartDelay = 2f;
    //Задержка между каждым ходом игрока.
    public float turnDelay = 0.1f;
    //Начальное значение очков еды игрока.
    public int playerFoodPoints = 100;
    //Статический экземпляр GameManager, который позволяет любому другому скрипту получить к нему доступ.
    public static GameManager instance = null;
    //Логическое значение для проверки хода игроков, скрытое в инспекторе, но общедоступное.
    [HideInInspector] public bool playersTurn = true;

    //Текст для отображения номера текущего уровня.
    private Text levelText;
    //Изображение для блокировки уровня при настройке уровней, фон для levelText.
    private GameObject levelImage;
    //Сохраните ссылку на наш BoardManager, который настроит уровень.
    private BoardManager boardScript;
    //Текущий номер уровня, выраженный в игре как «День 1».
    private int level = 1;
    //Список всех вражеских юнитов, используемый для отдачи им команд движения.
    private List<Enemy> enemies;
    //Логическое значение, чтобы проверить, двигаются ли враги.
    private bool enemiesMoving;
    //Логическое значение, чтобы проверить, устанавливаем ли мы доску, запретить игроку двигаться во время установки.
    private bool doingSetup = true;


    //Пробуждение всегда вызывается перед любыми функциями запуска.
    void Awake()
    {
        //Проверить, существует ли уже экземпляр
        if (instance == null)

            //если нет, установите этот экземпляр
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        //Assign enemies to a new List of Enemy objects.
        enemies = new List<Enemy>();

        //Get a component reference to the attached BoardManager script
        boardScript = GetComponent<BoardManager>();

        //Call the InitGame function to initialize the first level 
        InitGame();
    }

    //This is called each time a scene is loaded.
    void OnLevelWasLoaded(int index)
    {
        //Add one to our level number.
        level++;
        //Call InitGame to initialize our level.
        InitGame();
    }

    //Initializes the game for each level.
    void InitGame()
    {
        //While doingSetup is true the player can't move, prevent player from moving while title card is up.
        doingSetup = true;

        //Получите ссылку на наше изображение LevelImage, найдя его по имени.
        levelImage = GameObject.Find("LevelImage");

        //Получите ссылку на наш текстовый компонент LevelText, найдя его по имени и вызвав GetComponent.
        levelText = GameObject.Find("LevelText").GetComponent<Text>();

        //Установите текст levelText в строку «День» и добавьте номер текущего уровня.
        levelText.text = "День " + level;

        //Установите levelImage на активное блокирование обзора игрового поля игроком во время установки.
        levelImage.SetActive(true);

        //Вызовите функцию HideLevelImage с задержкой levelStartDelay в секундах.
        Invoke("HideLevelImage", levelStartDelay);

        //Clear any Enemy objects in our List to prepare for next level.
        enemies.Clear();

        //Call the SetupScene function of the BoardManager script, pass it current level number.
        boardScript.SetupScene(level);

    }


    //Hides black image used between levels
    void HideLevelImage()
    {
        //Disable the levelImage gameObject.
        levelImage.SetActive(false);

        //Set doingSetup to false allowing player to move again.
        doingSetup = false;
    }

    //Update is called every frame.
    void Update()
    {
        //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
        if (playersTurn || enemiesMoving || doingSetup)

            //If any of these are true, return and do not start MoveEnemies.
            return;

        //Start moving enemies.
        StartCoroutine(MoveEnemies());
    }

    //Call this to add the passed in Enemy to the List of Enemy objects.
    public void AddEnemyToList(Enemy script)
    {
        //Add Enemy to List enemies.
        enemies.Add(script);
    }


    //GameOver is called when the player reaches 0 food points
    public void GameOver()
    {
        //Set levelText to display number of levels passed and game over message
        levelText.text = level + " дней вы голодали.";

        //Enable black background image gameObject.
        levelImage.SetActive(true);

        //Disable this GameManager.
        enabled = false;
    }

    //Coroutine to move enemies in sequence.
    IEnumerator MoveEnemies()
    {
        //While enemiesMoving is true player is unable to move.
        enemiesMoving = true;

        //Wait for turnDelay seconds, defaults to .1 (100 ms).
        yield return new WaitForSeconds(turnDelay);

        //If there are no enemies spawned (IE in first level):
        if (enemies.Count == 0)
        {
            //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
            yield return new WaitForSeconds(turnDelay);
        }

        //Loop through List of Enemy objects.
        for (int i = 0; i < enemies.Count; i++)
        {
            //Call the MoveEnemy function of Enemy at index i in the enemies List.
            enemies[i].MoveEnemy();

            //Wait for Enemy's moveTime before moving next Enemy, 
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        //Once Enemies are done moving, set playersTurn to true so player can move.
        playersTurn = true;

        //Enemies are done moving, set enemiesMoving to false.
        enemiesMoving = false;
    }
}