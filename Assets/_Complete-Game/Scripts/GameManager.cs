using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using System.Collections.Generic;       //Позволяет нам использовать списки.
using UnityEngine.UI;                   //Позволяет нам использовать пользовательский интерфейс.


namespace Completed
{
	
	

	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f;                      //Время ожидания перед началом уровня в секундах.
		public float turnDelay = 0.1f;                          //Задержка между каждым ходом игрока.
		public int playerFoodPoints = 100;                      //Начальное значение очков еды игрока.
		public static GameManager instance = null;              //Статический экземпляр GameManager, который позволяет любому другому скрипту получить к нему доступ.
		[HideInInspector] public bool playersTurn = true;       //Логическое значение для проверки хода игроков, скрытое в инспекторе, но общедоступное.


		private Text levelText;                                 //Текст для отображения номера текущего уровня.
		private GameObject levelImage;                          //Изображение для блокировки уровня при настройке уровней, фон для levelText.
		private BoardManager boardScript;                       //Сохраните ссылку на наш BoardManager, который настроит уровень.
		private int level = 1;                                  //Текущий номер уровня, выраженный в игре как «День 1».
		private List<Enemy> enemies;                            //Список всех вражеских юнитов, используемый для отдачи им команд движения.
		private bool enemiesMoving;                             //Логическое значение, чтобы проверить, двигаются ли враги.
		private bool doingSetup = true;                         //Логическое значение, чтобы проверить, устанавливаем ли мы доску, запретить игроку двигаться во время установки.



		//Пробуждение всегда вызывается перед любыми функциями запуска.
		void Awake()
		{
			//Проверить, существует ли уже экземпляр
			if (instance == null)

				//если нет, установите этот экземпляр
				instance = this;

			//Если экземпляр уже существует и это не так:
			else if (instance != this)

				//Тогда уничтожь это. Это обеспечивает соблюдение нашего одноэлементного шаблона, а это означает, что может быть только один экземпляр GameManager.
				Destroy(gameObject);

			//Устанавливает, что это не будет уничтожено при перезагрузке сцены
			DontDestroyOnLoad(gameObject);

			//Назначьте врагов новому списку объектов Enemy.
			enemies = new List<Enemy>();

			//Получите ссылку на компонент для прикрепленного скрипта BoardManager
			boardScript = GetComponent<BoardManager>();

			//\Вызовите функцию InitGame для инициализации первого уровня.
			InitGame();
		}

		//тут большое новое

		//Это вызывается каждый раз при загрузке сцены.
		void OnLevelFinishedLoading(Scene scene, LoadSceneMode
		mode)
		{
			//Добавьте единицу к нашему номеру уровня.
			level++;
			//Вызовите InitGame, чтобы инициализировать наш уровень.
			InitGame();
		}
		void OnEnable()
		{
			//Скажите нашей функции OnLevelFinishedLoading, чтобы она начала прослушивать событие смены сцены, как только
			//этот скрипт включен.
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
		}
		void OnDisable()
		{
			//Tell our ‘OnLevelFinishedLoading’ function to stop listening for a scene change event as soon as this
			//script is disabled.
			//Remember to always have an unsubscription for every delegate you subscribe to!
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

		// тут заканчивается



		//это вызывается только один раз, и параметр указывает, что его нужно вызывать только после загрузки сцены.
		//(иначе наш обратный вызов загрузки сцены будет вызываться самой первой загрузкой, а мы этого не хотим)
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static public void CallbackInitialization()
		{
			//зарегистрировать обратный вызов, который будет вызываться каждый раз при загрузке сцены
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		//Это вызывается каждый раз при загрузке сцены.
		static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
		{
			instance.level++;
			instance.InitGame();
		}


		//Инициализирует игру для каждого уровня.
		void InitGame()
		{
			//Пока действие «Setup» равно true, игрок не может двигаться, не позволяйте игроку двигаться, пока открыта титульная карточка.
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

			//Очистите все вражеские объекты в нашем списке, чтобы подготовиться к следующему уровню.
			enemies.Clear();

			//Вызовите функцию SetupScene скрипта BoardManager, передайте ей номер текущего уровня.
			boardScript.SetupScene(level);

		}


		//Скрывает черное изображение, используемое между уровнями
		void HideLevelImage()
		{
			//Отключите игровой объект levelImage.
			levelImage.SetActive(false);

			//Установите для doingSetup значение false, позволяя игроку снова двигаться.
			doingSetup = false;
		}

		//Обновление вызывается каждый кадр.
		void Update()
		{
			//Убедитесь, что в настоящее время не верно.
			if (playersTurn || enemiesMoving || doingSetup)

				//Если что-то из этого верно, вернитесь и не запускайте MoveEnemies.
				return;

			//Начните перемещать врагов.
			StartCoroutine(MoveEnemies());
		}

		//Вызовите это, чтобы добавить переданный объект Enemy в список объектов Enemy.
		public void AddEnemyToList(Enemy script)
		{
			//Добавить врага в список врагов.
			enemies.Add(script);
		}


		//GameOver вызывается, когда игрок достигает 0 очков еды
		public void GameOver()
		{
			//Установите levelText для отображения количества пройденных уровней и сообщения об окончании игры.
			levelText.text = level + " дней ты голодал.";

			//Включить черное фоновое изображение gameObject.
			levelImage.SetActive(true);

			//Отключите этот GameManager.
			enabled = false;
		}

		//Корутина для последовательного перемещения врагов.
		IEnumerator MoveEnemies()
		{
			//В то время, когда для параметра animalsMoving установлено значение true, игрок не может двигаться.
			enemiesMoving = true;

			//Подождите TurnDelay секунд, по умолчанию 0,1 (100 мс).
			yield return new WaitForSeconds(turnDelay);

			//Если нет созданных врагов (IE на первом уровне):
			if (enemies.Count == 0)
			{
				//Ждать TurnDelay секунд между ходами, заменяет задержку, вызванную движением врагов, когда их нет.
				yield return new WaitForSeconds(turnDelay);
			}

			//Прокрутите список объектов противника.
			for (int i = 0; i < enemies.Count; i++)
			{
				//Вызовите функцию MoveEnemy врага с индексом i в списке врагов.
				enemies[i].MoveEnemy();

				//Дождитесь времени перемещения врага, прежде чем двигаться к следующему врагу,
				yield return new WaitForSeconds(enemies[i].moveTime);
			}
			//Как только враги закончат движение, установите для playerTurn значение true, чтобы игрок мог двигаться.
			playersTurn = true;

			//Враги больше не двигаются, установите для параметр enemiesMoving значение false.
			enemiesMoving = false;
		}
	}
}