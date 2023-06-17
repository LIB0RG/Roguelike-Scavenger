using UnityEngine;
using System.Collections;
using UnityEngine.UI;   //Позволяет нам использовать пользовательский интерфейс.
using UnityEngine.SceneManagement;

namespace Completed
{
	//Player наследуется от MovingObject, нашего базового класса для движущихся объектов, Enemy также наследуется от него.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;        //Время задержки в секундах до перезапуска уровня.
		public int pointsPerFood = 10;              //Количество очков, добавляемых игроку к очкам еды при подборе пищевого объекта.
		public int pointsPerSoda = 20;              //Количество очков, добавляемых игроку к очкам еды при подборе предмета с газировкой.
		public int wallDamage = 1;                  //Сколько урона игрок наносит стене, раскалывая ее.
		public Text foodText;                       //Текст пользовательского интерфейса для отображения общего количества еды для текущего игрока.
		public AudioClip moveSound1;                //1 из 2 аудиоклипов для воспроизведения при перемещении игрока.
		public AudioClip moveSound2;                //2 из 2 Аудиоклипы для воспроизведения при перемещении игрока.
		public AudioClip eatSound1;                 //1 из 2 аудиоклипов, которые воспроизводятся, когда игрок собирает пищевой объект.
		public AudioClip eatSound2;                 //2 из 2 Аудиоклипы, которые воспроизводятся, когда игрок собирает пищевой объект.
		public AudioClip drinkSound1;               //1 из 2 аудиоклипов, которые воспроизводятся, когда игрок собирает газировку.
		public AudioClip drinkSound2;               //2 из 2 Аудиоклипов, которые воспроизводятся, когда игрок собирает газировку.
		public AudioClip gameOverSound;             //Аудиоклип для воспроизведения, когда игрок умирает.

		private Animator animator;                  //Используется для хранения ссылки на компонент аниматора проигрывателя.
		private int food;                           //Используется для хранения общего количества очков еды игрока во время уровня.
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
		private Vector2 touchOrigin = -Vector2.one; //Используется для хранения местоположения источника касания экрана для мобильных элементов управления.
#endif


		//Start переопределяет функцию Start MovingObject.
		protected override void Start()
		{
			//Получить ссылку компонента на компонент аниматора проигрывателя
			animator = GetComponent<Animator>();

			//Получить текущее общее количество очков еды, хранящееся в GameManager. экземпляр между уровнями.
			food = GameManager.instance.playerFoodPoints;

			//Установите foodText, чтобы отразить общее количество еды текущего игрока.
			foodText.text = "Food: " + food;

			//Вызовите функцию Start базового класса MovingObject.
			base.Start();
		}


		//Эта функция вызывается, когда поведение становится отключенным или неактивным.
		private void OnDisable()
		{
			//Когда объект Player отключен, сохраните текущее общее количество еды в GameManager, чтобы его можно было повторно загрузить на следующем уровне.
			GameManager.instance.playerFoodPoints = food;
		}


