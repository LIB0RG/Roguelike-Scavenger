using UnityEngine;
using System.Collections;

//Enemy наследуется от MovingObject, нашего базового класса для движущихся объектов, Player также наследуется от него.
public class Enemy : MovingObject
{
    //Количество очков еды, которое нужно вычесть у игрока при атаке.
    public int playerDamage;

    //Переменная типа Animator для хранения ссылки на компонент Animator противника.
    private Animator animator;
    //Трансформируйтесь, чтобы попытаться двигаться к каждому повороту.
    private Transform target;
    //Логическое значение, определяющее, должен ли противник пропускать ход или двигаться в этом ходу.
    private bool skipMove;
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;


    //Start переопределяет виртуальную функцию Start базового класса.
    protected override void Start()
    {
        //Зарегистрируйте этого врага в нашем экземпляре GameManager, добавив его в список объектов Enemy.
        //Это позволяет GameManager выдавать команды движения.
        GameManager.instance.AddEnemyToList(this);

        //Получите и сохраните ссылку на прикрепленный компонент Animator.
        animator = GetComponent<Animator>();

        //Найдите Player GameObject, используя его тег, и сохраните ссылку на его компонент преобразования.
        target = GameObject.FindGameObjectWithTag("Player").transform;

        //Вызовите функцию запуска нашего базового класса MovingObject.
        base.Start();
    }


    //Переопределите функцию AttemptMove объекта MovingObject, чтобы включить функции, необходимые врагу для пропуска ходов.
    //См. комментарии в MovingObject для получения дополнительной информации о том, как работает базовая функция AttemptMove.
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //Проверьте, верно ли значение skipMove, если да, установите его в false и пропустите этот ход.
        if (skipMove)
        {
            skipMove = false;
            return;

        }

        //Вызовите функцию AttemptMove из MovingObject.
        base.AttemptMove<T>(xDir, yDir);

        //Теперь, когда враг сдвинулся, установите для skipMove значение true, чтобы пропустить следующий ход.
        skipMove = true;
    }


    //MoveEnemy вызывается GameManger каждый ход, чтобы сказать каждому врагу попытаться двигаться к игроку.
    public void MoveEnemy()
    {
        //Объявите переменные для направлений перемещения по осям X и Y в диапазоне от -1 до 1.
        //Эти значения позволяют нам выбирать между сторонами света: вверх, вниз, влево и вправо.
        int xDir = 0;
        int yDir = 0;

        //Если разница в позициях приблизительно равна нулю (эпсилон), выполните следующие действия:
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

            //Если координата y положения цели (игрока) больше, чем координата y положения этого врага, установите направление y 1 (для движения вверх).
            //Если нет, установите его на -1 (для перемещения вниз).
            yDir = target.position.y > transform.position.y ? 1 : -1;

        //Если разница в позициях не равна приблизительно нулю (эпсилон), выполните следующие действия:
        else
            //Проверьте, больше ли позиция x цели, чем позиция x врага, если это так, установите направление x на 1 (движение вправо), если не установлено на -1 (движение влево).
            xDir = target.position.x > transform.position.x ? 1 : -1;

        //Вызовите функцию AttemptMove и передайте общий параметр Player, поскольку враг движется и ожидает возможной встречи с игроком.
        AttemptMove<Player>(xDir, yDir);
    }


    //OnCantMove вызывается, если враг пытается переместиться в пространство, занятое игроком, он переопределяет функцию OnCantMove MovingObject.
    //и принимает общий параметр T, который мы используем для передачи компонента, с которым мы ожидаем столкнуться, в данном случае Player
    protected override void OnCantMove<T>(T component)
    {
        //Объявите hitPlayer и установите его равным обнаруженному компоненту.
        Player hitPlayer = component as Player;

        //Вызовите функцию LoseFood для hitPlayer, передав ей playerDamage, количество очков еды, которое нужно вычесть.
        hitPlayer.LoseFood(playerDamage);

        //Установите триггер атаки аниматора, чтобы вызвать анимацию атаки врага.
        animator.SetTrigger("enemyAttack");

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);

    }
}