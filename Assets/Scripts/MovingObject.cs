using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ключевое слово abstract позволяет создавать неполные
//классы и члены классов, которые должны быть реализованы в производном классе.
public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;       //Время, которое потребуется объекту для перемещения, в секундах.
    public LayerMask blockingLayer;     //Слой, на котором будет проверяться коллизия.


    private BoxCollider2D boxCollider;  //Компонент BoxCollider2D, прикрепленный к этому объекту.
    private Rigidbody2D rb2D;           //Компонент Rigidbody2D, прикрепленный к этому объекту.
    private float inverseMoveTime;      //Используется для повышения эффективности движения.

    //Новая строчка - проверяем, движется ли в данный момент персонаж игрока.
    private bool isMoving;

    //Защищенные виртуальные функции могут быть переопределены путем наследования классов.
    protected virtual void Start()
    {
        //Получить ссылку компонента на BoxCollider2D этого объекта
        boxCollider = GetComponent<BoxCollider2D>();

        //Получить ссылку компонента на Rigidbody2D этого объекта
        rb2D = GetComponent<Rigidbody2D>();

        //Сохраняя обратную величину времени перемещения, мы можем использовать ее,
        //умножая вместо деления, это более эффективно.
        inverseMoveTime = 1f / moveTime;
    }


    //Move возвращает true, если он может двигаться, и false, если нет.
    //Move принимает параметры для направления x, направления y и RaycastHit2D для проверки столкновения.
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        //Сохранение начальной позиции для перемещения на основе текущей позиции преобразования объектов.
        Vector2 start = transform.position;

        // Вычислить конечное положение на основе параметров направления, переданных при вызове Move.
        Vector2 end = start + new Vector2(xDir, yDir);

        //Отключите boxCollider, чтобы линейная трансляция не сталкивалась с собственным
        //коллайдером этого объекта.
        boxCollider.enabled = false;

        //Проведите линию от начальной точки до конечной, проверяя коллизию на blockingLayer.
        hit = Physics2D.Linecast(start, end, blockingLayer);

        //Повторно включить boxCollider после прямой трансляции
        boxCollider.enabled = true;

        //Проверьте, не попало ли что-нибудь (тут добавил)
        if (hit.transform == null && !isMoving)
        {
            //Если ничего не произошло, запустите сопрограмму SmoothMovement,
            //передав ее в конце Vector2 в качестве пункта назначения.
            StartCoroutine(SmoothMovement(end));

            //Возвратите true, чтобы сказать, что перемещение было успешным
            return true;
        }

        //Если что-то было поражено, верните false, перемещение было неудачным.
        return false;
    }


    //Сопрограмма для перемещения юнитов из одной области в другую использует параметр end,
    //чтобы указать, куда двигаться.
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //новая
        isMoving = true;
        //Рассчитайте оставшееся расстояние для перемещения на основе квадрата величины разницы
        //между текущим положением и конечным параметром.
        //Квадратная величина используется вместо величины, потому что это дешевле в
        //вычислительном отношении.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //Хотя это расстояние больше очень небольшого значения (эпсилон, почти ноль):
        while (sqrRemainingDistance > float.Epsilon)
        {
            //новые
            rb2D.MovePosition(end);
            isMoving = false;


            //Найти новую позицию пропорционально ближе к концу на основе moveTime
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

            //Вызовите MovePosition для присоединенного Rigidbody2D и переместите его в
            //вычисленное положение.
            rb2D.MovePosition(newPostion);

            //Пересчитайте оставшееся расстояние после перемещения.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //Возврат и цикл до тех пор, пока sqrRemainingDistance не станет достаточно
            //близким к нулю, чтобы завершить функцию
            yield return null;
        }
    }


    //Ключевое слово virtual означает, что AttemptMove может
    //быть переопределен путем наследования классов с помощью ключевого слова override.
    //AttemptMove принимает общий параметр T, чтобы указать тип компонента, с которым, как мы ожидаем,
    //будет взаимодействовать наш юнит, если он заблокирован (игрок для врагов, стена для игрока).
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        //Hit будет хранить все, что попадет в линейную трансляцию при вызове Move.
        RaycastHit2D hit;

        //Установите для canMove значение true, если перемещение прошло успешно,
        //и значение false, если оно не удалось.
        bool canMove = Move(xDir, yDir, out hit);

        //Проверьте, не было ли ничего затронуто линейной трансляцией
        if (hit.transform == null)
            //Если ничего не произошло, вернитесь и не выполняйте дальнейший код.
            return;

        //Получить ссылку компонента на компонент типа T, прикрепленный к объекту, который был поражен
        T hitComponent = hit.transform.GetComponent<T>();

        //Если canMove имеет значение false, а hitComponent не равен нулю, это означает,
        //что MovingObject заблокирован и столкнулся с чем-то, с чем он может взаимодействовать.
        if (!canMove && hitComponent != null)

            //Вызовите функцию OnCantMove и передайте ей hitComponent в качестве параметра.
            OnCantMove(hitComponent);
    }


    //Абстрактный модификатор указывает на то,
    //что модифицируемая вещь имеет отсутствующую или неполную реализацию.
    //OnCantMove будет переопределен функциями в классах-наследниках.
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}