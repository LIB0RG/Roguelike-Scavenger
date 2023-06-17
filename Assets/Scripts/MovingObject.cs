using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�������� ����� abstract ��������� ��������� ��������
//������ � ����� �������, ������� ������ ���� ����������� � ����������� ������.
public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = 0.1f;       //�����, ������� ����������� ������� ��� �����������, � ��������.
    public LayerMask blockingLayer;     //����, �� ������� ����� ����������� ��������.


    private BoxCollider2D boxCollider;  //��������� BoxCollider2D, ������������� � ����� �������.
    private Rigidbody2D rb2D;           //��������� Rigidbody2D, ������������� � ����� �������.
    private float inverseMoveTime;      //������������ ��� ��������� ������������� ��������.

    //����� ������� - ���������, �������� �� � ������ ������ �������� ������.
    private bool isMoving;

    //���������� ����������� ������� ����� ���� �������������� ����� ������������ �������.
    protected virtual void Start()
    {
        //�������� ������ ���������� �� BoxCollider2D ����� �������
        boxCollider = GetComponent<BoxCollider2D>();

        //�������� ������ ���������� �� Rigidbody2D ����� �������
        rb2D = GetComponent<Rigidbody2D>();

        //�������� �������� �������� ������� �����������, �� ����� ������������ ��,
        //������� ������ �������, ��� ����� ����������.
        inverseMoveTime = 1f / moveTime;
    }


    //Move ���������� true, ���� �� ����� ���������, � false, ���� ���.
    //Move ��������� ��������� ��� ����������� x, ����������� y � RaycastHit2D ��� �������� ������������.
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        //���������� ��������� ������� ��� ����������� �� ������ ������� ������� �������������� ��������.
        Vector2 start = transform.position;

        // ��������� �������� ��������� �� ������ ���������� �����������, ���������� ��� ������ Move.
        Vector2 end = start + new Vector2(xDir, yDir);

        //��������� boxCollider, ����� �������� ���������� �� ������������ � �����������
        //����������� ����� �������.
        boxCollider.enabled = false;

        //��������� ����� �� ��������� ����� �� ��������, �������� �������� �� blockingLayer.
        hit = Physics2D.Linecast(start, end, blockingLayer);

        //�������� �������� boxCollider ����� ������ ����������
        boxCollider.enabled = true;

        //���������, �� ������ �� ���-������ (��� �������)
        if (hit.transform == null && !isMoving)
        {
            //���� ������ �� ���������, ��������� ����������� SmoothMovement,
            //������� �� � ����� Vector2 � �������� ������ ����������.
            StartCoroutine(SmoothMovement(end));

            //���������� true, ����� �������, ��� ����������� ���� ��������
            return true;
        }

        //���� ���-�� ���� ��������, ������� false, ����������� ���� ���������.
        return false;
    }


    //����������� ��� ����������� ������ �� ����� ������� � ������ ���������� �������� end,
    //����� �������, ���� ���������.
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //�����
        isMoving = true;
        //����������� ���������� ���������� ��� ����������� �� ������ �������� �������� �������
        //����� ������� ���������� � �������� ����������.
        //���������� �������� ������������ ������ ��������, ������ ��� ��� ������� �
        //�������������� ���������.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //���� ��� ���������� ������ ����� ���������� �������� (�������, ����� ����):
        while (sqrRemainingDistance > float.Epsilon)
        {
            //�����
            rb2D.MovePosition(end);
            isMoving = false;


            //����� ����� ������� ��������������� ����� � ����� �� ������ moveTime
            Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

            //�������� MovePosition ��� ��������������� Rigidbody2D � ����������� ��� �
            //����������� ���������.
            rb2D.MovePosition(newPostion);

            //������������ ���������� ���������� ����� �����������.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            //������� � ���� �� ��� ���, ���� sqrRemainingDistance �� ������ ����������
            //������� � ����, ����� ��������� �������
            yield return null;
        }
    }


    //�������� ����� virtual ��������, ��� AttemptMove �����
    //���� ������������� ����� ������������ ������� � ������� ��������� ����� override.
    //AttemptMove ��������� ����� �������� T, ����� ������� ��� ����������, � �������, ��� �� �������,
    //����� ����������������� ��� ����, ���� �� ������������ (����� ��� ������, ����� ��� ������).
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        //Hit ����� ������� ���, ��� ������� � �������� ���������� ��� ������ Move.
        RaycastHit2D hit;

        //���������� ��� canMove �������� true, ���� ����������� ������ �������,
        //� �������� false, ���� ��� �� �������.
        bool canMove = Move(xDir, yDir, out hit);

        //���������, �� ���� �� ������ ��������� �������� �����������
        if (hit.transform == null)
            //���� ������ �� ���������, ��������� � �� ���������� ���������� ���.
            return;

        //�������� ������ ���������� �� ��������� ���� T, ������������� � �������, ������� ��� �������
        T hitComponent = hit.transform.GetComponent<T>();

        //���� canMove ����� �������� false, � hitComponent �� ����� ����, ��� ��������,
        //��� MovingObject ������������ � ���������� � ���-��, � ��� �� ����� �����������������.
        if (!canMove && hitComponent != null)

            //�������� ������� OnCantMove � ��������� �� hitComponent � �������� ���������.
            OnCantMove(hitComponent);
    }


    //����������� ����������� ��������� �� ��,
    //��� �������������� ���� ����� ������������� ��� �������� ����������.
    //OnCantMove ����� ������������� ��������� � �������-�����������.
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}