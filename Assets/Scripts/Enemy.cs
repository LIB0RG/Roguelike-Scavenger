using UnityEngine;
using System.Collections;

//Enemy ����������� �� MovingObject, ������ �������� ������ ��� ���������� ��������, Player ����� ����������� �� ����.
public class Enemy : MovingObject
{
    //���������� ����� ���, ������� ����� ������� � ������ ��� �����.
    public int playerDamage;

    //���������� ���� Animator ��� �������� ������ �� ��������� Animator ����������.
    private Animator animator;
    //�����������������, ����� ���������� ��������� � ������� ��������.
    private Transform target;
    //���������� ��������, ������������, ������ �� ��������� ���������� ��� ��� ��������� � ���� ����.
    private bool skipMove;
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;


    //Start �������������� ����������� ������� Start �������� ������.
    protected override void Start()
    {
        //��������������� ����� ����� � ����� ���������� GameManager, ������� ��� � ������ �������� Enemy.
        //��� ��������� GameManager �������� ������� ��������.
        GameManager.instance.AddEnemyToList(this);

        //�������� � ��������� ������ �� ������������� ��������� Animator.
        animator = GetComponent<Animator>();

        //������� Player GameObject, ��������� ��� ���, � ��������� ������ �� ��� ��������� ��������������.
        target = GameObject.FindGameObjectWithTag("Player").transform;

        //�������� ������� ������� ������ �������� ������ MovingObject.
        base.Start();
    }


    //�������������� ������� AttemptMove ������� MovingObject, ����� �������� �������, ����������� ����� ��� �������� �����.
    //��. ����������� � MovingObject ��� ��������� �������������� ���������� � ���, ��� �������� ������� ������� AttemptMove.
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //���������, ����� �� �������� skipMove, ���� ��, ���������� ��� � false � ���������� ���� ���.
        if (skipMove)
        {
            skipMove = false;
            return;

        }

        //�������� ������� AttemptMove �� MovingObject.
        base.AttemptMove<T>(xDir, yDir);

        //������, ����� ���� ���������, ���������� ��� skipMove �������� true, ����� ���������� ��������� ���.
        skipMove = true;
    }


    //MoveEnemy ���������� GameManger ������ ���, ����� ������� ������� ����� ���������� ��������� � ������.
    public void MoveEnemy()
    {
        //�������� ���������� ��� ����������� ����������� �� ���� X � Y � ��������� �� -1 �� 1.
        //��� �������� ��������� ��� �������� ����� ��������� �����: �����, ����, ����� � ������.
        int xDir = 0;
        int yDir = 0;

        //���� ������� � �������� �������������� ����� ���� (�������), ��������� ��������� ��������:
        if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

            //���� ���������� y ��������� ���� (������) ������, ��� ���������� y ��������� ����� �����, ���������� ����������� y 1 (��� �������� �����).
            //���� ���, ���������� ��� �� -1 (��� ����������� ����).
            yDir = target.position.y > transform.position.y ? 1 : -1;

        //���� ������� � �������� �� ����� �������������� ���� (�������), ��������� ��������� ��������:
        else
            //���������, ������ �� ������� x ����, ��� ������� x �����, ���� ��� ���, ���������� ����������� x �� 1 (�������� ������), ���� �� ����������� �� -1 (�������� �����).
            xDir = target.position.x > transform.position.x ? 1 : -1;

        //�������� ������� AttemptMove � ��������� ����� �������� Player, ��������� ���� �������� � ������� ��������� ������� � �������.
        AttemptMove<Player>(xDir, yDir);
    }


    //OnCantMove ����������, ���� ���� �������� ������������� � ������������, ������� �������, �� �������������� ������� OnCantMove MovingObject.
    //� ��������� ����� �������� T, ������� �� ���������� ��� �������� ����������, � ������� �� ������� �����������, � ������ ������ Player
    protected override void OnCantMove<T>(T component)
    {
        //�������� hitPlayer � ���������� ��� ������ ������������� ����������.
        Player hitPlayer = component as Player;

        //�������� ������� LoseFood ��� hitPlayer, ������� �� playerDamage, ���������� ����� ���, ������� ����� �������.
        hitPlayer.LoseFood(playerDamage);

        //���������� ������� ����� ���������, ����� ������� �������� ����� �����.
        animator.SetTrigger("enemyAttack");

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);

    }
}