		private void Update()
		{
			//Если это не ход игрока, выйдите из функции.
			if (!GameManager.instance.playersTurn) return;

			int horizontal = 0;     //Используется для хранения горизонтального направления перемещения.
			int vertical = 0;       //Используется для хранения направления вертикального перемещения.

			//Проверьте, работаем ли мы в редакторе Unity или в автономной сборке.
#if UNITY_STANDALONE || UNITY_WEBPLAYER

			//Получите ввод из диспетчера ввода, округлите его до целого числа и сохраните по горизонтали, чтобы установить направление движения по оси x.
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			//Получите ввод из диспетчера ввода, округлите его до целого числа и сохраните по вертикали, чтобы задать направление движения по оси Y.
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//Проверьте, двигаетесь ли вы по горизонтали, если да, то установите по вертикали ноль.
			if(horizontal != 0)
			{
				vertical = 0;
			}


			//Проверьте, работаем ли мы на iOS, Android, Windows Phone 8 или Unity iPhone.
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

			//Проверьте, зарегистрировал ли ввод больше нуля касаний
			if (Input.touchCount > 0)
			{
				//Сохраните первое обнаруженное касание.
				Touch myTouch = Input.touches[0];

				//Проверьте, равна ли фаза этого касания «Начало»
				if (myTouch.phase == TouchPhase.Began)
				{
					//Если это так, установите touchOrigin в положение этого касания.
					touchOrigin = myTouch.position;
				}

				//Если фаза касания не начинается, а вместо этого равна Ended, а x touchOrigin больше или равен нулю:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Установите touchEnd равным положению этого касания
					Vector2 touchEnd = myTouch.position;

					//Вычислите разницу между началом и концом касания по оси x.
					float x = touchEnd.x - touchOrigin.x;

					//Вычислите разницу между началом и концом касания по оси Y.
					float y = touchEnd.y - touchOrigin.y;

					//Установить сенсорное происхождение. x на -1, чтобы наш оператор else if оценивал ложь и не повторялся немедленно.
					touchOrigin.x = -1;

					//Проверьте, больше ли разница по оси x разницы по оси y.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//Если x больше нуля, установите Horizontal равным 1, в противном случае установите его равным -1.
						horizontal = x > 0 ? 1 : -1;
					else
						//Если y больше нуля, установите Horizontal равным 1, в противном случае установите его равным -1.
						vertical = y > 0 ? 1 : -1;
				}
			}

#endif //Конец раздела компиляции, зависящего от мобильной платформы, который начинается выше с #elif
			//Проверяем, есть ли у нас ненулевое значение для горизонтали или вертикали
			if (horizontal != 0 || vertical != 0)
			{
				//Вызвать AttemptMove, передав общий параметр Стена, так как это то, с чем игрок может взаимодействовать, если они столкнутся с ней (атакуя ее).
				//Передайте горизонталь и вертикаль в качестве параметров, чтобы указать направление перемещения игрока.
				AttemptMove<Wall>(horizontal, vertical);
			}
		}

		//AttemptMove переопределяет функцию AttemptMove в базовом классе MovingObject.
		//AttemptMove принимает общий параметр T, который для Player будет иметь тип Wall, а также целые числа для направления x и y для перемещения.
		protected override void AttemptMove<T>(int xDir, int yDir)
		{
			//Каждый раз, когда игрок перемещается, вычтите из общего количества очков еды.
			food--;

			//Обновите текстовый дисплей еды, чтобы отразить текущий счет.
			foodText.text = "Еды: " + food;

			//Вызовите метод AttemptMove базового класса, передав компонент T (в данном случае Wall) и направления x и y для перемещения.
			base.AttemptMove<T>(xDir, yDir);

			//Hit позволяет нам ссылаться на результат Linecast, выполненный в Move.
			RaycastHit2D hit;

			//Если Move возвращает true, это означает, что Player смог переместиться в пустое место.
			if (Move(xDir, yDir, out hit))
			{
				//Вызовите RandomizeSfx из SoundManager, чтобы воспроизвести звук движения, передав два аудиоклипа на выбор.
				SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
			}

			//Поскольку игрок переместился и потерял очки еды, проверьте, не закончилась ли игра.
			CheckIfGameOver();

			//Установите для логического значения playerTurn в GameManager значение false, когда ход игроков окончен.
			GameManager.instance.playersTurn = false;
		}


		//OnCantMove переопределяет абстрактную функцию OnCantMove в MovingObject.
		//Он принимает общий параметр T, который в случае Player представляет собой стену, которую игрок может атаковать и разрушить.
		protected override void OnCantMove<T>(T component)
		{
			//Установите hitWall равным компоненту, переданному в качестве параметра.
			Wall hitWall = component as Wall;

			//Вызовите функцию DamageWall стены, в которую мы врезаемся.
			hitWall.DamageWall(wallDamage);

			//Установите триггер атаки контроллера анимации игрока, чтобы воспроизвести анимацию атаки игрока.
			animator.SetTrigger("playerChop");
		}


		//OnTriggerEnter2D отправляется, когда другой объект входит в триггерный коллайдер, прикрепленный к этому объекту (только для 2D-физики).
		private void OnTriggerEnter2D(Collider2D other)
		{
			//Проверьте, не сталкивался ли тег триггера с Exit.
			if (other.tag == "Exit")
			{
				//Вызовите функцию Restart, чтобы начать следующий уровень с задержкой restartLevelDelay (по умолчанию 1 секунда).
				Invoke("Restart", restartLevelDelay);

				//Отключите объект игрока, так как уровень закончился.
				enabled = false;
			}

			//Проверьте, не столкнулся ли тег триггера с едой.
			else if (other.tag == "Food")
			{
				//Добавьте очки за еду к текущему количеству еды игроков.
				food += pointsPerFood;

				//Обновите foodText, чтобы представить текущую сумму и уведомить игрока о том, что он набрал очки.
				foodText.text = "+" + pointsPerFood + " Еды: " + food;

				//Вызовите функцию RandomizeSfx SoundManager и передайте два звука еды, чтобы выбрать один из них для воспроизведения звукового эффекта еды.
				SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

				//Отключить пищевой объект, с которым столкнулся игрок.
				other.gameObject.SetActive(false);
			}

			//Проверьте, сталкивался ли тег триггера с Soda.
			else if (other.tag == "Soda")
			{
				//Добавить очкиPerSoda к общему количеству очков еды игроков
				food += pointsPerSoda;

				//Обновите foodText, чтобы представить текущую сумму и уведомить игрока о том, что он набрал очки.
				foodText.text = "+" + pointsPerSoda + " Еды: " + food;

				//Вызовите функцию RandomizeSfx SoundManager и передайте два звука питья, чтобы выбрать один из них для воспроизведения звукового эффекта питья.
				SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

				//Отключите объект газировки, с которым столкнулся игрок.
				other.gameObject.SetActive(false);
			}
		}


		//Restart перезагружает сцену при вызове.
		private void Restart()
		{
			//Загрузите последнюю загруженную сцену, в данном случае Main, единственную сцену в игре. И мы загружаем его в «Одиночном» режиме, чтобы он заменил существующий.
			//и не загружать весь объект сцены в текущей сцене.
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}


		//LoseFood вызывается, когда противник атакует игрока.
		//Он принимает параметр loss, который указывает, сколько очков нужно потерять.
		public void LoseFood(int loss)
		{
			//Установите триггер для аниматора игрока на переход к анимации playerHit.
			animator.SetTrigger("playerHit");

			//Вычтите потерянные очки еды из общего количества игроков.
			food -= loss;

			//Обновите дисплей еды с новой суммой.
			foodText.text = "-" + loss + " Еды: " + food;

			//Проверьте, не закончилась ли игра.
			CheckIfGameOver();
		}


		//CheckIfGameOver проверяет, закончились ли у игрока очки еды, и если да, то завершает игру.
		private void CheckIfGameOver()
		{
			//Проверьте, меньше или равно нулю общее количество очков еды.
			if (food <= 0)
			{
				//Вызовите функцию PlaySingle SoundManager и передайте ей gameOverSound в качестве аудиоклипа для воспроизведения.
				SoundManager.instance.PlaySingle(gameOverSound);

				//Остановите фоновую музыку.Остановите фоновую музыку.
				SoundManager.instance.musicSource.Stop();

				//Вызовите функцию GameOver в GameManager.
				GameManager.instance.GameOver();
			}
		}
	}
}

